using ERP.Application.Common.Interfaces;
using ERP.Application.Features.Export.Queries.ExportPurchaseRequestPdf;
using ERP.Application.Features.Reports.Queries.GetCategoryProfitability;
using ERP.Application.Features.Reports.Queries.GetDeadStock;
using ERP.Application.Features.Reports.Queries.GetSeasonalDemandTrends;
using ERP.Application.Features.Reports.Queries.GetStockTurnoverRate;
using ERP.Application.Features.Reports.Queries.GetSupplierPerformance;
using MediatR;

namespace ERP.Application.Features.Export.Queries.ExportReportExcel;

public record ExportReportExcelQuery(string ReportType) : IRequest<ExportFileResult>;

public class ExportReportExcelQueryHandler : IRequestHandler<ExportReportExcelQuery, ExportFileResult>
{
    private readonly IMediator _mediator;
    private readonly IExcelExportService _excelService;

    public ExportReportExcelQueryHandler(IMediator mediator, IExcelExportService excelService)
    {
        _mediator = mediator;
        _excelService = excelService;
    }

    public async Task<ExportFileResult> Handle(ExportReportExcelQuery request, CancellationToken cancellationToken)
    {
        string type = request.ReportType.ToLowerInvariant();
        string title;
        string fileName;
        List<string> headers;
        List<object?[]> rows = new();

        switch (type)
        {
            case "stock-turnover":
            case "turnover":
                title = "Stok Devir Hızı";
                fileName = "Stok_Devir_Hizi_Raporu";
                headers = new() { "Ürün Kodu", "Ürün Adı", "Kategori", "Mevcut Stok", "Satılan Adet", "Toplam Ciro", "Devir Hızı", "Tükenme Süresi (Gün)", "Hız Grubu" };
                var turnoverRes = await _mediator.Send(new GetStockTurnoverRateQuery(), cancellationToken);
                if (turnoverRes.IsSuccess && turnoverRes.Data != null)
                {
                    var items = turnoverRes.Data.TopFastMovingProducts.Concat(turnoverRes.Data.TopSlowMovingProducts);
                    foreach (var i in items)
                    {
                        rows.Add(new object?[] { i.ProductCode, i.ProductName, i.Category, i.CurrentStock, i.TotalSoldQuantity, i.TotalRevenue, i.TurnoverRate, i.DaysToSellOut, i.VelocityCategory });
                    }
                }
                break;

            case "seasonal-trends":
            case "seasonal":
                title = "Sezonluk Talep";
                fileName = "Sezonluk_Talep_Trendleri_Raporu";
                headers = new() { "Ay", "Sezon Etiketi", "Toplam Çıkış Adedi", "Satış Cirosu", "İşlem Sayısı" };
                var seasonRes = await _mediator.Send(new GetSeasonalDemandTrendsQuery(), cancellationToken);
                if (seasonRes.IsSuccess && seasonRes.Data != null)
                {
                    foreach (var m in seasonRes.Data.MonthlyTrends)
                    {
                        rows.Add(new object?[] { m.MonthName, m.SeasonTag, m.TotalOutboundQuantity, m.TotalSalesAmount, m.TransactionCount });
                    }
                }
                break;

            case "dead-stock":
            case "deadstock":
                title = "Atıl ve Ölü Stoklar";
                fileName = "Atil_Olu_Stok_Raporu";
                headers = new() { "Ürün Kodu", "Ürün Adı", "Kategori", "Mevcut Stok", "Birim", "Birim Fiyat", "Bağlanan Sermaye", "Hareketsiz Gün", "Risk Derecesi" };
                var deadRes = await _mediator.Send(new GetDeadStockQuery(90), cancellationToken);
                if (deadRes.IsSuccess && deadRes.Data != null)
                {
                    foreach (var d in deadRes.Data.DeadStockItems)
                    {
                        rows.Add(new object?[] { d.ProductCode, d.ProductName, d.Category, d.CurrentStock, d.Unit, d.UnitPrice, d.TotalTiedUpValue, d.DaysInactive, d.RiskLevel });
                    }
                }
                break;

            case "supplier-performance":
            case "suppliers":
                title = "Tedarikçi Performansı";
                fileName = "Tedarikci_Performans_Raporu";
                headers = new() { "Tedarikçi Adı", "Yetkili", "Ürün Çeşidi", "Tamamlanan Sipariş", "Bekleyen Sipariş", "Toplam Tedarik Hacmi", "Ort. Teslimat (Gün)", "Karşılama %", "Güvenilirlik Puanı", "Derece" };
                var supRes = await _mediator.Send(new GetSupplierPerformanceQuery(), cancellationToken);
                if (supRes.IsSuccess && supRes.Data != null)
                {
                    foreach (var s in supRes.Data.Suppliers)
                    {
                        rows.Add(new object?[] { s.SupplierName, s.ContactPerson ?? s.Email ?? "-", s.SuppliedProductCount, s.CompletedRequestCount, s.PendingRequestCount, s.TotalSuppliedAmount, s.AverageDeliveryDays, s.FulfillmentRate, s.ReliabilityScore, s.PerformanceGrade });
                    }
                }
                break;

            case "category-analytics":
            case "category":
            case "profitability":
            default:
                title = "Kategori Kârlılık";
                fileName = "Kategori_Karlilik_Raporu";
                headers = new() { "Kategori Adı", "Ürün Çeşidi", "Satılan Adet", "Toplam Ciro", "Tahmini Maliyet", "Brüt Kâr", "Kâr Marjı %", "Mevcut Stok Değeri" };
                var catRes = await _mediator.Send(new GetCategoryProfitabilityQuery(), cancellationToken);
                if (catRes.IsSuccess && catRes.Data != null)
                {
                    foreach (var c in catRes.Data.Categories)
                    {
                        rows.Add(new object?[] { c.CategoryName, c.ProductCount, c.TotalUnitsSold, c.TotalRevenue, c.EstimatedCost, c.GrossProfit, c.ProfitMarginPercentage, c.CurrentStockValue });
                    }
                }
                break;
        }

        var excelBytes = _excelService.ExportGenericReportToExcel(title, headers, rows);

        return new ExportFileResult
        {
            FileBytes = excelBytes,
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            FileName = $"{fileName}_{DateTime.UtcNow:yyyyMMdd_HHmm}.xlsx"
        };
    }
}
