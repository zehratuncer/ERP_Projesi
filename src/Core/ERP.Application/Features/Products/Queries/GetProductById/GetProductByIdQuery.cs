using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Features.Products.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.Products.Queries.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<ApiResponse<ProductDto>>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ApiResponse<ProductDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProductByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken);

        if (product == null)
        {
            throw new NotFoundException("Ürün", request.Id);
        }

        var dto = new ProductDto
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
            SupplierId = product.SupplierId,
            SupplierName = product.Supplier?.Name,
            CreatedDate = product.CreatedDate,
            UpdatedDate = product.UpdatedDate
        };

        return ApiResponse<ProductDto>.Success(dto);
    }
}
