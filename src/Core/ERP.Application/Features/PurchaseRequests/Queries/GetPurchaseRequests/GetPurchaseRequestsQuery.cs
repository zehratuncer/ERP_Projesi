using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Features.PurchaseRequests.DTOs;
using ERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.PurchaseRequests.Queries.GetPurchaseRequests;

public record GetPurchaseRequestsQuery(
    RequestStatus? Status = null,
    string? Department = null,
    RequestPriority? Priority = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    Guid? RequesterUserId = null,
    string? Search = null
) : IRequest<ApiResponse<List<PurchaseRequestListDto>>>;

public class GetPurchaseRequestsQueryHandler : IRequestHandler<GetPurchaseRequestsQuery, ApiResponse<List<PurchaseRequestListDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetPurchaseRequestsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<PurchaseRequestListDto>>> Handle(GetPurchaseRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PurchaseRequests
            .Include(pr => pr.RequesterUser)
            .Include(pr => pr.Items)
            .Where(pr => !pr.IsDeleted)
            .AsNoTracking();

        if (request.Status.HasValue)
        {
            query = query.Where(pr => pr.Status == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Department))
        {
            query = query.Where(pr => pr.Department == request.Department.Trim());
        }

        if (request.Priority.HasValue)
        {
            query = query.Where(pr => pr.Priority == request.Priority.Value);
        }

        if (request.RequesterUserId.HasValue)
        {
            query = query.Where(pr => pr.RequesterUserId == request.RequesterUserId.Value);
        }

        if (request.StartDate.HasValue)
        {
            var start = DateTime.SpecifyKind(request.StartDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(pr => pr.CreatedDate >= start);
        }

        if (request.EndDate.HasValue)
        {
            var end = DateTime.SpecifyKind(request.EndDate.Value.Date.AddDays(1), DateTimeKind.Utc);
            query = query.Where(pr => pr.CreatedDate < end);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(pr => 
                pr.RequestNumber.ToLower().Contains(term) || 
                pr.Department.ToLower().Contains(term) ||
                (pr.Note != null && pr.Note.ToLower().Contains(term)) ||
                (pr.RequesterUser != null && pr.RequesterUser.FullName.ToLower().Contains(term)));
        }

        var list = await query
            .OrderByDescending(pr => pr.CreatedDate)
            .Select(pr => new PurchaseRequestListDto
            {
                Id = pr.Id,
                RequestNumber = pr.RequestNumber,
                Department = pr.Department,
                RequesterUserId = pr.RequesterUserId,
                RequesterUserName = pr.RequesterUser != null ? pr.RequesterUser.FullName : "Talep Sahibi",
                Priority = pr.Priority,
                Status = pr.Status,
                TotalEstimatedAmount = pr.TotalEstimatedAmount,
                CurrentApprovalStep = pr.CurrentApprovalStep,
                RequiredDate = pr.RequiredDate,
                CreatedDate = pr.CreatedDate,
                Note = pr.Note,
                ItemCount = pr.Items.Count
            })

            .ToListAsync(cancellationToken);

        return ApiResponse<List<PurchaseRequestListDto>>.Success(list);
    }
}
