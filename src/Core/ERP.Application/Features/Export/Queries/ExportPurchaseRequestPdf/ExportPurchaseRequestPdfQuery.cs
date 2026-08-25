using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Features.Export.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.Export.Queries.ExportPurchaseRequestPdf;

public record ExportPurchaseRequestPdfQuery(Guid Id) : IRequest<ExportFileResult>;

public class ExportFileResult
{
    public byte[] FileBytes { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/pdf";
    public string FileName { get; set; } = string.Empty;
}

public class ExportPurchaseRequestPdfQueryHandler : IRequestHandler<ExportPurchaseRequestPdfQuery, ExportFileResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IPdfReportService _pdfService;

    public ExportPurchaseRequestPdfQueryHandler(IApplicationDbContext context, IPdfReportService pdfService)
    {
        _context = context;
        _pdfService = pdfService;
    }

    public async Task<ExportFileResult> Handle(ExportPurchaseRequestPdfQuery request, CancellationToken cancellationToken)
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

        var pdfDto = new PurchaseRequestPdfDto
        {
            Id = purchaseRequest.Id,
            RequestNumber = purchaseRequest.RequestNumber,
            Department = purchaseRequest.Department,
            RequesterName = purchaseRequest.RequesterUser?.FullName ?? "Talep Sahibi",
            Priority = purchaseRequest.Priority.ToString(),
            Status = purchaseRequest.Status.ToString(),
            CreatedDate = purchaseRequest.CreatedDate,
            Description = purchaseRequest.Note,
            TotalEstimatedAmount = purchaseRequest.TotalEstimatedAmount,
            Items = purchaseRequest.Items
                .Where(i => !i.IsDeleted)
                .Select((i, idx) => new PurchaseRequestPdfItemDto
                {
                    ItemIndex = idx + 1,
                    ProductCode = i.Product.Code,
                    ProductName = i.Product.Name,
                    Quantity = i.RequestedQuantity,
                    Unit = i.Unit,
                    EstimatedUnitPrice = i.EstimatedUnitPrice,
                    EstimatedTotalPrice = i.EstimatedTotalPrice,
                    Note = i.Notes
                })
                .ToList(),

            Approvals = purchaseRequest.ApprovalHistories
                .Where(ah => !ah.IsDeleted)
                .OrderBy(ah => ah.StepNumber)
                .Select(ah => new PurchaseRequestPdfApprovalDto
                {
                    StepNumber = ah.StepNumber,
                    StepName = ah.StepName,
                    ApproverName = ah.ApproverUser?.FullName ?? "Yetkili",
                    Action = ah.Action.ToString(),
                    ActionDate = ah.ActionDate,
                    Comment = ah.Comment
                })
                .ToList()
        };

        var bytes = _pdfService.GeneratePurchaseRequestPdf(pdfDto);

        return new ExportFileResult
        {
            FileBytes = bytes,
            ContentType = "application/pdf",
            FileName = $"Satin_Alma_Talebi_{purchaseRequest.RequestNumber}.pdf"
        };
    }
}
