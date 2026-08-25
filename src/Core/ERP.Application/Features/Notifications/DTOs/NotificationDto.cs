using ERP.Domain.Enums;

namespace ERP.Application.Features.Notifications.DTOs;

public class NotificationDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? RoleName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string TypeName => Type.ToString();
    public bool IsRead { get; set; }
    public string? ActionUrl { get; set; }
    public DateTime CreatedDate { get; set; }
    public string TimeAgo { get; set; } = string.Empty;
}

public class UnreadNotificationCountDto
{
    public int Count { get; set; }
}
