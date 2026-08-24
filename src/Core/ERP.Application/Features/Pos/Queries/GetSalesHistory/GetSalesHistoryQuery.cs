using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Features.Pos.DTOs;
using ERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.Pos.Queries.GetSalesHistory;

public record GetSalesHistoryQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    PaymentMethod? PaymentMethod = null,
    string? Search = null
) : IRequest<ApiResponse<List<SaleHistoryDto>>>;

public class GetSalesHistoryQueryHandler : IRequestHandler<GetSalesHistoryQuery, ApiResponse<List<SaleHistoryDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetSalesHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<SaleHistoryDto>>> Handle(GetSalesHistoryQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Sales
            .Include(s => s.CashierUser)
            .Include(s => s.Items)
            .Where(s => !s.IsDeleted)
            .AsNoTracking();

        if (request.StartDate.HasValue)
        {
            var start = DateTime.SpecifyKind(request.StartDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(s => s.SaleDate >= start);
        }

        if (request.EndDate.HasValue)
        {
            var end = DateTime.SpecifyKind(request.EndDate.Value.Date.AddDays(1), DateTimeKind.Utc);
            query = query.Where(s => s.SaleDate < end);
        }

        if (request.PaymentMethod.HasValue)
        {
            query = query.Where(s => s.PaymentMethod == request.PaymentMethod.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.Trim().ToLower();
            query = query.Where(s => 
                s.ReceiptNumber.ToLower().Contains(searchTerm) || 
                (s.CustomerName != null && s.CustomerName.ToLower().Contains(searchTerm)));
        }

        var sales = await query
            .OrderByDescending(s => s.SaleDate)
            .Select(s => new SaleHistoryDto
            {
                Id = s.Id,
                ReceiptNumber = s.ReceiptNumber,
                SaleDate = s.SaleDate,
                CashierName = s.CashierUser != null ? s.CashierUser.FullName : "Kasiyer",
                CustomerName = s.CustomerName,
                PaymentMethod = s.PaymentMethod,
                TotalAmount = s.TotalAmount,
                DiscountAmount = s.DiscountAmount,
                FinalAmount = s.FinalAmount,
                ItemCount = s.Items.Count
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<List<SaleHistoryDto>>.Success(sales);
    }
}
