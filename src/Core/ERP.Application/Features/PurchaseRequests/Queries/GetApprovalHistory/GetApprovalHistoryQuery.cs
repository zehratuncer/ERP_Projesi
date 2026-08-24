using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Features.PurchaseRequests.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.PurchaseRequests.Queries.GetApprovalHistory;

public record GetApprovalHistoryQuery(Guid PurchaseRequestId) : IRequest<ApiResponse<List<ApprovalHistoryDto>>>;

public class GetApprovalHistoryQueryHandler : IRequestHandler<GetApprovalHistoryQuery, ApiResponse<List<ApprovalHistoryDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetApprovalHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<ApprovalHistoryDto>>> Handle(GetApprovalHistoryQuery request, CancellationToken cancellationToken)
    {
        var purchaseRequestExists = await _context.PurchaseRequests
            .AnyAsync(pr => pr.Id == request.PurchaseRequestId && !pr.IsDeleted, cancellationToken);

        if (!purchaseRequestExists)
        {
            throw new NotFoundException("Satın Alma Talebi", request.PurchaseRequestId);
        }

        var historyList = await _context.ApprovalHistories
            .Include(ah => ah.ApproverUser)
            .Where(ah => ah.PurchaseRequestId == request.PurchaseRequestId && !ah.IsDeleted)
            .OrderByDescending(ah => ah.ActionDate)
            .Select(ah => new ApprovalHistoryDto
            {
                Id = ah.Id,
                PurchaseRequestId = ah.PurchaseRequestId,
                ApproverUserId = ah.ApproverUserId,
                ApproverUserName = ah.ApproverUser != null ? ah.ApproverUser.FullName : "Sistem / Yönetici",
                StepNumber = ah.StepNumber,
                StepName = ah.StepName,
                Action = ah.Action,
                Comment = ah.Comment,
                ActionDate = ah.ActionDate
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<List<ApprovalHistoryDto>>.Success(historyList);
    }
}
