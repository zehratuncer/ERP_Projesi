using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Features.Reports.DTOs;
using ERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.Reports.Queries.GetStockTurnoverRate;

public record GetStockTurnoverRateQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int TopN = 10
) : IRequest<ApiResponse<StockTurnoverDto>>;

public class GetStockTurnoverRateQueryHandler : IRequestHandler<GetStockTurnoverRateQuery, ApiResponse<StockTurnoverDto>>
{
    private readonly IApplicationDbContext _context;

    public GetStockTurnoverRateQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<StockTurnoverDto>> Handle(GetStockTurnoverRateQuery request, CancellationToken cancellationToken)
    {
        var products = await _context.Products
            .Where(p => !p.IsDeleted)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var salesQuery = _context.SaleItems
            .Include(si => si.Sale)
            .Where(si => !si.IsDeleted && !si.Sale.IsDeleted)
            .AsNoTracking();

        if (request.StartDate.HasValue)
        {
            var start = DateTime.SpecifyKind(request.StartDate.Value.Date, DateTimeKind.Utc);
            salesQuery = salesQuery.Where(si => si.Sale.SaleDate >= start);
        }

        if (request.EndDate.HasValue)
        {
            var end = DateTime.SpecifyKind(request.EndDate.Value.Date.AddDays(1), DateTimeKind.Utc);
            salesQuery = salesQuery.Where(si => si.Sale.SaleDate < end);
        }

        var saleItems = await salesQuery.ToListAsync(cancellationToken);

        // Satışları ürün bazında grupla
        var salesByProduct = saleItems
            .GroupBy(si => si.ProductId)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    TotalSoldQuantity = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.TotalPrice)
                });

        var productTurnoverList = new List<ProductTurnoverItemDto>();

        foreach (var product in products)
        {
            salesByProduct.TryGetValue(product.Id, out var saleStats);

            int soldQty = saleStats?.TotalSoldQuantity ?? 0;
            decimal revenue = saleStats?.TotalRevenue ?? 0m;

            // Devir Hızı = Satılan Miktar / (Mevcut Stok + 1)
            // Eğer hiç satılmadıysa 0
            double avgStock = Math.Max(product.CurrentStock, 1);
            double turnoverRate = Math.Round((double)soldQty / avgStock, 2);

            // Stok Tüketim Süresi (Gün): (Mevcut Stok / Günlük Satış)
            double dailySold = soldQty > 0 ? (double)soldQty / 30.0 : 0.01;
            double daysToSell = Math.Round(product.CurrentStock / dailySold, 1);

            string category = DetermineCategory(product.Name, product.Code);
            string velocity = turnoverRate >= 1.5 ? "Hızlı" : (turnoverRate >= 0.5 ? "Normal" : "Yavaş");

            productTurnoverList.Add(new ProductTurnoverItemDto
            {
                ProductId = product.Id,
                ProductCode = product.Code,
                ProductName = product.Name,
                Category = category,
                CurrentStock = product.CurrentStock,
                TotalSoldQuantity = soldQty,
                TotalRevenue = revenue,
                TurnoverRate = turnoverRate,
                DaysToSellOut = daysToSell,
                VelocityCategory = velocity
            });
        }

        // Kategori bazında gruplama
        var categoryTurnover = productTurnoverList
            .GroupBy(p => p.Category)
            .Select(g => new CategoryTurnoverDto
            {
                Category = g.Key,
                TotalSoldQuantity = g.Sum(x => x.TotalSoldQuantity),
                CurrentStock = g.Sum(x => x.CurrentStock),
                TotalSalesAmount = g.Sum(x => x.TotalRevenue),
                TurnoverRate = Math.Round(g.Average(x => x.TurnoverRate), 2)
            })
            .OrderByDescending(c => c.TotalSoldQuantity)
            .ToList();

        var topFastMoving = productTurnoverList
            .OrderByDescending(p => p.TotalSoldQuantity)
            .ThenByDescending(p => p.TurnoverRate)
            .Take(request.TopN)
            .ToList();

        var topSlowMoving = productTurnoverList
            .Where(p => p.CurrentStock > 0)
            .OrderBy(p => p.TurnoverRate)
            .ThenBy(p => p.TotalSoldQuantity)
            .Take(request.TopN)
            .ToList();

        int totalItemsSold = productTurnoverList.Sum(p => p.TotalSoldQuantity);
        decimal totalRevenue = productTurnoverList.Sum(p => p.TotalRevenue);
        double overallTurnover = productTurnoverList.Any() 
            ? Math.Round(productTurnoverList.Average(p => p.TurnoverRate), 2) 
            : 0;
        double avgDays = productTurnoverList.Any() 
            ? Math.Round(productTurnoverList.Average(p => p.DaysToSellOut), 1) 
            : 0;

        var result = new StockTurnoverDto
        {
            OverallTurnoverRate = overallTurnover,
            AverageDaysToSell = avgDays,
            TotalItemsSold = totalItemsSold,
            TotalSalesRevenue = totalRevenue,
            TopFastMovingProducts = topFastMoving,
            TopSlowMovingProducts = topSlowMoving,
            TurnoverByCategory = categoryTurnover
        };

        return ApiResponse<StockTurnoverDto>.Success(result, "Stok devir hızı ve hareket analizi başarıyla hesaplandı.");
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
