using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Features.Pos.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.Pos.Queries.GetReceiptByNumber;

public record GetReceiptByNumberQuery(string ReceiptNumber) : IRequest<ApiResponse<SaleReceiptDto>>;

public class GetReceiptByNumberQueryHandler : IRequestHandler<GetReceiptByNumberQuery, ApiResponse<SaleReceiptDto>>
{
    private readonly IApplicationDbContext _context;

    public GetReceiptByNumberQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<SaleReceiptDto>> Handle(GetReceiptByNumberQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ReceiptNumber))
        {
            throw new BusinessException("Fiş numarası boş olamaz.");
        }

        var normalizedReceipt = request.ReceiptNumber.Trim();

        var sale = await _context.Sales
            .Include(s => s.CashierUser)
            .Include(s => s.Items)
                .ThenInclude(si => si.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => !s.IsDeleted && s.ReceiptNumber.ToLower() == normalizedReceipt.ToLower(), cancellationToken);

        if (sale == null)
        {
            throw new NotFoundException("Satış Fişi", request.ReceiptNumber);
        }

        var dto = new SaleReceiptDto
        {
            Id = sale.Id,
            ReceiptNumber = sale.ReceiptNumber,
            SaleDate = sale.SaleDate,
            CashierUserId = sale.CashierUserId,
            CashierName = sale.CashierUser?.FullName ?? "Kasiyer",
            CustomerName = sale.CustomerName,
            PaymentMethod = sale.PaymentMethod,
            TotalAmount = sale.TotalAmount,
            DiscountAmount = sale.DiscountAmount,
            FinalAmount = sale.FinalAmount,
            Items = sale.Items.Select(item => new SaleItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductCode = item.Product?.Code ?? string.Empty,
                ProductName = item.Product?.Name ?? string.Empty,
                Unit = item.Product?.Unit ?? "Adet",
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice,
                DiscountRate = item.DiscountRate
            }).ToList()
        };

        return ApiResponse<SaleReceiptDto>.Success(dto);
    }
}
