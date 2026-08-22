using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Features.Inventory.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.Inventory.Queries.GetStockMovements;

public record GetStockMovementsQuery(Guid? ProductId = null, int Limit = 50) : IRequest<ApiResponse<List<StockMovementDto>>>;

public class GetStockMovementsQueryHandler : IRequestHandler<GetStockMovementsQuery, ApiResponse<List<StockMovementDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetStockMovementsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<StockMovementDto>>> Handle(GetStockMovementsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.InventoryTransactions
            .Include(t => t.Product)
            .Include(t => t.User)
            .AsNoTracking();

        if (request.ProductId.HasValue)
        {
            query = query.Where(t => t.ProductId == request.ProductId.Value);
        }

        var movements = await query
            .OrderByDescending(t => t.TransactionDate)
            .Take(request.Limit > 0 ? request.Limit : 50)
            .Select(t => new StockMovementDto
            {
                Id = t.Id,
                ProductId = t.ProductId,
                ProductCode = t.Product.Code,
                ProductName = t.Product.Name,
                Unit = t.Product.Unit,
                Quantity = t.Quantity,
                TransactionType = t.TransactionType,
                Description = t.Description,
                TransactionDate = t.TransactionDate,
                UserName = t.User != null ? t.User.FullName : "Sistem Yöneticisi"
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<List<StockMovementDto>>.Success(movements);
    }
}
