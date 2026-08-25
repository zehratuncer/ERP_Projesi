using ERP.Application.Common.Interfaces;
using ERP.Application.Features.Export.DTOs;
using ERP.Application.Features.Export.Queries.ExportPurchaseRequestPdf;
using ERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.Export.Queries.ExportStockMovementsExcel;

public record ExportStockMovementsExcelQuery(
    Guid? ProductId = null,
    TransactionType? TransactionType = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<ExportFileResult>;

public class ExportStockMovementsExcelQueryHandler : IRequestHandler<ExportStockMovementsExcelQuery, ExportFileResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IExcelExportService _excelService;

    public ExportStockMovementsExcelQueryHandler(IApplicationDbContext context, IExcelExportService excelService)
    {
        _context = context;
        _excelService = excelService;
    }

    public async Task<ExportFileResult> Handle(ExportStockMovementsExcelQuery request, CancellationToken cancellationToken)
    {
        var query = _context.InventoryTransactions
            .Include(t => t.Product)
            .Include(t => t.User)
            .Where(t => !t.IsDeleted)
            .AsNoTracking();

        if (request.ProductId.HasValue)
        {
            query = query.Where(t => t.ProductId == request.ProductId.Value);
        }

        if (request.TransactionType.HasValue)
        {
            query = query.Where(t => t.TransactionType == request.TransactionType.Value);
        }

        if (request.StartDate.HasValue)
        {
            var start = DateTime.SpecifyKind(request.StartDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(t => t.TransactionDate >= start);
        }

        if (request.EndDate.HasValue)
        {
            var end = DateTime.SpecifyKind(request.EndDate.Value.Date.AddDays(1), DateTimeKind.Utc);
            query = query.Where(t => t.TransactionDate < end);
        }

        var movements = await query
            .OrderByDescending(t => t.TransactionDate)
            .Select(t => new StockMovementExportDto
            {
                TransactionDate = t.TransactionDate,
                ProductCode = t.Product.Code,
                ProductName = t.Product.Name,
                TransactionType = t.TransactionType == TransactionType.In ? "Giriş" : "Çıkış",
                Quantity = t.Quantity,
                Unit = t.Product.Unit,
                Description = t.Description,
                UserName = t.User != null ? t.User.FullName : "Sistem"
            })
            .ToListAsync(cancellationToken);

        var excelBytes = _excelService.ExportStockMovementsToExcel(movements);

        return new ExportFileResult
        {
            FileBytes = excelBytes,
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            FileName = $"Stok_Hareketleri_{DateTime.UtcNow:yyyyMMdd_HHmm}.xlsx"
        };
    }
}
