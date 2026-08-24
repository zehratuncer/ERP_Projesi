using ERP.Domain.Common;

namespace ERP.Domain.Entities;

public class PurchaseRequestItem : BaseEntity
{
    public Guid PurchaseRequestId { get; set; }
    public virtual PurchaseRequest PurchaseRequest { get; set; } = null!;

    public Guid ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;

    public int RequestedQuantity { get; set; }
    public string Unit { get; set; } = "Adet";
    public decimal EstimatedUnitPrice { get; set; } = 0.0m;
    public decimal EstimatedTotalPrice { get; set; } = 0.0m;
    public string? Notes { get; set; }
}
