using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Features.Suppliers.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.Suppliers.Queries.GetSuppliers;

public record GetSuppliersQuery(string? Search = null, bool? OnlyActive = null) : IRequest<ApiResponse<List<SupplierDto>>>;

public class GetSuppliersQueryHandler : IRequestHandler<GetSuppliersQuery, ApiResponse<List<SupplierDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetSuppliersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<SupplierDto>>> Handle(GetSuppliersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Suppliers
            .Where(s => !s.IsDeleted)
            .AsNoTracking();

        if (request.OnlyActive.HasValue && request.OnlyActive.Value)
        {
            query = query.Where(s => s.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(s =>
                s.Name.ToLower().Contains(search) ||
                (s.ContactPerson != null && s.ContactPerson.ToLower().Contains(search)) ||
                (s.Email != null && s.Email.ToLower().Contains(search)) ||
                (s.Phone != null && s.Phone.ToLower().Contains(search)) ||
                (s.TaxNumber != null && s.TaxNumber.ToLower().Contains(search)));
        }

        var suppliers = await query
            .OrderByDescending(s => s.CreatedDate)
            .Select(s => new SupplierDto
            {
                Id = s.Id,
                Name = s.Name,
                ContactPerson = s.ContactPerson,
                Email = s.Email,
                Phone = s.Phone,
                Address = s.Address,
                TaxNumber = s.TaxNumber,
                IsActive = s.IsActive,
                ProductCount = s.Products.Count(p => !p.IsDeleted),
                CreatedDate = s.CreatedDate,
                UpdatedDate = s.UpdatedDate
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<List<SupplierDto>>.Success(suppliers);
    }
}
