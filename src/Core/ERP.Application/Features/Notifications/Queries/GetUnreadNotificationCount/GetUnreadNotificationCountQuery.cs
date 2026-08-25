using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Features.Notifications.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.Notifications.Queries.GetUnreadNotificationCount;

public record GetUnreadNotificationCountQuery : IRequest<ApiResponse<UnreadNotificationCountDto>>;

public class GetUnreadNotificationCountQueryHandler : IRequestHandler<GetUnreadNotificationCountQuery, ApiResponse<UnreadNotificationCountDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetUnreadNotificationCountQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<UnreadNotificationCountDto>> Handle(GetUnreadNotificationCountQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        var currentRole = _currentUserService.UserRole;

        var unreadCount = await _context.Notifications
            .AsNoTracking()
            .Where(n => !n.IsRead && (
                (n.UserId == null && string.IsNullOrEmpty(n.RoleName)) ||
                (currentUserId.HasValue && n.UserId == currentUserId.Value) ||
                (!string.IsNullOrEmpty(currentRole) && n.RoleName == currentRole)
            ))
            .CountAsync(cancellationToken);

        return ApiResponse<UnreadNotificationCountDto>.Success(new UnreadNotificationCountDto { Count = unreadCount });
    }
}
