using ERP.Domain.Common;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities;

public class InventoryTransaction : BaseEntity
{
    public Guid ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;

    public int Quantity { get; set; }
    public TransactionType TransactionType { get; set; }
    public string? Description { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    // İşlemi yapan kullanıcı (Audit)
    public Guid? UserId { get; set; }
    public virtual User? User { get; set; }
}
