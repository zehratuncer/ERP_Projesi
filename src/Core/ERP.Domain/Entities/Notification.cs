using ERP.Domain.Common;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid? UserId { get; set; }
    public string? RoleName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.Info;
    public bool IsRead { get; set; } = false;
    public string? ActionUrl { get; set; }

    // Navigation Property
    public virtual User? User { get; set; }
}
