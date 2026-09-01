using ERP.Application.Common.Models;
using ERP.Application.Features.Products.Commands.CreateProduct;
using ERP.Application.Features.Products.Commands.DeleteProduct;
using ERP.Application.Features.Products.Commands.UpdateProduct;
using ERP.Application.Features.Products.DTOs;
using ERP.Application.Features.Products.Queries.GetLowStockProducts;
using ERP.Application.Features.Products.Queries.GetProductById;
using ERP.Application.Features.Products.Queries.GetProducts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ERP.Domain.Constants;

namespace ERP.API.Controllers;

[Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.Employee}")]
public class ProductsController : BaseApiController
{
    /// <summary>
    /// Tüm ürünleri filtreleme ve arama desteği ile listeler.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts([FromQuery] string? search, [FromQuery] bool? onlyActive)
    {
        var result = await Mediator.Send(new GetProductsQuery(search, onlyActive));
        return Ok(result);
    }

    /// <summary>
    /// Kritik stok seviyesinin altındaki ürünleri listeler.
    /// </summary>
    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(ApiResponse<List<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLowStockProducts()
    {
        var result = await Mediator.Send(new GetLowStockProductsQuery());
        return Ok(result);
    }

    /// <summary>
    /// Id'ye göre tek bir ürünün detayını döner.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var result = await Mediator.Send(new GetProductByIdQuery(id));
        return Ok(result);
    }

    /// <summary>
    /// Yeni ürün ekler.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Mevcut ürünü günceller.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(ApiResponse<object>.Failure("URL'deki Id ile gövdedeki Id uyuşmuyor."));
        }

        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Ürünü siler (Soft-delete).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var result = await Mediator.Send(new DeleteProductCommand(id));
        return Ok(result);
    }

    /// <summary>
    /// Ürün listesini biçimlendirilmiş Excel (.xlsx) dosyası olarak dışa aktarır.
    /// </summary>
    [HttpGet("export-excel")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportProductsExcel([FromQuery] string? search, [FromQuery] bool? isCriticalOnly)
    {
        var result = await Mediator.Send(new ERP.Application.Features.Export.Queries.ExportProductsExcel.ExportProductsExcelQuery(search, isCriticalOnly));
        return File(result.FileBytes, result.ContentType, result.FileName);
    }
}

