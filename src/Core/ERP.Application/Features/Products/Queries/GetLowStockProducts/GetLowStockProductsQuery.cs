using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Features.Products.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.Products.Queries.GetLowStockProducts;

public record GetLowStockProductsQuery : IRequest<ApiResponse<List<ProductDto>>>;

public class GetLowStockProductsQueryHandler : IRequestHandler<GetLowStockProductsQuery, ApiResponse<List<ProductDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetLowStockProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<ProductDto>>> Handle(GetLowStockProductsQuery request, CancellationToken cancellationToken)
    {
        var lowStockProducts = await _context.Products
            .Where(p => !p.IsDeleted && p.IsActive && p.CurrentStock <= p.MinStockLevel)
            .OrderBy(p => p.CurrentStock)
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
                SupplierName = p.Supplier != null ? p.Supplier.Name : null,
                CreatedDate = p.CreatedDate,
                UpdatedDate = p.UpdatedDate
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<List<ProductDto>>.Success(lowStockProducts, $"Kritik stok seviyesindeki {lowStockProducts.Count} ürün listelendi.");
    }
}
