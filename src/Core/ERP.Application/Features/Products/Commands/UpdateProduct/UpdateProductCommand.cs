using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Features.Products.DTOs;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.Products.Commands.UpdateProduct;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    string? Description,
    string Unit,
    int MinStockLevel,
    decimal UnitPrice,
    bool IsActive,
    Guid? SupplierId = null
) : IRequest<ApiResponse<ProductDto>>;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Ürün Id boş olamaz.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Ürün adı boş olamaz.").MaximumLength(150);
        RuleFor(x => x.Unit).NotEmpty().WithMessage("Birim boş olamaz.");
        RuleFor(x => x.MinStockLevel).GreaterThanOrEqualTo(0);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
    }
}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ApiResponse<ProductDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken);
        if (product == null)
        {
            throw new NotFoundException("Ürün", request.Id);
        }

        product.Name = request.Name.Trim();
        product.Description = request.Description;
        product.Unit = request.Unit.Trim();
        product.MinStockLevel = request.MinStockLevel;
        product.UnitPrice = request.UnitPrice;
        product.IsActive = request.IsActive;
        product.SupplierId = request.SupplierId;

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

        return ApiResponse<ProductDto>.Success(dto, "Ürün bilgileri başarıyla güncellendi.");
    }
}
