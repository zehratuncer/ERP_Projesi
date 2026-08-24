using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Features.Reports.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.Reports.Queries.GetCategoryProfitability;

public record GetCategoryProfitabilityQuery : IRequest<ApiResponse<CategoryProfitabilityDto>>;

public class GetCategoryProfitabilityQueryHandler : IRequestHandler<GetCategoryProfitabilityQuery, ApiResponse<CategoryProfitabilityDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCategoryProfitabilityQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<CategoryProfitabilityDto>> Handle(GetCategoryProfitabilityQuery request, CancellationToken cancellationToken)
    {
        var products = await _context.Products
            .Where(p => !p.IsDeleted)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var saleItems = await _context.SaleItems
            .Include(si => si.Sale)
            .Include(si => si.Product)
            .Where(si => !si.IsDeleted && !si.Sale.IsDeleted)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Kategori bazlı ürün eşleme
        var productCategoryMap = products.ToDictionary(p => p.Id, p => DetermineCategory(p.Name, p.Code));

        var categories = new[]
        {
            "Kağıt & Fotokopi Grubu",
            "Yazım Gereçleri",
            "Defter & Bloknot",
            "Dosyalama & Arşiv",
            "Ofis & Masaüstü Gereçleri",
            "Sanatsal & Hobi",
            "Genel Kırtasiye"
        };

        var categoryStats = new List<CategoryProfitabilityItemDto>();

        foreach (var categoryName in categories)
        {
            var categoryProducts = products.Where(p => productCategoryMap[p.Id] == categoryName).ToList();
            var categoryProductIds = categoryProducts.Select(p => p.Id).ToHashSet();

            var categorySaleItems = saleItems.Where(si => categoryProductIds.Contains(si.ProductId)).ToList();

            int productCount = categoryProducts.Count;
            int totalUnitsSold = categorySaleItems.Sum(si => si.Quantity);
            decimal totalRevenue = categorySaleItems.Sum(si => si.TotalPrice);

            // Tahmini maliyet: Satış fiyatının %65'i (ortalama %35 brüt kâr marjı)
            decimal estimatedCost = totalRevenue > 0 
                ? Math.Round(totalRevenue * 0.65m, 2) 
                : 0m;

            decimal grossProfit = totalRevenue - estimatedCost;

            double profitMargin = totalRevenue > 0 
                ? Math.Round((double)(grossProfit / totalRevenue) * 100.0, 1) 
                : 35.0; // Varsayılan hedef kâr marjı %35

            decimal currentStockValuation = categoryProducts.Sum(p => p.CurrentStock * p.UnitPrice);

            categoryStats.Add(new CategoryProfitabilityItemDto
            {
                CategoryName = categoryName,
                ProductCount = productCount,
                TotalUnitsSold = totalUnitsSold,
                TotalRevenue = totalRevenue,
                EstimatedCost = estimatedCost,
                GrossProfit = grossProfit,
                ProfitMarginPercentage = profitMargin,
                CurrentStockValue = currentStockValuation
            });
        }

        categoryStats = categoryStats
            .OrderByDescending(c => c.TotalRevenue)
            .ThenByDescending(c => c.CurrentStockValue)
            .ToList();

        decimal totalRev = categoryStats.Sum(c => c.TotalRevenue);
        decimal totalProfit = categoryStats.Sum(c => c.GrossProfit);
        decimal totalValuation = categoryStats.Sum(c => c.CurrentStockValue);
        double overallMargin = totalRev > 0 
            ? Math.Round((double)(totalProfit / totalRev) * 100.0, 1) 
            : 35.0;

        var result = new CategoryProfitabilityDto
        {
            TotalRevenue = totalRev,
            TotalGrossProfit = totalProfit,
            OverallProfitMargin = overallMargin,
            TotalInventoryValuation = totalValuation,
            Categories = categoryStats
        };

        return ApiResponse<CategoryProfitabilityDto>.Success(result, "Kategori kârlılık, maliyet ve stok değerleme analizi başarıyla tamamlandı.");
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
