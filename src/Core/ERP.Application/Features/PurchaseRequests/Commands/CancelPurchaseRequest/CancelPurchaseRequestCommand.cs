using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.PurchaseRequests.Commands.CancelPurchaseRequest;

public record CancelPurchaseRequestCommand(
    Guid Id,
    string? Reason = null
) : IRequest<ApiResponse<bool>>;

public class CancelPurchaseRequestCommandValidator : AbstractValidator<CancelPurchaseRequestCommand>
{
    public CancelPurchaseRequestCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("İptal edilecek talep ID'si zorunludur.");
    }
}

public class CancelPurchaseRequestCommandHandler : IRequestHandler<CancelPurchaseRequestCommand, ApiResponse<bool>>
{
    private readonly IApplicationDbContext _context;

    public CancelPurchaseRequestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<bool>> Handle(CancelPurchaseRequestCommand request, CancellationToken cancellationToken)
    {
        var purchaseRequest = await _context.PurchaseRequests
            .FirstOrDefaultAsync(pr => pr.Id == request.Id && !pr.IsDeleted, cancellationToken);

        if (purchaseRequest == null)
        {
            throw new NotFoundException("Satın Alma Talebi", request.Id);
        }

        if (purchaseRequest.Status == RequestStatus.Completed)
        {
            throw new BusinessException("Tamamlanmış bir satın alma talebi iptal edilemez.");
        }

        if (purchaseRequest.Status == RequestStatus.Cancelled)
        {
            throw new BusinessException("Talep zaten iptal edilmiş durumda.");
        }

        purchaseRequest.Status = RequestStatus.Cancelled;
        if (!string.IsNullOrWhiteSpace(request.Reason))
        {
            purchaseRequest.Note = string.IsNullOrWhiteSpace(purchaseRequest.Note) 
                ? $"[İPTAL SEBEBİ]: {request.Reason.Trim()}"
                : $"{purchaseRequest.Note} | [İPTAL SEBEBİ]: {request.Reason.Trim()}";
        }

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Success(true, $"'{purchaseRequest.RequestNumber}' numaralı talep başarıyla iptal edildi.");
    }
}
