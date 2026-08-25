namespace ERP.Application.Features.Export.DTOs;

public class ProductExportDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Unit { get; set; } = "Adet";
    public int CurrentStock { get; set; }
    public int MinStockLevel { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalStockValue { get; set; }
    public string? SupplierName { get; set; }
    public string Status { get; set; } = "Aktif";
}

public class StockMovementExportDto
{
    public DateTime TransactionDate { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty; // Giriş, Çıkış
    public int Quantity { get; set; }
    public string Unit { get; set; } = "Adet";
    public string? Description { get; set; }
    public string? UserName { get; set; }
}
