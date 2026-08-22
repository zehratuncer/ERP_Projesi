using ERP.Domain.Enums;

namespace ERP.Application.Features.Inventory.DTOs;

public class StockMovementDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = "Adet";
    public int Quantity { get; set; }
    public TransactionType TransactionType { get; set; }
    public string TransactionTypeName => TransactionType switch
    {
        TransactionType.In => "Giriş",
        TransactionType.Out => "Çıkış",
        TransactionType.Adjustment => "Düzeltme",
        _ => "Bilinmiyor"
    };
    public string? Description { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? UserName { get; set; }
}
