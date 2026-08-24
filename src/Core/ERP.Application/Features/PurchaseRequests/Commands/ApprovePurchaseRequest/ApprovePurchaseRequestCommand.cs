using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Features.PurchaseRequests.DTOs;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.PurchaseRequests.Commands.ApprovePurchaseRequest;

public record ApprovePurchaseRequestCommand(
    Guid Id,
    string? Comment = null
) : IRequest<ApiResponse<PurchaseRequestDto>>;

public class ApprovePurchaseRequestCommandHandler : IRequestHandler<ApprovePurchaseRequestCommand, ApiResponse<PurchaseRequestDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public const decimal HighAmountThreshold = 10000m; // 10.000 TL ve üzeri direktör onayı gerektirir

    public ApprovePurchaseRequestCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<PurchaseRequestDto>> Handle(ApprovePurchaseRequestCommand request, CancellationToken cancellationToken)
    {
        var purchaseRequest = await _context.PurchaseRequests
            .Include(pr => pr.RequesterUser)
            .Include(pr => pr.Items)
                .ThenInclude(pri => pri.Product)
            .Include(pr => pr.ApprovalHistories)
                .ThenInclude(ah => ah.ApproverUser)
            .FirstOrDefaultAsync(pr => pr.Id == request.Id && !pr.IsDeleted, cancellationToken);

        if (purchaseRequest == null)
        {
            throw new NotFoundException("Satın Alma Talebi", request.Id);
        }

        if (purchaseRequest.Status != RequestStatus.PendingApproval)
        {
            return ApiResponse<PurchaseRequestDto>.Failure($"Talep '{purchaseRequest.RequestNumber}' onay bekleyen (PendingApproval) durumda değil. Mevcut durum: {purchaseRequest.Status}");
        }

        var approverUserId = _currentUserService.UserId;
        var approverUser = approverUserId.HasValue
            ? await _context.Users.FirstOrDefaultAsync(u => u.Id == approverUserId.Value, cancellationToken)
            : null;

        string message;
        bool isMultiStepRequired = purchaseRequest.TotalEstimatedAmount > HighAmountThreshold;

        if (isMultiStepRequired && purchaseRequest.CurrentApprovalStep == 1)
        {
            // 1. Kademe Onayı (Şube / Birim Müdürü) -> 2. Kademeye Sevk (Genel Direktör)
            var history = new ApprovalHistory
            {
                PurchaseRequestId = purchaseRequest.Id,
                ApproverUserId = approverUserId,
                StepNumber = 1,
                StepName = "1. Aşama: Şube / Birim Müdürü Onayı",
                Action = ApprovalAction.Approved,
                Comment = !string.IsNullOrWhiteSpace(request.Comment) 
                    ? request.Comment.Trim() 
                    : $"1. Seviye onaylandı. Tutar {purchaseRequest.TotalEstimatedAmount:N2} TL > {HighAmountThreshold:N2} TL olduğu için Genel Satın Alma Direktörü onayına iletildi.",
                ActionDate = DateTime.UtcNow
            };

            purchaseRequest.ApprovalHistories.Add(history);
            purchaseRequest.CurrentApprovalStep = 2; // Bir sonraki kademeye geçti
            // Status hala PendingApproval

            message = $"Satın alma talebi 1. seviye (Şube Müdürü) tarafından onaylandı ve tutar limiti gereği Genel Direktör onayına iletildi.";
        }
        else
        {
            // Nihai Onay (Ya tek kademeli <= 10.000 TL ya da 2. kademe Direktör Onayı)
            int stepNum = isMultiStepRequired ? 2 : 1;
            string stepName = isMultiStepRequired 
                ? "2. Aşama: Genel Satın Alma Direktörü Onayı" 
                : "Birim / Şube Müdürü Onayı";

            var history = new ApprovalHistory
            {
                PurchaseRequestId = purchaseRequest.Id,
                ApproverUserId = approverUserId,
                StepNumber = stepNum,
                StepName = stepName,
                Action = ApprovalAction.Approved,
                Comment = !string.IsNullOrWhiteSpace(request.Comment) ? request.Comment.Trim() : "Talep başarıyla onaylandı.",
                ActionDate = DateTime.UtcNow
            };

            purchaseRequest.ApprovalHistories.Add(history);
            purchaseRequest.Status = RequestStatus.Approved;

            message = $"Satın alma talebi '{purchaseRequest.RequestNumber}' başarıyla onaylandı.";
        }

        await _context.SaveChangesAsync(cancellationToken);

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
            CurrentApprovalStep = purchaseRequest.CurrentApprovalStep,
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
            }).ToList(),
            ApprovalHistories = purchaseRequest.ApprovalHistories
                .OrderByDescending(ah => ah.ActionDate)
                .Select(ah => new ApprovalHistoryDto
                {
                    Id = ah.Id,
                    PurchaseRequestId = ah.PurchaseRequestId,
                    ApproverUserId = ah.ApproverUserId,
                    ApproverUserName = ah.ApproverUser?.FullName ?? (ah.ApproverUserId == approverUserId ? approverUser?.FullName ?? "Yönetici" : "Yönetici"),
                    StepNumber = ah.StepNumber,
                    StepName = ah.StepName,
                    Action = ah.Action,
                    Comment = ah.Comment,
                    ActionDate = ah.ActionDate
                }).ToList()
        };

        return ApiResponse<PurchaseRequestDto>.Success(dto, message);
    }
}
