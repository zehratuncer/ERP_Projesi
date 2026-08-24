using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Features.PurchaseRequests.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.PurchaseRequests.Queries.GetPurchaseRequestById;

public record GetPurchaseRequestByIdQuery(Guid Id) : IRequest<ApiResponse<PurchaseRequestDto>>;

public class GetPurchaseRequestByIdQueryHandler : IRequestHandler<GetPurchaseRequestByIdQuery, ApiResponse<PurchaseRequestDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPurchaseRequestByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<PurchaseRequestDto>> Handle(GetPurchaseRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var purchaseRequest = await _context.PurchaseRequests
            .Include(pr => pr.RequesterUser)
            .Include(pr => pr.Items)
                .ThenInclude(pri => pri.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync(pr => pr.Id == request.Id && !pr.IsDeleted, cancellationToken);

        if (purchaseRequest == null)
        {
            throw new NotFoundException("Satın Alma Talebi", request.Id);
        }

        var dto = new PurchaseRequestDto
        {
            Id = purchaseRequest.Id,
            RequestNumber = purchaseRequest.RequestNumber,
            Department = purchaseRequest.Department,
            RequesterUserId = purchaseRequest.RequesterUserId,
            RequesterUserName = purchaseRequest.RequesterUser?.FullName ?? "Talep Sahibi",
            Priority = purchaseRequest.Priority,
            Status = purchaseRequest.Status,
            TotalEstimatedAmount = purchaseRequest.TotalEstimatedAmount,
            RequiredDate = purchaseRequest.RequiredDate,
            Note = purchaseRequest.Note,
            CreatedDate = purchaseRequest.CreatedDate,
            Items = purchaseRequest.Items.Select(item => new PurchaseRequestItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductCode = item.Product?.Code ?? string.Empty,
                ProductName = item.Product?.Name ?? string.Empty,
                CurrentStock = item.Product?.CurrentStock ?? 0,
                MinStockLevel = item.Product?.MinStockLevel ?? 0,
                RequestedQuantity = item.RequestedQuantity,
                Unit = item.Unit,
                EstimatedUnitPrice = item.EstimatedUnitPrice,
                EstimatedTotalPrice = item.EstimatedTotalPrice,
                Notes = item.Notes
            }).ToList()
        };

        return ApiResponse<PurchaseRequestDto>.Success(dto);
    }
}
