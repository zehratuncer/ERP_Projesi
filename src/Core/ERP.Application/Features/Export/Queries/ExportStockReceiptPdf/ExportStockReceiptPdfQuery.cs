using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Features.Export.DTOs;
using ERP.Application.Features.Export.Queries.ExportPurchaseRequestPdf;
using ERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.Export.Queries.ExportStockReceiptPdf;

public record ExportStockReceiptPdfQuery(Guid TransactionId) : IRequest<ExportFileResult>;

public class ExportStockReceiptPdfQueryHandler : IRequestHandler<ExportStockReceiptPdfQuery, ExportFileResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IPdfReportService _pdfService;

    public ExportStockReceiptPdfQueryHandler(IApplicationDbContext context, IPdfReportService pdfService)
    {
        _context = context;
        _pdfService = pdfService;
    }

    public async Task<ExportFileResult> Handle(ExportStockReceiptPdfQuery request, CancellationToken cancellationToken)
    {
        var transaction = await _context.InventoryTransactions
            .Include(t => t.Product)
                .ThenInclude(p => p.Supplier)
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId && !t.IsDeleted, cancellationToken);

        if (transaction == null)
        {
            throw new NotFoundException("Stok Hareketi", request.TransactionId);
        }

        var receiptDto = new StockReceiptPdfDto
        {
            TransactionId = transaction.Id,
            ReceiptNumber = $"STK-{transaction.TransactionDate:yyyyMMdd}-{transaction.Id.ToString()[..6].ToUpper()}",
            TransactionDate = transaction.TransactionDate,
            TransactionType = transaction.TransactionType == TransactionType.In ? "Stok Girişi (Mal Kabul)" : "Stok Çıkışı / Sarf",
            ProductCode = transaction.Product.Code,
            ProductName = transaction.Product.Name,
            Unit = transaction.Product.Unit,
            Quantity = transaction.Quantity,
            UnitPrice = transaction.Product.UnitPrice,
            TotalAmount = transaction.Quantity * transaction.Product.UnitPrice,
            Description = transaction.Description,
            OperatorName = transaction.User?.FullName ?? "Sistem Yetkilisi",
            SupplierOrDepartment = transaction.Product.Supplier?.Name ?? "Depo Yönetimi"
        };

        var bytes = _pdfService.GenerateStockReceiptPdf(receiptDto);

        return new ExportFileResult
        {
            FileBytes = bytes,
            ContentType = "application/pdf",
            FileName = $"Stok_Fisi_{receiptDto.ReceiptNumber}.pdf"
        };
    }
}
