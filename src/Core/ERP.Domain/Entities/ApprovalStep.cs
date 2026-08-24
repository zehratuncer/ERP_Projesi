using ERP.Domain.Common;

namespace ERP.Domain.Entities;

public class ApprovalStep : BaseEntity
{
    public Guid ApprovalWorkflowId { get; set; }
    public virtual ApprovalWorkflow ApprovalWorkflow { get; set; } = null!;

    public int StepNumber { get; set; }
    public string StepName { get; set; } = string.Empty;

    // Role bazlı onay yetkisi (Örn: Şube Müdürü veya Satın Alma Direktörü)
    public Guid? RoleId { get; set; }
    public virtual Role? Role { get; set; }

    // Özel kullanıcı bazlı onay yetkisi (Opsiyonel)
    public Guid? UserId { get; set; }
    public virtual User? User { get; set; }

    public bool IsRequired { get; set; } = true;
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
}
