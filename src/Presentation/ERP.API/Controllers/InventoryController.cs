using ERP.Application.Common.Models;
using ERP.Application.Features.Inventory.Commands.CreateStockMovement;
using ERP.Application.Features.Inventory.DTOs;
using ERP.Application.Features.Inventory.Queries.GetStockMovements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[Authorize]
public class InventoryController : BaseApiController
{
    /// <summary>
    /// Stok giriş, çıkış veya düzeltme hareketi kaydeder (Atomik bakiye kontrolü yapar).
    /// </summary>
    [HttpPost("movement")]
    [ProducesResponseType(typeof(ApiResponse<StockMovementDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateStockMovement([FromBody] CreateStockMovementCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Stok hareket geçmişini döner (Opsiyonel olarak belirli bir ürüne göre filtrelenebilir).
    /// </summary>
    [HttpGet("movements")]
    [ProducesResponseType(typeof(ApiResponse<List<StockMovementDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStockMovements([FromQuery] Guid? productId, [FromQuery] int limit = 50)
    {
        var result = await Mediator.Send(new GetStockMovementsQuery(productId, limit));
        return Ok(result);
    }
}
