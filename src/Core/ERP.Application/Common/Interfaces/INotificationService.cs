using ERP.Application.Features.Notifications.DTOs;
using ERP.Domain.Enums;

namespace ERP.Application.Common.Interfaces;

public interface INotificationService
{
    /// <summary>
    /// Hem veritabanına bildirim kaydeder hem de SignalR üzerinden hedef kullanıcı/rol veya herkese anlık iletir.
    /// </summary>
    Task<NotificationDto> SendNotificationAsync(
        Guid? userId,
        string? roleName,
        string title,
        string message,
        NotificationType type,
        string? actionUrl = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// E-posta bildirimi gönderir.
    /// </summary>
    Task SendEmailNotificationAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default);
}
