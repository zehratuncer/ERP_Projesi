namespace ERP.Application.Features.Products.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Unit { get; set; } = "Adet";
    public int CurrentStock { get; set; }
    public int MinStockLevel { get; set; }
    public decimal UnitPrice { get; set; }
    public bool IsActive { get; set; }
    public bool IsLowStock => CurrentStock <= MinStockLevel;
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}
