using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Features.Notifications.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.Notifications.Queries.GetNotifications;

public record GetNotificationsQuery(int Limit = 20) : IRequest<ApiResponse<List<NotificationDto>>>;

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, ApiResponse<List<NotificationDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetNotificationsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<List<NotificationDto>>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        var currentRole = _currentUserService.UserRole;

        var query = _context.Notifications
            .AsNoTracking()
            .Where(n =>
                (n.UserId == null && string.IsNullOrEmpty(n.RoleName)) ||
                (currentUserId.HasValue && n.UserId == currentUserId.Value) ||
                (!string.IsNullOrEmpty(currentRole) && n.RoleName == currentRole)
            )
            .OrderByDescending(n => n.CreatedDate)
            .Take(request.Limit > 0 ? request.Limit : 20);

        var notifications = await query
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                UserId = n.UserId,
                RoleName = n.RoleName,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                IsRead = n.IsRead,
                ActionUrl = n.ActionUrl,
                CreatedDate = n.CreatedDate,
                TimeAgo = CalculateTimeAgo(n.CreatedDate)
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<List<NotificationDto>>.Success(notifications);
    }

    private static string CalculateTimeAgo(DateTime date)
    {
        var span = DateTime.UtcNow - date;
        if (span.TotalMinutes < 1) return "Az önce";
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} dk önce";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours} saat önce";
        if (span.TotalDays < 30) return $"{(int)span.TotalDays} gün önce";
        return date.ToString("dd.MM.yyyy");
    }
}
