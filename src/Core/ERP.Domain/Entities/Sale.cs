using ERP.Domain.Common;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities;

public class Sale : BaseEntity
{
    public string ReceiptNumber { get; set; } = string.Empty;
    public Guid? CashierUserId { get; set; }
    public virtual User? CashierUser { get; set; }

    public decimal TotalAmount { get; set; } = 0.0m;
    public decimal DiscountAmount { get; set; } = 0.0m;
    public decimal FinalAmount { get; set; } = 0.0m;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;
    public string? CustomerName { get; set; }

    // Navigation property
    public virtual ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
}
