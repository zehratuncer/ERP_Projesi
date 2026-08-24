using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Features.Reports.DTOs;
using ERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.Reports.Queries.GetDeadStock;

public record GetDeadStockQuery(int InactiveDays = 90) : IRequest<ApiResponse<DeadStockDto>>;

public class GetDeadStockQueryHandler : IRequestHandler<GetDeadStockQuery, ApiResponse<DeadStockDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDeadStockQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<DeadStockDto>> Handle(GetDeadStockQuery request, CancellationToken cancellationToken)
    {
        int days = request.InactiveDays > 0 ? request.InactiveDays : 90;
        var thresholdDate = DateTime.UtcNow.AddDays(-days);

        // Mevcut stoğu olan ürünler
        var productsInStock = await _context.Products
            .Where(p => !p.IsDeleted && p.IsActive && p.CurrentStock > 0)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Son X gündeki stok çıkış hareketleri
        var recentOutboundTransactions = await _context.InventoryTransactions
            .Where(t => !t.IsDeleted && 
                        t.TransactionType == TransactionType.Out && 
                        t.TransactionDate >= thresholdDate)
            .Select(t => t.ProductId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Son X gündeki POS satışları
        var recentSoldProductIds = await _context.SaleItems
            .Include(si => si.Sale)
            .Where(si => !si.IsDeleted && !si.Sale.IsDeleted && si.Sale.SaleDate >= thresholdDate)
            .Select(si => si.ProductId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var activeProductIds = recentOutboundTransactions
            .Union(recentSoldProductIds)
            .ToHashSet();

        // En son hareket tarihini bulmak için tüm geçmiş çıkışlar
        var lastTransactions = await _context.InventoryTransactions
            .Where(t => !t.IsDeleted && t.TransactionType == TransactionType.Out)
            .GroupBy(t => t.ProductId)
            .Select(g => new { ProductId = g.Key, LastDate = g.Max(t => t.TransactionDate) })
            .ToDictionaryAsync(x => x.ProductId, x => x.LastDate, cancellationToken);

        var deadStockItems = new List<DeadStockItemDto>();

        foreach (var product in productsInStock)
        {
            if (!activeProductIds.Contains(product.Id))
            {
                lastTransactions.TryGetValue(product.Id, out var lastDate);
                DateTime effectiveLastDate = lastDate != default ? lastDate : product.CreatedDate;
                int inactiveDays = (int)(DateTime.UtcNow - effectiveLastDate).TotalDays;

                decimal tiedUpValue = product.CurrentStock * product.UnitPrice;
                string category = DetermineCategory(product.Name, product.Code);

                string riskLevel = inactiveDays >= 180 || tiedUpValue >= 5000 
                    ? "Kritik" 
                    : (inactiveDays >= 120 ? "Yüksek Risk" : "Orta Risk");

                deadStockItems.Add(new DeadStockItemDto
                {
                    ProductId = product.Id,
                    ProductCode = product.Code,
                    ProductName = product.Name,
                    Category = category,
                    CurrentStock = product.CurrentStock,
                    Unit = product.Unit,
                    UnitPrice = product.UnitPrice,
                    TotalTiedUpValue = tiedUpValue,
                    LastMovementDate = effectiveLastDate,
                    DaysInactive = inactiveDays,
                    RiskLevel = riskLevel
                });
            }
        }

        deadStockItems = deadStockItems
            .OrderByDescending(d => d.TotalTiedUpValue)
            .ThenByDescending(d => d.DaysInactive)
            .ToList();

        var result = new DeadStockDto
        {
            InactiveDaysThreshold = days,
            TotalDeadStockCount = deadStockItems.Count,
            TotalDeadStockQuantity = deadStockItems.Sum(d => d.CurrentStock),
            TotalTiedUpCapital = deadStockItems.Sum(d => d.TotalTiedUpValue),
            DeadStockItems = deadStockItems
        };

        return ApiResponse<DeadStockDto>.Success(result, $"Son {days} gündür hareketsiz olan {deadStockItems.Count} adet ölü/atıl kırtasiye stoğu tespit edildi.");
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
