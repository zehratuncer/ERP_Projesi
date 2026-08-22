using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Features.Products.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.Suppliers.Queries.GetSupplierProducts;

public record GetSupplierProductsQuery(Guid SupplierId) : IRequest<ApiResponse<List<ProductDto>>>;

public class GetSupplierProductsQueryHandler : IRequestHandler<GetSupplierProductsQuery, ApiResponse<List<ProductDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetSupplierProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<ProductDto>>> Handle(GetSupplierProductsQuery request, CancellationToken cancellationToken)
    {
        var supplier = await _context.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.SupplierId && !s.IsDeleted, cancellationToken);

        if (supplier == null)
        {
            throw new NotFoundException("Tedarikçi", request.SupplierId);
        }

        var products = await _context.Products
            .Where(p => p.SupplierId == request.SupplierId && !p.IsDeleted)
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedDate)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                Unit = p.Unit,
                CurrentStock = p.CurrentStock,
                MinStockLevel = p.MinStockLevel,
                UnitPrice = p.UnitPrice,
                IsActive = p.IsActive,
                SupplierId = p.SupplierId,
                SupplierName = supplier.Name,
                CreatedDate = p.CreatedDate,
                UpdatedDate = p.UpdatedDate
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<List<ProductDto>>.Success(products, $"'{supplier.Name}' firmasına ait {products.Count} ürün listelendi.");
    }
}
