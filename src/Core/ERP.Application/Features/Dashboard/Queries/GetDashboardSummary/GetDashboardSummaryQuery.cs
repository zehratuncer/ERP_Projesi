using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Features.Dashboard.DTOs;
using ERP.Application.Features.Inventory.DTOs;
using ERP.Application.Features.Products.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.Dashboard.Queries.GetDashboardSummary;

public record GetDashboardSummaryQuery : IRequest<ApiResponse<DashboardSummaryDto>>;

public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, ApiResponse<DashboardSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDashboardSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<DashboardSummaryDto>> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        // 1. KPI Sayaçları
        var totalProductsCount = await _context.Products
            .CountAsync(p => !p.IsDeleted, cancellationToken);

        var criticalStockCount = await _context.Products
            .CountAsync(p => !p.IsDeleted && p.IsActive && p.CurrentStock <= p.MinStockLevel, cancellationToken);

        var totalSuppliersCount = await _context.Suppliers
            .CountAsync(s => !s.IsDeleted && s.IsActive, cancellationToken);

        var activeProducts = _context.Products
            .Where(p => !p.IsDeleted && p.IsActive);

        var totalInventoryQuantity = await activeProducts
            .SumAsync(p => p.CurrentStock, cancellationToken);

        var totalInventoryValue = await activeProducts
            .SumAsync(p => p.CurrentStock * p.UnitPrice, cancellationToken);

        // 2. Son 10 Stok Hareketi (Recent Stock Movements Stream)
        var recentMovements = await _context.InventoryTransactions
            .Where(t => !t.IsDeleted)
            .AsNoTracking()
            .OrderByDescending(t => t.TransactionDate)
            .Take(10)
            .Select(t => new StockMovementDto
            {
                Id = t.Id,
                ProductId = t.ProductId,
                ProductCode = t.Product.Code,
                ProductName = t.Product.Name,
                Unit = t.Product.Unit,
                Quantity = t.Quantity,
                TransactionType = t.TransactionType,
                Description = t.Description,
                TransactionDate = t.TransactionDate,
                UserName = t.User != null ? t.User.FullName : "Sistem"
            })
            .ToListAsync(cancellationToken);

        // 3. Kritik Stok Uyarı Listesi (Critical Stock Alerts - Acil tedarik gerekenler)
        var criticalAlerts = await _context.Products
            .Where(p => !p.IsDeleted && p.IsActive && p.CurrentStock <= p.MinStockLevel)
            .AsNoTracking()
            .OrderBy(p => p.CurrentStock)
            .Take(10)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                Unit = p.Unit,
                CurrentStock = p.CurrentStock,
                MinStockLevel = p.MinStockLevel,
                UnitPrice = p.UnitPrice,
                IsActive = p.IsActive,
                SupplierId = p.SupplierId,
                SupplierName = p.Supplier != null ? p.Supplier.Name : null,
                CreatedDate = p.CreatedDate,
                UpdatedDate = p.UpdatedDate
            })
            .ToListAsync(cancellationToken);

        var summary = new DashboardSummaryDto
        {
            TotalProductsCount = totalProductsCount,
            CriticalStockCount = criticalStockCount,
            TotalSuppliersCount = totalSuppliersCount,
            TotalInventoryQuantity = totalInventoryQuantity,
            TotalInventoryValue = totalInventoryValue,
            RecentStockMovements = recentMovements,
            CriticalStockAlerts = criticalAlerts
        };

        return ApiResponse<DashboardSummaryDto>.Success(summary, "Dashboard KPI ve özet verileri başarıyla getirildi.");
    }
}
