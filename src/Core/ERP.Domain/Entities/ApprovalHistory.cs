using ERP.Domain.Common;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities;

public class ApprovalHistory : BaseEntity
{
    public Guid PurchaseRequestId { get; set; }
    public virtual PurchaseRequest PurchaseRequest { get; set; } = null!;

    public Guid? ApproverUserId { get; set; }
    public virtual User? ApproverUser { get; set; }

    public int StepNumber { get; set; }
    public string StepName { get; set; } = string.Empty;

    public ApprovalAction Action { get; set; }
    public string? Comment { get; set; }
    public DateTime ActionDate { get; set; } = DateTime.UtcNow;
}
