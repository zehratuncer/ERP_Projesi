using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Features.Products.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.Products.Queries.GetProducts;

public record GetProductsQuery(string? Search = null, bool? OnlyActive = null) : IRequest<ApiResponse<List<ProductDto>>>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, ApiResponse<List<ProductDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Products
            .Where(p => !p.IsDeleted)
            .AsNoTracking();

        if (request.OnlyActive.HasValue && request.OnlyActive.Value)
        {
            query = query.Where(p => p.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(p => p.Code.ToLower().Contains(search) || p.Name.ToLower().Contains(search));
        }

        var products = await query
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
                SupplierName = p.Supplier != null ? p.Supplier.Name : null,
                CreatedDate = p.CreatedDate,
                UpdatedDate = p.UpdatedDate
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<List<ProductDto>>.Success(products);
    }
}
