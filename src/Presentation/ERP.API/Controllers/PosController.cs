using ERP.Application.Common.Models;
using ERP.Application.Features.Pos.Commands.CompleteSale;
using ERP.Application.Features.Pos.DTOs;
using ERP.Application.Features.Pos.Queries.GetDailyPosSummary;
using ERP.Application.Features.Pos.Queries.GetProductByBarcode;
using ERP.Application.Features.Pos.Queries.GetReceiptByNumber;
using ERP.Application.Features.Pos.Queries.GetSalesHistory;
using ERP.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ERP.Domain.Constants;

namespace ERP.API.Controllers;

[Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.Employee}")]
public class PosController : BaseApiController
{
    /// <summary>
    /// Barkod tabancasından veya klavyeden girilen ürün kodu / barkoda göre ürünü ve anlık stok durumunu getirir.
    /// </summary>
    [HttpGet("product/{barcodeOrCode}")]
    [ProducesResponseType(typeof(ApiResponse<PosProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductByBarcode(string barcodeOrCode)
    {
        var result = await Mediator.Send(new GetProductByBarcodeQuery(barcodeOrCode));
        return Ok(result);
    }

    /// <summary>
    /// Sepetteki ürünlerin satışını tamamlar, stoktan otomatik düşer ve satış fişi üretir.
    /// </summary>
    [HttpPost("complete-sale")]
    [ProducesResponseType(typeof(ApiResponse<SaleReceiptDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteSale([FromBody] CompleteSaleCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Fiş numarasına göre detaylı satış ve kalem dökümünü getirir.
    /// </summary>
    [HttpGet("receipt/{receiptNumber}")]
    [ProducesResponseType(typeof(ApiResponse<SaleReceiptDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReceiptByNumber(string receiptNumber)
    {
        var result = await Mediator.Send(new GetReceiptByNumberQuery(receiptNumber));
        return Ok(result);
    }

    /// <summary>
    /// Günlük kasa, ciro, ödeme yöntemi kırılımı ve en çok satan ürünler özetini getirir.
    /// </summary>
    [HttpGet("daily-summary")]
    [ProducesResponseType(typeof(ApiResponse<DailyPosSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDailySummary([FromQuery] DateTime? date)
    {
        var result = await Mediator.Send(new GetDailyPosSummaryQuery(date));
        return Ok(result);
    }

    /// <summary>
    /// Geçmiş satış fişlerini ve satış geçmişini tarih, ödeme yöntemi ve müşteri aramasına göre listeler.
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(ApiResponse<List<SaleHistoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSalesHistory(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] PaymentMethod? paymentMethod,
        [FromQuery] string? search)
    {
        var result = await Mediator.Send(new GetSalesHistoryQuery(startDate, endDate, paymentMethod, search));
        return Ok(result);
    }
}
