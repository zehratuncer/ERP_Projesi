using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Features.PurchaseRequests.Commands.CreatePurchaseRequest;
using ERP.Application.Features.PurchaseRequests.DTOs;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.PurchaseRequests.Commands.UpdatePurchaseRequest;

public record UpdatePurchaseRequestCommand(
    Guid Id,
    string Department,
    RequestPriority Priority,
    DateTime? RequiredDate,
    string? Note,
    List<CreatePurchaseRequestItemRequest> Items,
    bool SubmitForApproval = true
) : IRequest<ApiResponse<PurchaseRequestDto>>;

public class UpdatePurchaseRequestCommandValidator : AbstractValidator<UpdatePurchaseRequestCommand>
{
    public UpdatePurchaseRequestCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Talep ID zorunludur.");
        RuleFor(x => x.Department).NotEmpty().WithMessage("Departman bilgisi zorunludur.");
        RuleFor(x => x.Priority).IsInEnum().WithMessage("Geçerli bir öncelik seçiniz.");
        RuleFor(x => x.Items).NotEmpty().WithMessage("Talepte en az bir ürün bulunmalıdır.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty().WithMessage("Ürün seçilmelidir.");
            item.RuleFor(i => i.RequestedQuantity).GreaterThan(0).WithMessage("Miktar 0'dan büyük olmalıdır.");
        });
    }
}

public class UpdatePurchaseRequestCommandHandler : IRequestHandler<UpdatePurchaseRequestCommand, ApiResponse<PurchaseRequestDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdatePurchaseRequestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<PurchaseRequestDto>> Handle(UpdatePurchaseRequestCommand request, CancellationToken cancellationToken)
    {
        var purchaseRequest = await _context.PurchaseRequests
            .Include(pr => pr.RequesterUser)
            .Include(pr => pr.Items)
            .FirstOrDefaultAsync(pr => pr.Id == request.Id && !pr.IsDeleted, cancellationToken);

        if (purchaseRequest == null)
        {
            throw new NotFoundException("Satın Alma Talebi", request.Id);
        }

        // Sadece Taslak veya Onay Bekleyen talepler güncellenebilir
        if (purchaseRequest.Status != RequestStatus.Draft && purchaseRequest.Status != RequestStatus.PendingApproval)
        {
            throw new BusinessException($"Yalnızca 'Taslak' veya 'Onay Bekliyor' durumundaki talepler güncellenebilir. Mevcut durum: {purchaseRequest.Status}");
        }

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

        purchaseRequest.Department = request.Department.Trim();
        purchaseRequest.Priority = request.Priority;
        purchaseRequest.RequiredDate = request.RequiredDate;
        purchaseRequest.Note = request.Note?.Trim();
        if (request.SubmitForApproval)
        {
            purchaseRequest.Status = RequestStatus.PendingApproval;
        }

        // Eski kalemleri temizle ve yenilerini ekle
        _context.PurchaseRequestItems.RemoveRange(purchaseRequest.Items);
        purchaseRequest.Items.Clear();

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

        return ApiResponse<PurchaseRequestDto>.Success(dto, $"Satın alma talebi '{purchaseRequest.RequestNumber}' başarıyla güncellendi.");
    }
}
