using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Features.PurchaseRequests.DTOs;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.PurchaseRequests.Commands.ConvertPurchaseRequestToInventory;

public record ConvertPurchaseRequestToInventoryCommand(
    Guid Id,
    string? Note = null
) : IRequest<ApiResponse<PurchaseRequestDto>>;

public class ConvertPurchaseRequestToInventoryCommandHandler : IRequestHandler<ConvertPurchaseRequestToInventoryCommand, ApiResponse<PurchaseRequestDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ConvertPurchaseRequestToInventoryCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<PurchaseRequestDto>> Handle(ConvertPurchaseRequestToInventoryCommand request, CancellationToken cancellationToken)
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

        if (purchaseRequest.Status != RequestStatus.Approved)
        {
            return ApiResponse<PurchaseRequestDto>.Failure(
                $"Yalnızca onaylanmış (Approved) durumdaki satın alma talepleri mal kabul / stok girişine dönüştürülebilir. Mevcut durum: {purchaseRequest.Status}");
        }

        if (!purchaseRequest.Items.Any())
        {
            return ApiResponse<PurchaseRequestDto>.Failure("Bu satın alma talebinde stok girişi yapılacak kalem bulunamadı.");
        }

        var productIds = purchaseRequest.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        int totalQuantityReceived = 0;

        foreach (var item in purchaseRequest.Items)
        {
            if (products.TryGetValue(item.ProductId, out var product))
            {
                // Stok miktarını güncelle (Stok Girişi / Mal Kabul)
                product.CurrentStock += item.RequestedQuantity;
                totalQuantityReceived += item.RequestedQuantity;

                // Stok hareket kaydı (InventoryTransaction)
                var invTransaction = new InventoryTransaction
                {
                    ProductId = product.Id,
                    Quantity = item.RequestedQuantity,
                    TransactionType = TransactionType.In,
                    Description = $"Satın Alma Talebi Mal Kabul: {purchaseRequest.RequestNumber}" +
                                  (!string.IsNullOrWhiteSpace(request.Note) ? $" ({request.Note.Trim()})" : ""),
                    UserId = _currentUserService.UserId,
                    TransactionDate = DateTime.UtcNow
                };

                _context.InventoryTransactions.Add(invTransaction);
            }
        }

        // Talep durumunu Tamamlandı olarak güncelle
        purchaseRequest.Status = RequestStatus.Completed;

        // Geçmiş kaydı oluştur
        var approverUserId = _currentUserService.UserId;
        var approverUser = approverUserId.HasValue
            ? await _context.Users.FirstOrDefaultAsync(u => u.Id == approverUserId.Value, cancellationToken)
            : null;

        var history = new ApprovalHistory
        {
            PurchaseRequestId = purchaseRequest.Id,
            ApproverUserId = approverUserId,
            StepNumber = purchaseRequest.CurrentApprovalStep,
            StepName = "Mal Kabul & Depo Stok Girişi",
            Action = ApprovalAction.Approved,
            Comment = !string.IsNullOrWhiteSpace(request.Note)
                ? request.Note.Trim()
                : $"Onaylanan talep kalemleri ({totalQuantityReceived} adet ürün) depoya teslim alındı ve stoklara aktarıldı.",
            ActionDate = DateTime.UtcNow
        };

        purchaseRequest.ApprovalHistories.Add(history);

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
                ProductCode = products.ContainsKey(item.ProductId) ? products[item.ProductId].Code : string.Empty,
                ProductName = products.ContainsKey(item.ProductId) ? products[item.ProductId].Name : string.Empty,
                CurrentStock = products.ContainsKey(item.ProductId) ? products[item.ProductId].CurrentStock : 0,
                MinStockLevel = products.ContainsKey(item.ProductId) ? products[item.ProductId].MinStockLevel : 0,
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

        return ApiResponse<PurchaseRequestDto>.Success(
            dto,
            $"Satın alma talebi '{purchaseRequest.RequestNumber}' başarıyla mal kabul edilerek toplam {totalQuantityReceived} adet ürün stoğa işlendi ve talep tamamlandı.");
    }
}
