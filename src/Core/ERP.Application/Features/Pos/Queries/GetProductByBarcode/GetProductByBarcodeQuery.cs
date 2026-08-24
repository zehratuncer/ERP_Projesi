using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Features.Pos.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.Pos.Queries.GetProductByBarcode;

public record GetProductByBarcodeQuery(string BarcodeOrCode) : IRequest<ApiResponse<PosProductDto>>;

public class GetProductByBarcodeQueryHandler : IRequestHandler<GetProductByBarcodeQuery, ApiResponse<PosProductDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProductByBarcodeQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<PosProductDto>> Handle(GetProductByBarcodeQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.BarcodeOrCode))
        {
            throw new BusinessException("Barkod veya ürün kodu boş olamaz.");
        }

        var normalizedCode = request.BarcodeOrCode.Trim();

        var product = await _context.Products
            .Include(p => p.Supplier)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => !p.IsDeleted && p.Code.ToLower() == normalizedCode.ToLower(), cancellationToken);

        if (product == null)
        {
            throw new NotFoundException("Ürün Barkodu/Kodu", request.BarcodeOrCode);
        }

        var dto = new PosProductDto
        {
            Id = product.Id,
            Code = product.Code,
            Name = product.Name,
            Description = product.Description,
            Unit = product.Unit,
            CurrentStock = product.CurrentStock,
            MinStockLevel = product.MinStockLevel,
            UnitPrice = product.UnitPrice,
            IsActive = product.IsActive,
            SupplierName = product.Supplier?.Name
        };

        return ApiResponse<PosProductDto>.Success(dto);
    }
}
