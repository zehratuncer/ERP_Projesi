using ERP.Application.Common.Interfaces;
using ERP.Application.Features.Export.DTOs;
using ERP.Application.Features.Export.Queries.ExportPurchaseRequestPdf;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Features.Export.Queries.ExportProductsExcel;

public record ExportProductsExcelQuery(
    string? SearchTerm = null,
    bool? IsCriticalOnly = false
) : IRequest<ExportFileResult>;

public class ExportProductsExcelQueryHandler : IRequestHandler<ExportProductsExcelQuery, ExportFileResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IExcelExportService _excelService;

    public ExportProductsExcelQueryHandler(IApplicationDbContext context, IExcelExportService excelService)
    {
        _context = context;
        _excelService = excelService;
    }

    public async Task<ExportFileResult> Handle(ExportProductsExcelQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Products
            .Include(p => p.Supplier)
            .Where(p => !p.IsDeleted)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(search) || p.Code.ToLower().Contains(search));
        }

        if (request.IsCriticalOnly == true)
        {
            query = query.Where(p => p.CurrentStock <= p.MinStockLevel);
        }

        var products = await query
            .OrderBy(p => p.Code)
            .Select(p => new ProductExportDto
            {
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                Unit = p.Unit,
                CurrentStock = p.CurrentStock,
                MinStockLevel = p.MinStockLevel,
                UnitPrice = p.UnitPrice,
                TotalStockValue = p.CurrentStock * p.UnitPrice,
                SupplierName = p.Supplier != null ? p.Supplier.Name : null,
                Status = p.IsActive ? "Aktif" : "Pasif"
            })
            .ToListAsync(cancellationToken);

        var excelBytes = _excelService.ExportProductsToExcel(products);

        return new ExportFileResult
        {
            FileBytes = excelBytes,
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            FileName = $"Urun_Stok_Listesi_{DateTime.UtcNow:yyyyMMdd_HHmm}.xlsx"
        };
    }
}
