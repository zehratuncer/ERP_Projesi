using ERP.Application.Common.Models;
using ERP.Application.Features.Inventory.Commands.CreateStockMovement;
using ERP.Application.Features.Inventory.DTOs;
using ERP.Application.Features.Inventory.Queries.GetStockMovements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ERP.Domain.Constants;

namespace ERP.API.Controllers;

[Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
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

    /// <summary>
    /// Stok hareket geçmişini biçimlendirilmiş Excel (.xlsx) dosyası olarak dışa aktarır.
    /// </summary>
    [HttpGet("export-excel")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportStockMovementsExcel(
        [FromQuery] Guid? productId, 
        [FromQuery] ERP.Domain.Enums.TransactionType? transactionType,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var result = await Mediator.Send(new ERP.Application.Features.Export.Queries.ExportStockMovementsExcel.ExportStockMovementsExcelQuery(productId, transactionType, startDate, endDate));
        return File(result.FileBytes, result.ContentType, result.FileName);
    }

    /// <summary>
    /// Belirli bir stok hareketine ait Mal Kabul / Stok Fişi PDF belgesini üretir ve indirir.
    /// </summary>
    [HttpGet("transactions/{id:guid}/export-pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportStockReceiptPdf(Guid id)
    {
        var result = await Mediator.Send(new ERP.Application.Features.Export.Queries.ExportStockReceiptPdf.ExportStockReceiptPdfQuery(id));
        return File(result.FileBytes, result.ContentType, result.FileName);
    }
}

