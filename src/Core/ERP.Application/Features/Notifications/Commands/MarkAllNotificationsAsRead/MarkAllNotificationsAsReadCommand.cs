using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;

public record MarkAllNotificationsAsReadCommand : IRequest<ApiResponse<int>>;

public class MarkAllNotificationsAsReadCommandHandler : IRequestHandler<MarkAllNotificationsAsReadCommand, ApiResponse<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public MarkAllNotificationsAsReadCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<int>> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        var currentRole = _currentUserService.UserRole;

        var unreadNotifications = await _context.Notifications
            .Where(n => !n.IsRead && (
                (n.UserId == null && string.IsNullOrEmpty(n.RoleName)) ||
                (currentUserId.HasValue && n.UserId == currentUserId.Value) ||
                (!string.IsNullOrEmpty(currentRole) && n.RoleName == currentRole)
            ))
            .ToListAsync(cancellationToken);

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
        }

        var updatedCount = await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<int>.Success(unreadNotifications.Count, $"{unreadNotifications.Count} adet bildirim okundu olarak işaretlendi.");
    }
}
