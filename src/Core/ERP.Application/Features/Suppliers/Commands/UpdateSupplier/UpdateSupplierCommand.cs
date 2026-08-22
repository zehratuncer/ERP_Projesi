using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Features.Suppliers.DTOs;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.Suppliers.Commands.UpdateSupplier;

public record UpdateSupplierCommand(
    Guid Id,
    string Name,
    string? ContactPerson = null,
    string? Email = null,
    string? Phone = null,
    string? Address = null,
    string? TaxNumber = null,
    bool IsActive = true
) : IRequest<ApiResponse<SupplierDto>>;

public class UpdateSupplierCommandValidator : AbstractValidator<UpdateSupplierCommand>
{
    public UpdateSupplierCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Tedarikçi Id boş olamaz.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tedarikçi adı boş bırakılamaz.")
            .MaximumLength(200).WithMessage("Tedarikçi adı en fazla 200 karakter olabilir.");

        RuleFor(x => x.ContactPerson)
            .MaximumLength(150).WithMessage("İletişim yetkilisi en fazla 150 karakter olabilir.");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email)).WithMessage("Geçerli bir e-posta adresi giriniz.")
            .MaximumLength(150).WithMessage("E-posta en fazla 150 karakter olabilir.");

        RuleFor(x => x.Phone)
            .MaximumLength(50).WithMessage("Telefon numarası en fazla 50 karakter olabilir.");

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Adres en fazla 500 karakter olabilir.");

        RuleFor(x => x.TaxNumber)
            .MaximumLength(50).WithMessage("Vergi numarası en fazla 50 karakter olabilir.");
    }
}

public class UpdateSupplierCommandHandler : IRequestHandler<UpdateSupplierCommand, ApiResponse<SupplierDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdateSupplierCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<SupplierDto>> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == request.Id && !s.IsDeleted, cancellationToken);

        if (supplier == null)
        {
            throw new NotFoundException("Tedarikçi", request.Id);
        }

        // Check if name changed and new name is already taken
        if (!supplier.Name.Equals(request.Name.Trim(), StringComparison.CurrentCultureIgnoreCase))
        {
            var exists = await _context.Suppliers
                .AnyAsync(s => s.Name.ToLower() == request.Name.ToLower() && s.Id != request.Id && !s.IsDeleted, cancellationToken);

            if (exists)
            {
                throw new BusinessException($"'{request.Name}' adında başka bir tedarikçi zaten mevcut.");
            }
        }

        supplier.Name = request.Name.Trim();
        supplier.ContactPerson = request.ContactPerson?.Trim();
        supplier.Email = request.Email?.Trim();
        supplier.Phone = request.Phone?.Trim();
        supplier.Address = request.Address?.Trim();
        supplier.TaxNumber = request.TaxNumber?.Trim();
        supplier.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        var productCount = await _context.Products
            .CountAsync(p => p.SupplierId == supplier.Id && !p.IsDeleted, cancellationToken);

        var dto = new SupplierDto
        {
            Id = supplier.Id,
            Name = supplier.Name,
            ContactPerson = supplier.ContactPerson,
            Email = supplier.Email,
            Phone = supplier.Phone,
            Address = supplier.Address,
            TaxNumber = supplier.TaxNumber,
            IsActive = supplier.IsActive,
            ProductCount = productCount,
            CreatedDate = supplier.CreatedDate,
            UpdatedDate = supplier.UpdatedDate
        };

        return ApiResponse<SupplierDto>.Success(dto, "Tedarikçi bilgileri başarıyla güncellendi.");
    }
}
