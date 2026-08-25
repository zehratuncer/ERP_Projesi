using ERP.Application.Features.Export.DTOs;

namespace ERP.Application.Common.Interfaces;

public interface IExcelExportService
{
    byte[] ExportProductsToExcel(IEnumerable<ProductExportDto> products);
    byte[] ExportStockMovementsToExcel(IEnumerable<StockMovementExportDto> movements);
    byte[] ExportGenericReportToExcel(string worksheetTitle, IEnumerable<string> headers, IEnumerable<object?[]> rows);
}
