using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Features.PurchaseRequests.DTOs;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.PurchaseRequests.Commands.RejectPurchaseRequest;

public record RejectPurchaseRequestCommand(
    Guid Id,
    string Reason
) : IRequest<ApiResponse<PurchaseRequestDto>>;

public class RejectPurchaseRequestCommandValidator : AbstractValidator<RejectPurchaseRequestCommand>
{
    public RejectPurchaseRequestCommandValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Talebi reddederken ret gerekçesi/açıklaması belirtilmesi zorunludur.")
            .MinimumLength(5).WithMessage("Ret açıklaması en az 5 karakter olmalıdır.")
            .MaximumLength(500).WithMessage("Ret açıklaması en fazla 500 karakter olabilir.");
    }
}

public class RejectPurchaseRequestCommandHandler : IRequestHandler<RejectPurchaseRequestCommand, ApiResponse<PurchaseRequestDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationService _notificationService;

    public RejectPurchaseRequestCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        INotificationService notificationService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
    }

    public async Task<ApiResponse<PurchaseRequestDto>> Handle(RejectPurchaseRequestCommand request, CancellationToken cancellationToken)
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
            return ApiResponse<PurchaseRequestDto>.Failure($"Talep '{purchaseRequest.RequestNumber}' onay bekleyen durumda değil. Mevcut durum: {purchaseRequest.Status}");
        }

        var approverUserId = _currentUserService.UserId;
        var approverUser = approverUserId.HasValue
            ? await _context.Users.FirstOrDefaultAsync(u => u.Id == approverUserId.Value, cancellationToken)
            : null;

        var history = new ApprovalHistory
        {
            PurchaseRequestId = purchaseRequest.Id,
            ApproverUserId = approverUserId,
            StepNumber = purchaseRequest.CurrentApprovalStep,
            StepName = purchaseRequest.CurrentApprovalStep == 2 
                ? "2. Aşama: Genel Satın Alma Direktörü Onayı" 
                : "Birim / Şube Müdürü Onayı",
            Action = ApprovalAction.Rejected,
            Comment = request.Reason.Trim(),
            ActionDate = DateTime.UtcNow
        };

        _context.ApprovalHistories.Add(history);
        purchaseRequest.Status = RequestStatus.Rejected;

        await _context.SaveChangesAsync(cancellationToken);

        // Talep Sahibine Ret Bildirimi
        if (purchaseRequest.RequesterUserId.HasValue)
        {
            await _notificationService.SendNotificationAsync(
                purchaseRequest.RequesterUserId.Value,
                null,
                "❌ Satın Alma Talebiniz Reddedildi",
                $"{purchaseRequest.RequestNumber} numaralı talebiniz reddedildi. Ret Gerekçesi: {request.Reason.Trim()}",
                NotificationType.Warning,
                "/purchase-requests",
                cancellationToken);
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

        return ApiResponse<PurchaseRequestDto>.Success(dto, $"Satın alma talebi '{purchaseRequest.RequestNumber}' reddedildi. Ret sebebi: {request.Reason.Trim()}");
    }
}
