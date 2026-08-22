using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Features.Inventory.DTOs;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.Inventory.Commands.CreateStockMovement;

public record CreateStockMovementCommand(
    Guid ProductId,
    int Quantity,
    TransactionType TransactionType,
    string? Description
) : IRequest<ApiResponse<StockMovementDto>>;

public class CreateStockMovementCommandValidator : AbstractValidator<CreateStockMovementCommand>
{
    public CreateStockMovementCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("Ürün seçilmelidir.");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("İşlem miktarı 0'dan büyük olmalıdır.");
        RuleFor(x => x.TransactionType).IsInEnum().WithMessage("Geçerli bir hareket tipi seçiniz.");
    }
}

public class CreateStockMovementCommandHandler : IRequestHandler<CreateStockMovementCommand, ApiResponse<StockMovementDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateStockMovementCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<StockMovementDto>> Handle(CreateStockMovementCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId && !p.IsDeleted, cancellationToken);
        if (product == null)
        {
            throw new NotFoundException("Ürün", request.ProductId);
        }

        // Stok Çıkışında Bakiye Yeterlilik Kontrolü
        if (request.TransactionType == TransactionType.Out)
        {
            if (product.CurrentStock < request.Quantity)
            {
                throw new BusinessException($"Yetersiz stok! '{product.Name}' için mevcut stok: {product.CurrentStock} {product.Unit}, çıkış yapılmak istenen: {request.Quantity} {product.Unit}.");
            }

            product.CurrentStock -= request.Quantity;
        }
        else if (request.TransactionType == TransactionType.In)
        {
            product.CurrentStock += request.Quantity;
        }
        else if (request.TransactionType == TransactionType.Adjustment)
        {
            // Sayım düzeltmesi: Doğrudan yeni miktar atanır
            product.CurrentStock = request.Quantity;
        }

        var transaction = new InventoryTransaction
        {
            ProductId = product.Id,
            Quantity = request.Quantity,
            TransactionType = request.TransactionType,
            Description = request.Description,
            TransactionDate = DateTime.UtcNow,
            UserId = _currentUserService.UserId
        };

        _context.InventoryTransactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        var user = _currentUserService.UserId.HasValue 
            ? await _context.Users.FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId.Value, cancellationToken)
            : null;

        var dto = new StockMovementDto
        {
            Id = transaction.Id,
            ProductId = product.Id,
            ProductCode = product.Code,
            ProductName = product.Name,
            Unit = product.Unit,
            Quantity = transaction.Quantity,
            TransactionType = transaction.TransactionType,
            Description = transaction.Description,
            TransactionDate = transaction.TransactionDate,
            UserName = user?.FullName ?? "Sistem Yöneticisi"
        };

        return ApiResponse<StockMovementDto>.Success(dto, $"Stok {dto.TransactionTypeName.ToLower()} işlemi başarıyla tamamlandı. Yeni stok: {product.CurrentStock} {product.Unit}");
    }
}
