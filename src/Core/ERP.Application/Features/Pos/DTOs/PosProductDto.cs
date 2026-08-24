namespace ERP.Application.Features.Pos.DTOs;

public class PosProductDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Unit { get; set; } = "Adet";
    public int CurrentStock { get; set; }
    public int MinStockLevel { get; set; }
    public decimal UnitPrice { get; set; }
    public bool IsLowStock => CurrentStock <= MinStockLevel;
    public bool IsActive { get; set; }
    public string? SupplierName { get; set; }
}
