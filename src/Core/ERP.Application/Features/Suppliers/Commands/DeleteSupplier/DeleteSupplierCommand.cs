using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.Suppliers.Commands.DeleteSupplier;

public record DeleteSupplierCommand(Guid Id) : IRequest<ApiResponse<bool>>;

public class DeleteSupplierCommandHandler : IRequestHandler<DeleteSupplierCommand, ApiResponse<bool>>
{
    private readonly IApplicationDbContext _context;

    public DeleteSupplierCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == request.Id && !s.IsDeleted, cancellationToken);

        if (supplier == null)
        {
            throw new NotFoundException("Tedarikçi", request.Id);
        }

        // Soft-delete supplier
        supplier.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Success(true, "Tedarikçi başarıyla silindi.");
    }
}
