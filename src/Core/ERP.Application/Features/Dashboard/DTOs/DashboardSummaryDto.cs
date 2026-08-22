using ERP.Application.Features.Inventory.DTOs;
using ERP.Application.Features.Products.DTOs;

namespace ERP.Application.Features.Dashboard.DTOs;

public class DashboardSummaryDto
{
    public int TotalProductsCount { get; set; }
    public int CriticalStockCount { get; set; }
    public int TotalSuppliersCount { get; set; }
    public int TotalInventoryQuantity { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public List<StockMovementDto> RecentStockMovements { get; set; } = new();
    public List<ProductDto> CriticalStockAlerts { get; set; } = new();
}
