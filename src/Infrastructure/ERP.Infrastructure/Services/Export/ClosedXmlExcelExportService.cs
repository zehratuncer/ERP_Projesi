using ClosedXML.Excel;
using ERP.Application.Common.Interfaces;
using ERP.Application.Features.Export.DTOs;

namespace ERP.Infrastructure.Services.Export;

public class ClosedXmlExcelExportService : IExcelExportService
{
    private static readonly XLColor HeaderBgColor = XLColor.FromArgb(30, 41, 59); // Dark Slate Blue
    private static readonly XLColor HeaderTextColor = XLColor.White;
    private static readonly XLColor ZebraRowColor = XLColor.FromArgb(248, 250, 252); // Soft Light Blue-Gray

    public byte[] ExportProductsToExcel(IEnumerable<ProductExportDto> products)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Kırtasiye Ürün Listesi");

        // Başlıklar
        string[] headers =
        [
            "Ürün Kodu",
            "Ürün Adı",
            "Açıklama",
            "Birim",
            "Mevcut Stok",
            "Kritik Stok Seviyesi",
            "Birim Fiyat",
            "Toplam Stok Değeri",
            "Tedarikçi",
            "Durum"
        ];

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = HeaderTextColor;
            cell.Style.Fill.BackgroundColor = HeaderBgColor;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }
        worksheet.Row(1).Height = 26;

        int row = 2;
        foreach (var item in products)
        {
            worksheet.Cell(row, 1).SetValue(item.Code);
            worksheet.Cell(row, 2).SetValue(item.Name);
            worksheet.Cell(row, 3).SetValue(item.Description ?? "-");
            worksheet.Cell(row, 4).SetValue(item.Unit);
            
            var stockCell = worksheet.Cell(row, 5);
            stockCell.SetValue(item.CurrentStock);
            stockCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            var minStockCell = worksheet.Cell(row, 6);
            minStockCell.SetValue(item.MinStockLevel);
            minStockCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            var priceCell = worksheet.Cell(row, 7);
            priceCell.SetValue((double)item.UnitPrice);
            priceCell.Style.NumberFormat.Format = "#,##0.00 \"₺\"";
            priceCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            var totalValCell = worksheet.Cell(row, 8);
            totalValCell.SetValue((double)item.TotalStockValue);
            totalValCell.Style.NumberFormat.Format = "#,##0.00 \"₺\"";
            totalValCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            worksheet.Cell(row, 9).SetValue(item.SupplierName ?? "Belirtilmemiş");
            worksheet.Cell(row, 10).SetValue(item.Status);

            if (row % 2 == 1)
            {
                worksheet.Row(row).Style.Fill.BackgroundColor = ZebraRowColor;
            }

            row++;
        }

        // Kenarlıklar ve otomatik genişlik
        var tableRange = worksheet.Range(1, 1, Math.Max(row - 1, 1), headers.Length);
        tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        tableRange.Style.Border.OutsideBorderColor = XLColor.FromArgb(203, 213, 225);
        tableRange.Style.Border.InsideBorderColor = XLColor.FromArgb(226, 232, 240);

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportStockMovementsToExcel(IEnumerable<StockMovementExportDto> movements)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Stok Hareketleri");

        string[] headers =
        [
            "İşlem Tarihi",
            "Ürün Kodu",
            "Ürün Adı",
            "İşlem Türü",
            "Miktar",
            "Birim",
            "Açıklama",
            "İşlemi Yapan Kullanıcı"
        ];

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = HeaderTextColor;
            cell.Style.Fill.BackgroundColor = HeaderBgColor;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }
        worksheet.Row(1).Height = 26;

        int row = 2;
        foreach (var item in movements)
        {
            var dateCell = worksheet.Cell(row, 1);
            dateCell.SetValue(item.TransactionDate);
            dateCell.Style.DateFormat.Format = "yyyy-MM-dd HH:mm";

            worksheet.Cell(row, 2).SetValue(item.ProductCode);
            worksheet.Cell(row, 3).SetValue(item.ProductName);

            var typeCell = worksheet.Cell(row, 4);
            typeCell.SetValue(item.TransactionType);
            typeCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            var qtyCell = worksheet.Cell(row, 5);
            qtyCell.SetValue(item.Quantity);
            qtyCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            worksheet.Cell(row, 6).SetValue(item.Unit);
            worksheet.Cell(row, 7).SetValue(item.Description ?? "-");
            worksheet.Cell(row, 8).SetValue(item.UserName ?? "Sistem");

            if (row % 2 == 1)
            {
                worksheet.Row(row).Style.Fill.BackgroundColor = ZebraRowColor;
            }

            row++;
        }

        var tableRange = worksheet.Range(1, 1, Math.Max(row - 1, 1), headers.Length);
        tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        tableRange.Style.Border.OutsideBorderColor = XLColor.FromArgb(203, 213, 225);
        tableRange.Style.Border.InsideBorderColor = XLColor.FromArgb(226, 232, 240);

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportGenericReportToExcel(string worksheetTitle, IEnumerable<string> headers, IEnumerable<object?[]> rows)
    {
        using var workbook = new XLWorkbook();
        var safeTitle = string.IsNullOrWhiteSpace(worksheetTitle) ? "Rapor" : worksheetTitle.Length > 30 ? worksheetTitle.Substring(0, 30) : worksheetTitle;
        var worksheet = workbook.Worksheets.Add(safeTitle);

        var headerList = headers.ToList();
        for (int i = 0; i < headerList.Count; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headerList[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = HeaderTextColor;
            cell.Style.Fill.BackgroundColor = HeaderBgColor;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }
        worksheet.Row(1).Height = 26;

        int rowIndex = 2;
        foreach (var r in rows)
        {
            for (int colIndex = 0; colIndex < r.Length; colIndex++)
            {
                var val = r[colIndex];
                var cell = worksheet.Cell(rowIndex, colIndex + 1);

                if (val is null)
                {
                    cell.SetValue("-");
                }
                else if (val is int intVal)
                {
                    cell.SetValue(intVal);
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                }
                else if (val is decimal decVal)
                {
                    cell.SetValue((double)decVal);
                    cell.Style.NumberFormat.Format = "#,##0.00 \"₺\"";
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                }
                else if (val is double dblVal)
                {
                    cell.SetValue(dblVal);
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                }
                else if (val is DateTime dtVal)
                {
                    cell.SetValue(dtVal);
                    cell.Style.DateFormat.Format = "yyyy-MM-dd HH:mm";
                }
                else
                {
                    cell.SetValue(val.ToString());
                }
            }

            if (rowIndex % 2 == 1)
            {
                worksheet.Row(rowIndex).Style.Fill.BackgroundColor = ZebraRowColor;
            }

            rowIndex++;
        }

        var tableRange = worksheet.Range(1, 1, Math.Max(rowIndex - 1, 1), headerList.Count);
        tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        tableRange.Style.Border.OutsideBorderColor = XLColor.FromArgb(203, 213, 225);
        tableRange.Style.Border.InsideBorderColor = XLColor.FromArgb(226, 232, 240);

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
