using ERP.Application.Common.Models;
using ERP.Application.Features.Products.DTOs;
using ERP.Application.Features.Suppliers.Commands.CreateSupplier;
using ERP.Application.Features.Suppliers.Commands.DeleteSupplier;
using ERP.Application.Features.Suppliers.Commands.UpdateSupplier;
using ERP.Application.Features.Suppliers.DTOs;
using ERP.Application.Features.Suppliers.Queries.GetSupplierById;
using ERP.Application.Features.Suppliers.Queries.GetSupplierProducts;
using ERP.Application.Features.Suppliers.Queries.GetSuppliers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ERP.Domain.Constants;

namespace ERP.API.Controllers;

[Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
public class SuppliersController : BaseApiController
{
    /// <summary>
    /// Tüm tedarikçileri arama ve aktiflik filtresi ile listeler.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<SupplierDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSuppliers([FromQuery] string? search, [FromQuery] bool? onlyActive)
    {
        var result = await Mediator.Send(new GetSuppliersQuery(search, onlyActive));
        return Ok(result);
    }

    /// <summary>
    /// Id'ye göre tek bir tedarikçinin detayını döner.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SupplierDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSupplierById(Guid id)
    {
        var result = await Mediator.Send(new GetSupplierByIdQuery(id));
        return Ok(result);
    }

    /// <summary>
    /// Seçili tedarikçinin sağladığı tüm ürünleri listeler.
    /// </summary>
    [HttpGet("{id:guid}/products")]
    [ProducesResponseType(typeof(ApiResponse<List<ProductDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSupplierProducts(Guid id)
    {
        var result = await Mediator.Send(new GetSupplierProductsQuery(id));
        return Ok(result);
    }

    /// <summary>
    /// Yeni tedarikçi ekler.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SupplierDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Mevcut tedarikçiyi günceller.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SupplierDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSupplier(Guid id, [FromBody] UpdateSupplierCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(ApiResponse<object>.Failure("URL'deki Id ile gövdedeki Id uyuşmuyor."));
        }

        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Tedarikçiyi siler (Soft-delete).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSupplier(Guid id)
    {
        var result = await Mediator.Send(new DeleteSupplierCommand(id));
        return Ok(result);
    }
}
