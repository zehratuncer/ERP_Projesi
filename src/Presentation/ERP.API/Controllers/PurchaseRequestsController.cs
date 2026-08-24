using ERP.Application.Common.Models;
using ERP.Application.Features.PurchaseRequests.Commands.CancelPurchaseRequest;
using ERP.Application.Features.PurchaseRequests.Commands.CreatePurchaseRequest;
using ERP.Application.Features.PurchaseRequests.Commands.UpdatePurchaseRequest;
using ERP.Application.Features.PurchaseRequests.DTOs;
using ERP.Application.Features.PurchaseRequests.Queries.GetPurchaseRequestById;
using ERP.Application.Features.PurchaseRequests.Queries.GetPurchaseRequests;
using ERP.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[Authorize]
public class PurchaseRequestsController : BaseApiController
{
    /// <summary>
    /// Satın alma taleplerini filtreleme, departman, durum ve tarih aralığına göre listeler.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<PurchaseRequestListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPurchaseRequests(
        [FromQuery] RequestStatus? status,
        [FromQuery] string? department,
        [FromQuery] RequestPriority? priority,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] Guid? requesterUserId,
        [FromQuery] string? search)
    {
        var result = await Mediator.Send(new GetPurchaseRequestsQuery(status, department, priority, startDate, endDate, requesterUserId, search));
        return Ok(result);
    }

    /// <summary>
    /// ID'ye göre satın alma talebinin kalemleri ve detaylarını getirir.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPurchaseRequestById(Guid id)
    {
        var result = await Mediator.Send(new GetPurchaseRequestByIdQuery(id));
        return Ok(result);
    }

    /// <summary>
    /// Yeni satın alma talebi oluşturur (Taslak veya Onaya Gönderim).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PurchaseRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePurchaseRequest([FromBody] CreatePurchaseRequestCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Taslak veya onay bekleyen satın alma talebini günceller.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePurchaseRequest(Guid id, [FromBody] UpdatePurchaseRequestCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(ApiResponse<object>.Failure("URL'deki ID ile istek gövdesindeki ID uyuşmuyor."));
        }

        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Satın alma talebini iptal eder.
    /// </summary>
    [HttpDelete("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelPurchaseRequest(Guid id, [FromQuery] string? reason)
    {
        var result = await Mediator.Send(new CancelPurchaseRequestCommand(id, reason));
        return Ok(result);
    }
}
