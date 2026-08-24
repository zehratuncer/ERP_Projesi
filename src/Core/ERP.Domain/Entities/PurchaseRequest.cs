using ERP.Domain.Common;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities;

public class PurchaseRequest : BaseEntity
{
    public string RequestNumber { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public Guid? RequesterUserId { get; set; }
    public virtual User? RequesterUser { get; set; }

    public RequestPriority Priority { get; set; } = RequestPriority.Medium;
    public RequestStatus Status { get; set; } = RequestStatus.PendingApproval;

    public decimal TotalEstimatedAmount { get; set; } = 0.0m;
    public DateTime? RequiredDate { get; set; }
    public string? Note { get; set; }

    // Çok Kademeli Onay Takibi
    public int CurrentApprovalStep { get; set; } = 1;

    // Navigation properties
    public virtual ICollection<PurchaseRequestItem> Items { get; set; } = new List<PurchaseRequestItem>();
    public virtual ICollection<ApprovalHistory> ApprovalHistories { get; set; } = new List<ApprovalHistory>();
}

