using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Features.Products.DTOs;
using ERP.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.Products.Commands.CreateProduct;

public record CreateProductCommand(
    string Code,
    string Name,
    string? Description,
    string Unit,
    int InitialStock,
    int MinStockLevel,
    decimal UnitPrice,
    Guid? SupplierId = null
) : IRequest<ApiResponse<ProductDto>>;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Ürün kodu boş bırakılamaz.")
            .MaximumLength(50).WithMessage("Ürün kodu en fazla 50 karakter olabilir.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ürün adı boş bırakılamaz.")
            .MaximumLength(150).WithMessage("Ürün adı en fazla 150 karakter olabilir.");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("Birim boş bırakılamaz.");

        RuleFor(x => x.MinStockLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Kritik stok seviyesi 0 veya daha büyük olmalıdır.");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Birim fiyat 0 veya daha büyük olmalıdır.");
    }
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ApiResponse<ProductDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.Products.AnyAsync(p => p.Code.ToLower() == request.Code.ToLower() && !p.IsDeleted, cancellationToken);
        if (exists)
        {
            throw new BusinessException($"'{request.Code}' koduna sahip başka bir ürün zaten mevcut.");
        }

        var product = new Product
        {
            Code = request.Code.ToUpper().Trim(),
            Name = request.Name.Trim(),
            Description = request.Description,
            Unit = request.Unit.Trim(),
            CurrentStock = request.InitialStock > 0 ? request.InitialStock : 0,
            MinStockLevel = request.MinStockLevel,
            UnitPrice = request.UnitPrice,
            SupplierId = request.SupplierId,
            IsActive = true
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new ProductDto
        {
            Id = product.Id,
            Code = product.Code,
            Name = product.Name,
            Description = product.Description,
            Unit = product.Unit,
            CurrentStock = product.CurrentStock,
            MinStockLevel = product.MinStockLevel,
            UnitPrice = product.UnitPrice,
            IsActive = product.IsActive,
            CreatedDate = product.CreatedDate,
            UpdatedDate = product.UpdatedDate
        };

        return ApiResponse<ProductDto>.Success(dto, "Ürün başarıyla oluşturuldu.");
    }
}
