using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Features.Pos.DTOs;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.Pos.Commands.CompleteSale;

public record CompleteSaleItemRequest(
    Guid ProductId,
    int Quantity,
    decimal? CustomUnitPrice = null,
    decimal DiscountRate = 0.0m
);

public record CompleteSaleCommand(
    List<CompleteSaleItemRequest> Items,
    PaymentMethod PaymentMethod,
    string? CustomerName = null,
    decimal GeneralDiscountAmount = 0.0m
) : IRequest<ApiResponse<SaleReceiptDto>>;

public class CompleteSaleCommandValidator : AbstractValidator<CompleteSaleCommand>
{
    public CompleteSaleCommandValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Satış yapılabilmesi için sepette en az 1 ürün bulunmalıdır.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty().WithMessage("Ürün seçilmelidir.");
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Satış adedi 0'dan büyük olmalıdır.");
            item.RuleFor(i => i.DiscountRate).InclusiveBetween(0m, 100m).WithMessage("İndirim oranı %0 ile %100 arasında olmalıdır.");
            item.RuleFor(i => i.CustomUnitPrice)
                .GreaterThanOrEqualTo(0m).When(i => i.CustomUnitPrice.HasValue)
                .WithMessage("Birim fiyat 0'dan küçük olamaz.");
        });

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Geçerli bir ödeme yöntemi seçiniz.");

        RuleFor(x => x.GeneralDiscountAmount)
            .GreaterThanOrEqualTo(0m).WithMessage("Genel indirim tutarı 0'dan küçük olamaz.");
    }
}

public class CompleteSaleCommandHandler : IRequestHandler<CompleteSaleCommand, ApiResponse<SaleReceiptDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CompleteSaleCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<SaleReceiptDto>> Handle(CompleteSaleCommand request, CancellationToken cancellationToken)
    {
        if (request.Items == null || !request.Items.Any())
        {
            throw new BusinessException("Sepette ürün bulunmamaktadır.");
        }

        // Ürün ID'lerini topla ve topluca veritabanından getir
        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id) && !p.IsDeleted)
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        // Eksik ürün kontrolü
        foreach (var itemReq in request.Items)
        {
            if (!products.ContainsKey(itemReq.ProductId))
            {
                throw new NotFoundException("Ürün", itemReq.ProductId);
            }
        }

        // Stok yeterlilik kontrolü
        var requestedQuantities = request.Items
            .GroupBy(i => i.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        foreach (var (productId, totalRequestedQty) in requestedQuantities)
        {
            var product = products[productId];
            if (product.CurrentStock < totalRequestedQty)
            {
                throw new BusinessException($"Yetersiz stok! '{product.Name}' için mevcut stok: {product.CurrentStock} {product.Unit}, satılmak istenen: {totalRequestedQty} {product.Unit}.");
            }
        }

        // Benzersiz Fiş Numarası Üretimi (Örnek: FIS-20260824-112233-042)
        var receiptNumber = $"FIS-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Random.Shared.Next(100, 999)}";
        var saleDate = DateTime.UtcNow;

        var sale = new Sale
        {
            ReceiptNumber = receiptNumber,
            CashierUserId = _currentUserService.UserId,
            CustomerName = string.IsNullOrWhiteSpace(request.CustomerName) ? "Perakende Müşteri" : request.CustomerName.Trim(),
            PaymentMethod = request.PaymentMethod,
            SaleDate = saleDate,
            TotalAmount = 0m,
            DiscountAmount = request.GeneralDiscountAmount,
            FinalAmount = 0m
        };

        decimal calculatedTotalAmount = 0m;
        decimal totalItemDiscounts = 0m;
        var criticalStockAlerts = new List<string>();

        foreach (var itemReq in request.Items)
        {
            var product = products[itemReq.ProductId];
            var unitPrice = itemReq.CustomUnitPrice ?? product.UnitPrice;
            var grossItemPrice = unitPrice * itemReq.Quantity;
            var itemDiscount = itemReq.DiscountRate > 0 ? (grossItemPrice * (itemReq.DiscountRate / 100m)) : 0m;
            var netItemPrice = grossItemPrice - itemDiscount;

            calculatedTotalAmount += grossItemPrice;
            totalItemDiscounts += itemDiscount;

            // Stoktan otomatik düşüm
            product.CurrentStock -= itemReq.Quantity;

            // Otomatik Stok Hareketi (InventoryTransaction)
            var invTransaction = new InventoryTransaction
            {
                ProductId = product.Id,
                Quantity = itemReq.Quantity,
                TransactionType = TransactionType.Out,
                Description = $"Fiş No: {receiptNumber} Satışı",
                TransactionDate = saleDate,
                UserId = _currentUserService.UserId
            };
            _context.InventoryTransactions.Add(invTransaction);

            // Kritik Stok Eşiği Kontrolü & Uyarı
            if (product.CurrentStock <= product.MinStockLevel)
            {
                criticalStockAlerts.Add($"'{product.Name}' kritik stok seviyesine düştü! (Kalan Stok: {product.CurrentStock} {product.Unit}, Kritik Eşik: {product.MinStockLevel})");
            }

            var saleItem = new SaleItem
            {
                Sale = sale,
                ProductId = product.Id,
                Quantity = itemReq.Quantity,
                UnitPrice = unitPrice,
                TotalPrice = netItemPrice,
                DiscountRate = itemReq.DiscountRate
            };
            sale.Items.Add(saleItem);
        }

        var totalDiscount = totalItemDiscounts + request.GeneralDiscountAmount;
        var finalAmount = Math.Max(0m, calculatedTotalAmount - totalDiscount);

        sale.TotalAmount = calculatedTotalAmount;
        sale.DiscountAmount = totalDiscount;
        sale.FinalAmount = finalAmount;

        _context.Sales.Add(sale);
        await _context.SaveChangesAsync(cancellationToken);

        var cashierUser = _currentUserService.UserId.HasValue
            ? await _context.Users.FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId.Value, cancellationToken)
            : null;

        var receiptDto = new SaleReceiptDto
        {
            Id = sale.Id,
            ReceiptNumber = sale.ReceiptNumber,
            SaleDate = sale.SaleDate,
            CashierUserId = sale.CashierUserId,
            CashierName = cashierUser?.FullName ?? "Kasiyer",
            CustomerName = sale.CustomerName,
            PaymentMethod = sale.PaymentMethod,
            TotalAmount = sale.TotalAmount,
            DiscountAmount = sale.DiscountAmount,
            FinalAmount = sale.FinalAmount,
            CriticalStockAlerts = criticalStockAlerts,
            Items = sale.Items.Select(si => new SaleItemDto
            {
                Id = si.Id,
                ProductId = si.ProductId,
                ProductCode = products[si.ProductId].Code,
                ProductName = products[si.ProductId].Name,
                Unit = products[si.ProductId].Unit,
                Quantity = si.Quantity,
                UnitPrice = si.UnitPrice,
                TotalPrice = si.TotalPrice,
                DiscountRate = si.DiscountRate
            }).ToList()
        };

        var message = $"Satış başarıyla tamamlandı. Fiş No: {receiptNumber}, Tutar: {finalAmount:N2} ₺";
        if (criticalStockAlerts.Any())
        {
            message += $" ({criticalStockAlerts.Count} üründe kritik stok uyarısı oluştu!)";
        }

        return ApiResponse<SaleReceiptDto>.Success(receiptDto, message);
    }
}
