using ERP.Domain.Common;

namespace ERP.Domain.Entities;

public class ApprovalWorkflow : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal MinAmount { get; set; } = 0m;
    public decimal? MaxAmount { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation Property
    public virtual ICollection<ApprovalStep> Steps { get; set; } = new List<ApprovalStep>();
}
