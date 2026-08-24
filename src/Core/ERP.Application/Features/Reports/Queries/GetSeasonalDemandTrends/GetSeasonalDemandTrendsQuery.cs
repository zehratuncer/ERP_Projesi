using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Features.Reports.DTOs;
using ERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ERP.Application.Features.Reports.Queries.GetSeasonalDemandTrends;

public record GetSeasonalDemandTrendsQuery(int? Year = null) : IRequest<ApiResponse<SeasonalDemandTrendsDto>>;

public class GetSeasonalDemandTrendsQueryHandler : IRequestHandler<GetSeasonalDemandTrendsQuery, ApiResponse<SeasonalDemandTrendsDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSeasonalDemandTrendsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<SeasonalDemandTrendsDto>> Handle(GetSeasonalDemandTrendsQuery request, CancellationToken cancellationToken)
    {
        int targetYear = request.Year ?? DateTime.UtcNow.Year;

        // Yıllık Stok Çıkışları ve Satışlar
        var inventoryTransactions = await _context.InventoryTransactions
            .Include(t => t.Product)
            .Where(t => !t.IsDeleted && 
                        t.TransactionType == TransactionType.Out && 
                        t.TransactionDate.Year == targetYear)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var sales = await _context.Sales
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
            .Where(s => !s.IsDeleted && s.SaleDate.Year == targetYear)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var trCulture = new CultureInfo("tr-TR");
        var monthlyTrends = new List<MonthlyDemandDto>();

        for (int month = 1; month <= 12; month++)
        {
            var monthTx = inventoryTransactions.Where(t => t.TransactionDate.Month == month).ToList();
            var monthSales = sales.Where(s => s.SaleDate.Month == month).ToList();

            int txQuantity = monthTx.Sum(t => t.Quantity);
            int saleQuantity = monthSales.SelectMany(s => s.Items).Sum(i => i.Quantity);
            int totalOutbound = Math.Max(txQuantity, saleQuantity);
            if (totalOutbound == 0 && (txQuantity > 0 || saleQuantity > 0))
            {
                totalOutbound = txQuantity + saleQuantity;
            }

            decimal totalRevenue = monthSales.Sum(s => s.FinalAmount);

            string seasonTag = month switch
            {
                >= 8 and <= 10 => "🎒 Okul Açılış Sezonu (Yoğun)",
                >= 1 and <= 3 => "📝 Sınav & Ara Dönem",
                >= 5 and <= 6 => "🎓 Yıl Sonu & Mezuniyet",
                _ => "🏢 Standart Ofis & Kurumsal"
            };

            var monthName = trCulture.DateTimeFormat.GetMonthName(month);
            // İlk harfi büyük yap
            monthName = char.ToUpper(monthName[0]) + monthName.Substring(1);

            monthlyTrends.Add(new MonthlyDemandDto
            {
                Month = month,
                MonthName = monthName,
                SeasonTag = seasonTag,
                TotalOutboundQuantity = totalOutbound,
                TotalSalesAmount = totalRevenue,
                TransactionCount = monthSales.Count + monthTx.Count
            });
        }

        // Kategori Sezonluk Dağılımı
        var allSaleItems = sales.SelectMany(s => s.Items.Select(i => new { Item = i, s.SaleDate })).ToList();
        var allProducts = await _context.Products.Where(p => !p.IsDeleted).AsNoTracking().ToListAsync(cancellationToken);
        var productCategories = allProducts.ToDictionary(p => p.Id, p => DetermineCategory(p.Name, p.Code));

        var categoryList = new[] 
        { 
            "Kağıt & Fotokopi Grubu", 
            "Yazım Gereçleri", 
            "Defter & Bloknot", 
            "Dosyalama & Arşiv", 
            "Ofis & Masaüstü Gereçleri", 
            "Sanatsal & Hobi", 
            "Genel Kırtasiye" 
        };

        var categoryBreakdown = new List<SeasonalCategoryTrendDto>();

        foreach (var cat in categoryList)
        {
            var catItems = allSaleItems
                .Where(x => productCategories.TryGetValue(x.Item.ProductId, out var c) && c == cat)
                .ToList();

            int schoolSeason = catItems.Where(x => x.SaleDate.Month >= 8 && x.SaleDate.Month <= 10).Sum(x => x.Item.Quantity);
            int examSeason = catItems.Where(x => x.SaleDate.Month >= 1 && x.SaleDate.Month <= 3).Sum(x => x.Item.Quantity);
            int officeRoutine = catItems.Where(x => x.SaleDate.Month < 1 || (x.SaleDate.Month > 3 && x.SaleDate.Month < 8) || x.SaleDate.Month > 10).Sum(x => x.Item.Quantity);
            int total = schoolSeason + examSeason + officeRoutine;

            string peak = "Standart Ofis";
            if (schoolSeason >= examSeason && schoolSeason >= officeRoutine) peak = "Okul Sezonu (Ağustos-Ekim)";
            else if (examSeason >= schoolSeason && examSeason >= officeRoutine) peak = "Sınav Dönemi (Ocak-Mart)";

            categoryBreakdown.Add(new SeasonalCategoryTrendDto
            {
                CategoryName = cat,
                SchoolSeasonSales = schoolSeason,
                ExamSeasonSales = examSeason,
                OfficeRoutineSales = officeRoutine,
                TotalSales = total,
                PeakSeason = peak
            });
        }

        var peakMonth = monthlyTrends.OrderByDescending(m => m.TotalOutboundQuantity).FirstOrDefault();
        double avgMonthly = monthlyTrends.Average(m => m.TotalOutboundQuantity);
        double seasonalityIndex = avgMonthly > 0 && peakMonth != null 
            ? Math.Round((double)peakMonth.TotalOutboundQuantity / avgMonthly, 2) 
            : 1.0;

        var result = new SeasonalDemandTrendsDto
        {
            Year = targetYear,
            PeakSeasonName = peakMonth != null ? $"{peakMonth.MonthName} ({peakMonth.SeasonTag})" : "Okul Açılış Sezonu (Ağustos - Ekim)",
            SeasonalityIndex = seasonalityIndex,
            MonthlyTrends = monthlyTrends,
            CategorySeasonalBreakdown = categoryBreakdown
        };

        return ApiResponse<SeasonalDemandTrendsDto>.Success(result, $"{targetYear} yılı kırtasiye sezonluk talep trendleri başarıyla analiz edildi.");
    }

    private static string DetermineCategory(string name, string code)
    {
        var text = (name + " " + code).ToLower();
        if (text.Contains("a4") || text.Contains("kağıt") || text.Contains("fotokopi") || text.Contains("karton"))
            return "Kağıt & Fotokopi Grubu";
        if (text.Contains("kalem") || text.Contains("tükenmez") || text.Contains("kurşun") || text.Contains("fosforlu") || text.Contains("silgi") || text.Contains("kalemtıraş"))
            return "Yazım Gereçleri";
        if (text.Contains("defter") || text.Contains("bloknot") || text.Contains("ajanda") || text.Contains("not"))
            return "Defter & Bloknot";
        if (text.Contains("klasör") || text.Contains("dosya") || text.Contains("fihrist") || text.Contains("poşet dosya"))
            return "Dosyalama & Arşiv";
        if (text.Contains("zımba") || text.Contains("delgeç") || text.Contains("makas") || text.Contains("bant") || text.Contains("yapıştırıcı"))
            return "Ofis & Masaüstü Gereçleri";
        if (text.Contains("boya") || text.Contains("fırça") || text.Contains("tuval") || text.Contains("pastel"))
            return "Sanatsal & Hobi";

        return "Genel Kırtasiye";
    }
}
