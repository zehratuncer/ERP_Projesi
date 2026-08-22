using ERP.Application.Common.Models;
using ERP.Application.Features.Dashboard.DTOs;
using ERP.Application.Features.Dashboard.Queries.GetDashboardSummary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[Authorize]
public class DashboardController : BaseApiController
{
    /// <summary>
    /// Yönetici dashboard KPI sayaçlarını, son stok hareketlerini ve kritik stok uyarılarını döner.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<DashboardSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardSummary()
    {
        var result = await Mediator.Send(new GetDashboardSummaryQuery());
        return Ok(result);
    }
}
