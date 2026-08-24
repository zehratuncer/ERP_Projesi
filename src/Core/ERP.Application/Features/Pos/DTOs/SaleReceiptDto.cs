using ERP.Domain.Enums;

namespace ERP.Application.Features.Pos.DTOs;

public class SaleReceiptDto
{
    public Guid Id { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public Guid? CashierUserId { get; set; }
    public string CashierName { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string PaymentMethodName => PaymentMethod switch
    {
        PaymentMethod.Cash => "Nakit",
        PaymentMethod.CreditCard => "Kredi Kartı",
        PaymentMethod.Split => "Parçalı Ödeme",
        PaymentMethod.OnAccount => "Veresiye / Cari",
        _ => PaymentMethod.ToString()
    };
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public List<SaleItemDto> Items { get; set; } = new();
    public List<string> CriticalStockAlerts { get; set; } = new();
}
