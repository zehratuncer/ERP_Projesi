using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Features.PurchaseRequests.DTOs;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.PurchaseRequests.Commands.CreatePurchaseRequest;

public record CreatePurchaseRequestItemRequest(
    Guid ProductId,
    int RequestedQuantity,
    string? Unit = null,
    decimal? EstimatedUnitPrice = null,
    string? Notes = null
);

public record CreatePurchaseRequestCommand(
    string Department,
    RequestPriority Priority,
    DateTime? RequiredDate,
    string? Note,
    List<CreatePurchaseRequestItemRequest> Items,
    bool SubmitForApproval = true
) : IRequest<ApiResponse<PurchaseRequestDto>>;

public class CreatePurchaseRequestCommandValidator : AbstractValidator<CreatePurchaseRequestCommand>
{
    public CreatePurchaseRequestCommandValidator()
    {
        RuleFor(x => x.Department)
            .NotEmpty().WithMessage("Departman bilgisi zorunludur.")
            .MaximumLength(100).WithMessage("Departman adı en fazla 100 karakter olabilir.");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Geçerli bir aciliyet / öncelik derecesi seçiniz.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Satın alma talebinde en az bir ürün kalemi bulunmalıdır.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty().WithMessage("Ürün seçilmelidir.");
            item.RuleFor(i => i.RequestedQuantity).GreaterThan(0).WithMessage("Talep edilen miktar 0'dan büyük olmalıdır.");
            item.RuleFor(i => i.EstimatedUnitPrice)
                .GreaterThanOrEqualTo(0m).When(i => i.EstimatedUnitPrice.HasValue)
                .WithMessage("Tahmini birim fiyat 0'dan küçük olamaz.");
        });
    }
}

public class CreatePurchaseRequestCommandHandler : IRequestHandler<CreatePurchaseRequestCommand, ApiResponse<PurchaseRequestDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationService _notificationService;

    public CreatePurchaseRequestCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        INotificationService notificationService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
    }

    public async Task<ApiResponse<PurchaseRequestDto>> Handle(CreatePurchaseRequestCommand request, CancellationToken cancellationToken)
    {
        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id) && !p.IsDeleted)
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        foreach (var itemReq in request.Items)
        {
            if (!products.ContainsKey(itemReq.ProductId))
            {
                throw new NotFoundException("Ürün", itemReq.ProductId);
            }
        }

        // Talep Numarası Üretimi (Örn: TALEP-20260824-042)
        var requestNumber = $"TALEP-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(100, 999)}";

        var purchaseRequest = new PurchaseRequest
        {
            RequestNumber = requestNumber,
            Department = request.Department.Trim(),
            RequesterUserId = _currentUserService.UserId,
            Priority = request.Priority,
            Status = request.SubmitForApproval ? RequestStatus.PendingApproval : RequestStatus.Draft,
            RequiredDate = request.RequiredDate,
            Note = request.Note?.Trim(),
            TotalEstimatedAmount = 0m
        };

        decimal totalAmount = 0m;

        foreach (var itemReq in request.Items)
        {
            var product = products[itemReq.ProductId];
            var unitPrice = itemReq.EstimatedUnitPrice ?? product.UnitPrice;
            var totalPrice = unitPrice * itemReq.RequestedQuantity;
            totalAmount += totalPrice;

            var requestItem = new PurchaseRequestItem
            {
                PurchaseRequest = purchaseRequest,
                ProductId = product.Id,
                RequestedQuantity = itemReq.RequestedQuantity,
                Unit = !string.IsNullOrWhiteSpace(itemReq.Unit) ? itemReq.Unit.Trim() : product.Unit,
                EstimatedUnitPrice = unitPrice,
                EstimatedTotalPrice = totalPrice,
                Notes = itemReq.Notes?.Trim()
            };

            purchaseRequest.Items.Add(requestItem);
        }

        purchaseRequest.TotalEstimatedAmount = totalAmount;

        _context.PurchaseRequests.Add(purchaseRequest);
        await _context.SaveChangesAsync(cancellationToken);

        var requesterUser = _currentUserService.UserId.HasValue
            ? await _context.Users.FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId.Value, cancellationToken)
            : null;

        // Bildirim Tetikleme (Onaya Gönderildiyse)
        if (purchaseRequest.Status == RequestStatus.PendingApproval)
        {
            await _notificationService.SendNotificationAsync(
                null,
                "Manager",
                "📝 Yeni Satın Alma Talebi Onay Bekliyor",
                $"{requesterUser?.FullName ?? "Bir personel"} tarafından {purchaseRequest.Department} departmanı için {purchaseRequest.RequestNumber} numaralı talep ({totalAmount:N2} ₺) oluşturuldu.",
                NotificationType.ApprovalNeeded,
                "/purchase-requests",
                cancellationToken);
        }

        var dto = new PurchaseRequestDto
        {
            Id = purchaseRequest.Id,
            RequestNumber = purchaseRequest.RequestNumber,
            Department = purchaseRequest.Department,
            RequesterUserId = purchaseRequest.RequesterUserId,
            RequesterUserName = requesterUser?.FullName ?? "Talep Sahibi",
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
                ProductCode = products[item.ProductId].Code,
                ProductName = products[item.ProductId].Name,
                CurrentStock = products[item.ProductId].CurrentStock,
                MinStockLevel = products[item.ProductId].MinStockLevel,
                RequestedQuantity = item.RequestedQuantity,
                Unit = item.Unit,
                EstimatedUnitPrice = item.EstimatedUnitPrice,
                EstimatedTotalPrice = item.EstimatedTotalPrice,
                Notes = item.Notes
            }).ToList()
        };

        var statusMessage = purchaseRequest.Status == RequestStatus.PendingApproval ? "onaya gönderildi." : "taslak olarak kaydedildi.";
        return ApiResponse<PurchaseRequestDto>.Success(dto, $"Satın alma talebi '{requestNumber}' başarıyla oluşturuldu ve {statusMessage}");
    }
}
