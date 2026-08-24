namespace ERP.Application.Features.PurchaseRequests.DTOs;

public class PurchaseRequestItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int MinStockLevel { get; set; }
    public int RequestedQuantity { get; set; }
    public string Unit { get; set; } = "Adet";
    public decimal EstimatedUnitPrice { get; set; }
    public decimal EstimatedTotalPrice { get; set; }
    public string? Notes { get; set; }
}
