using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Features.Pos.DTOs;
using ERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.Pos.Queries.GetDailyPosSummary;

public record GetDailyPosSummaryQuery(DateTime? Date = null) : IRequest<ApiResponse<DailyPosSummaryDto>>;

public class GetDailyPosSummaryQueryHandler : IRequestHandler<GetDailyPosSummaryQuery, ApiResponse<DailyPosSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDailyPosSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<DailyPosSummaryDto>> Handle(GetDailyPosSummaryQuery request, CancellationToken cancellationToken)
    {
        var targetDate = request.Date?.Date ?? DateTime.UtcNow.Date;
        var startOfDay = DateTime.SpecifyKind(targetDate, DateTimeKind.Utc);
        var endOfDay = startOfDay.AddDays(1);

        var sales = await _context.Sales
            .Include(s => s.Items)
                .ThenInclude(si => si.Product)
            .Where(s => !s.IsDeleted && s.SaleDate >= startOfDay && s.SaleDate < endOfDay)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var totalRevenue = sales.Sum(s => s.FinalAmount);
        var totalSalesCount = sales.Count;
        var totalDiscountsGiven = sales.Sum(s => s.DiscountAmount);

        var cashTotal = sales.Where(s => s.PaymentMethod == PaymentMethod.Cash).Sum(s => s.FinalAmount);
        var creditCardTotal = sales.Where(s => s.PaymentMethod == PaymentMethod.CreditCard).Sum(s => s.FinalAmount);
        var splitTotal = sales.Where(s => s.PaymentMethod == PaymentMethod.Split).Sum(s => s.FinalAmount);
        var onAccountTotal = sales.Where(s => s.PaymentMethod == PaymentMethod.OnAccount).Sum(s => s.FinalAmount);

        var allItems = sales.SelectMany(s => s.Items).ToList();
        var totalItemsSold = allItems.Sum(i => i.Quantity);

        var topSelling = allItems
            .GroupBy(i => new { i.ProductId, ProductCode = i.Product?.Code ?? string.Empty, ProductName = i.Product?.Name ?? string.Empty })
            .Select(g => new TopSellingProductDto
            {
                ProductId = g.Key.ProductId,
                ProductCode = g.Key.ProductCode,
                ProductName = g.Key.ProductName,
                TotalQuantitySold = g.Sum(x => x.Quantity),
                TotalRevenue = g.Sum(x => x.TotalPrice)
            })
            .OrderByDescending(x => x.TotalQuantitySold)
            .Take(5)
            .ToList();

        var summary = new DailyPosSummaryDto
        {
            Date = targetDate,
            TotalRevenue = totalRevenue,
            TotalSalesCount = totalSalesCount,
            TotalItemsSold = totalItemsSold,
            CashTotal = cashTotal,
            CreditCardTotal = creditCardTotal,
            SplitTotal = splitTotal,
            OnAccountTotal = onAccountTotal,
            TotalDiscountsGiven = totalDiscountsGiven,
            TopSellingProducts = topSelling
        };

        return ApiResponse<DailyPosSummaryDto>.Success(summary);
    }
}
