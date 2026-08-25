using ERP.Application.Common.Models;
using ERP.Application.Features.Reports.DTOs;
using ERP.Application.Features.Reports.Queries.GetCategoryProfitability;
using ERP.Application.Features.Reports.Queries.GetDeadStock;
using ERP.Application.Features.Reports.Queries.GetSeasonalDemandTrends;
using ERP.Application.Features.Reports.Queries.GetStockTurnoverRate;
using ERP.Application.Features.Reports.Queries.GetSupplierPerformance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[Authorize]
[Route("api/reports")]
public class ReportsController : BaseApiController
{
    /// <summary>
    /// Kırtasiye ürünlerinin devir hızı, en hızlı tükenen ve yavaş hareket eden stokların analizini getirir.
    /// </summary>
    [HttpGet("stock-turnover")]
    [ProducesResponseType(typeof(ApiResponse<StockTurnoverDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStockTurnover(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int topN = 10)
    {
        var result = await Mediator.Send(new GetStockTurnoverRateQuery(startDate, endDate, topN));
        return Ok(result);
    }

    /// <summary>
    /// Okul açılış sezonu (Ağustos-Ekim), sınav ve ofis dönemlerine göre aylık talep trendlerini getirir.
    /// </summary>
    [HttpGet("seasonal-trends")]
    [ProducesResponseType(typeof(ApiResponse<SeasonalDemandTrendsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSeasonalDemandTrends([FromQuery] int? year)
    {
        var result = await Mediator.Send(new GetSeasonalDemandTrendsQuery(year));
        return Ok(result);
    }

    /// <summary>
    /// Son 90/180 günde hiç hareketi olmayan atıl/ölü kırtasiye stoklarını ve bağlanan sermayeyi listeler.
    /// </summary>
    [HttpGet("dead-stock")]
    [ProducesResponseType(typeof(ApiResponse<DeadStockDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeadStock([FromQuery] int inactiveDays = 90)
    {
        var result = await Mediator.Send(new GetDeadStockQuery(inactiveDays));
        return Ok(result);
    }

    /// <summary>
    /// Tedarikçilerin ortalama teslimat süresi, talep karşılama oranı ve güvenilirlik puanlarını listeler.
    /// </summary>
    [HttpGet("supplier-performance")]
    [ProducesResponseType(typeof(ApiResponse<SupplierPerformanceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSupplierPerformance()
    {
        var result = await Mediator.Send(new GetSupplierPerformanceQuery());
        return Ok(result);
    }

    /// <summary>
    /// Kategori bazında kâr marjı, ciro, tahmini maliyet ve toplam stok değerleme analizini getirir.
    /// </summary>
    [HttpGet("category-analytics")]
    [ProducesResponseType(typeof(ApiResponse<CategoryProfitabilityDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategoryAnalytics()
    {
        var result = await Mediator.Send(new GetCategoryProfitabilityQuery());
        return Ok(result);
    }

    /// <summary>
    /// Belirtilen analitik raporu biçimlendirilmiş Excel (.xlsx) dosyası olarak dışa aktarır.
    /// </summary>
    [HttpGet("{reportType}/export-excel")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportReportExcel(string reportType)
    {
        var result = await Mediator.Send(new ERP.Application.Features.Export.Queries.ExportReportExcel.ExportReportExcelQuery(reportType));
        return File(result.FileBytes, result.ContentType, result.FileName);
    }
}

