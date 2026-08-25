using ERP.Application.Common.Interfaces;
using ERP.Application.Features.Notifications.DTOs;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace ERP.Infrastructure.Services.Notifications;

public class SignalRNotificationService : INotificationService
{
    private readonly IApplicationDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IEmailService _emailService;
    private readonly ILogger<SignalRNotificationService> _logger;

    public SignalRNotificationService(
        IApplicationDbContext context,
        IHubContext<NotificationHub> hubContext,
        IEmailService emailService,
        ILogger<SignalRNotificationService> logger)
    {
        _context = context;
        _hubContext = hubContext;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<NotificationDto> SendNotificationAsync(
        Guid? userId,
        string? roleName,
        string title,
        string message,
        NotificationType type,
        string? actionUrl = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Veritabanına kaydet
        var notification = new Notification
        {
            UserId = userId,
            RoleName = roleName,
            Title = title,
            Message = message,
            Type = type,
            IsRead = false,
            ActionUrl = actionUrl
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            RoleName = notification.RoleName,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type,
            IsRead = notification.IsRead,
            ActionUrl = notification.ActionUrl,
            CreatedDate = notification.CreatedDate,
            TimeAgo = "Az önce"
        };

        // 2. SignalR üzerinden gerçek zamanlı ilet
        try
        {
            if (userId.HasValue)
            {
                await _hubContext.Clients.Group($"User_{userId.Value}").SendAsync("ReceiveNotification", dto, cancellationToken);
            }
            else if (!string.IsNullOrEmpty(roleName))
            {
                await _hubContext.Clients.Group($"Role_{roleName}").SendAsync("ReceiveNotification", dto, cancellationToken);
            }
            else
            {
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", dto, cancellationToken);
            }

            _logger.LogInformation("SignalR bildirimi gönderildi: {Title} -> User: {UserId}, Role: {Role}", title, userId, roleName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SignalR bildirimi iletilirken hata oluştu: {Message}", ex.Message);
        }

        return dto;
    }

    public async Task SendEmailNotificationAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        await _emailService.SendEmailAsync(toEmail, subject, htmlBody, cancellationToken);
    }
}
