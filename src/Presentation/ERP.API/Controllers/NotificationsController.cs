using ERP.Application.Common.Models;
using ERP.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;
using ERP.Application.Features.Notifications.Commands.MarkNotificationAsRead;
using ERP.Application.Features.Notifications.DTOs;
using ERP.Application.Features.Notifications.Queries.GetNotifications;
using ERP.Application.Features.Notifications.Queries.GetUnreadNotificationCount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[Authorize]
public class NotificationsController : BaseApiController
{
    /// <summary>
    /// Kullanıcıya ait bildirimleri listeler.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<NotificationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications([FromQuery] int limit = 20)
    {
        var result = await Mediator.Send(new GetNotificationsQuery(limit));
        return Ok(result);
    }

    /// <summary>
    /// Kullanıcının okunmamış bildirim sayısını döner.
    /// </summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(ApiResponse<UnreadNotificationCountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount()
    {
        var result = await Mediator.Send(new GetUnreadNotificationCountQuery());
        return Ok(result);
    }

    /// <summary>
    /// Belirtilen bildirimi okundu olarak işaretler.
    /// </summary>
    [HttpPut("{id}/read")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var result = await Mediator.Send(new MarkNotificationAsReadCommand(id));
        return Ok(result);
    }

    /// <summary>
    /// Kullanıcının tüm bildirimlerini okundu olarak işaretler.
    /// </summary>
    [HttpPut("read-all")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var result = await Mediator.Send(new MarkAllNotificationsAsReadCommand());
        return Ok(result);
    }
}
