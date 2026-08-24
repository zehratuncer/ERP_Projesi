using ERP.Domain.Enums;

namespace ERP.Application.Features.Pos.DTOs;

public class SaleHistoryDto
{
    public Guid Id { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
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
    public int ItemCount { get; set; }
}
