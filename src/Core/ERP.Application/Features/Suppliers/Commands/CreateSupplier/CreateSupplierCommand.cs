using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Features.Suppliers.DTOs;
using ERP.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.Suppliers.Commands.CreateSupplier;

public record CreateSupplierCommand(
    string Name,
    string? ContactPerson = null,
    string? Email = null,
    string? Phone = null,
    string? Address = null,
    string? TaxNumber = null
) : IRequest<ApiResponse<SupplierDto>>;

public class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierCommandValidator()
    {
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

public class CreateSupplierCommandHandler : IRequestHandler<CreateSupplierCommand, ApiResponse<SupplierDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateSupplierCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<SupplierDto>> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.Suppliers
            .AnyAsync(s => s.Name.ToLower() == request.Name.ToLower() && !s.IsDeleted, cancellationToken);

        if (exists)
        {
            throw new BusinessException($"'{request.Name}' adında bir tedarikçi zaten mevcut.");
        }

        var supplier = new Supplier
        {
            Name = request.Name.Trim(),
            ContactPerson = request.ContactPerson?.Trim(),
            Email = request.Email?.Trim(),
            Phone = request.Phone?.Trim(),
            Address = request.Address?.Trim(),
            TaxNumber = request.TaxNumber?.Trim(),
            IsActive = true
        };

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync(cancellationToken);

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
            ProductCount = 0,
            CreatedDate = supplier.CreatedDate,
            UpdatedDate = supplier.UpdatedDate
        };

        return ApiResponse<SupplierDto>.Success(dto, "Tedarikçi başarıyla oluşturuldu.");
    }
}
