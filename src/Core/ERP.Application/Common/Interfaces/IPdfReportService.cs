using ERP.Application.Features.Export.DTOs;

namespace ERP.Application.Common.Interfaces;

public interface IPdfReportService
{
    byte[] GeneratePurchaseRequestPdf(PurchaseRequestPdfDto requestDto);
    byte[] GenerateStockReceiptPdf(StockReceiptPdfDto receiptDto);
}
