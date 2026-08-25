using ERP.Application.Common.Interfaces;
using ERP.Application.Features.Export.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ERP.Infrastructure.Services.Export;

public class QuestPdfReportService : IPdfReportService
{
    static QuestPdfReportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GeneratePurchaseRequestPdf(PurchaseRequestPdfDto requestDto)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial).FontColor(Colors.Grey.Darken3));

                // 1. Header
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(titleCol =>
                        {
                            titleCol.Item().Text("GÖKÇE KIRTASİYE & OFİS SİSTEMLERİ")
                                .FontSize(16)
                                .SemiBold()
                                .FontColor(Colors.Blue.Darken3);

                            titleCol.Item().Text("Kurumsal Kırtasiye ERP & Tedarik Yönetim Sistemi")
                                .FontSize(9)
                                .FontColor(Colors.Grey.Medium);
                        });

                        row.RelativeItem().AlignRight().Column(metaCol =>
                        {
                            metaCol.Item().Text("SATIN ALMA TALEP FORMU")
                                .FontSize(14)
                                .Bold()
                                .FontColor(Colors.Grey.Darken4);

                            metaCol.Item().Text($"Talep No: {requestDto.RequestNumber}")
                                .FontSize(10)
                                .SemiBold()
                                .FontColor(Colors.Blue.Medium);

                            metaCol.Item().Text($"Tarih: {requestDto.CreatedDate:dd.MM.yyyy HH:mm}")
                                .FontSize(9)
                                .FontColor(Colors.Grey.Darken1);
                        });
                    });

                    col.Item().PaddingTop(8).LineHorizontal(1.5f).LineColor(Colors.Blue.Darken2);
                });

                // 2. Content
                page.Content().PaddingVertical(15).Column(col =>
                {
                    // Info Cards Grid
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                        {
                            c.Item().Text("TALEP BİLGİLERİ").FontSize(9).Bold().FontColor(Colors.Blue.Darken3);
                            c.Item().PaddingTop(3).Text($"Departman: {requestDto.Department}");
                            c.Item().Text($"Talep Eden: {requestDto.RequesterName}");
                            c.Item().Text($"Öncelik: {requestDto.Priority}");
                        });

                        row.ConstantItem(15);

                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                        {
                            c.Item().Text("DURUM & BÜTÇE").FontSize(9).Bold().FontColor(Colors.Blue.Darken3);
                            c.Item().PaddingTop(3).Text($"Mevcut Durum: {requestDto.Status}");
                            c.Item().Text($"Tahmini Toplam: {requestDto.TotalEstimatedAmount:N2} ₺").Bold().FontColor(Colors.Green.Darken2);
                            c.Item().Text($"Kalem Sayısı: {requestDto.Items.Count} Çeşit");
                        });
                    });

                    if (!string.IsNullOrWhiteSpace(requestDto.Description))
                    {
                        col.Item().PaddingTop(10).Border(1).BorderColor(Colors.Grey.Lighten3).Background(Colors.Grey.Lighten5).Padding(8).Column(c =>
                        {
                            c.Item().Text("Talep Gerekçesi / Açıklama:").FontSize(9).Bold();
                            c.Item().Text(requestDto.Description).FontSize(9);
                        });
                    }

                    // Items Table
                    col.Item().PaddingTop(15).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30); // #
                            columns.ConstantColumn(80); // Kod
                            columns.RelativeColumn(3);  // Ürün Adı
                            columns.ConstantColumn(50); // Miktar
                            columns.ConstantColumn(50); // Birim
                            columns.ConstantColumn(80); // Birim Fiyat
                            columns.ConstantColumn(90); // Toplam
                        });

                        // Header
                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("#").FontSize(9).Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Ürün Kodu").FontSize(9).Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Ürün Adı").FontSize(9).Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).AlignCenter().Text("Miktar").FontSize(9).Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).AlignCenter().Text("Birim").FontSize(9).Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).AlignRight().Text("Birim Fiyat").FontSize(9).Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).AlignRight().Text("Toplam").FontSize(9).Bold().FontColor(Colors.White);
                        });

                        // Rows
                        int index = 1;
                        foreach (var item in requestDto.Items)
                        {
                            var bg = index % 2 == 0 ? Colors.Grey.Lighten5 : Colors.White;

                            table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(index.ToString()).FontSize(9);
                            table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.ProductCode).FontSize(9);
                            table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.ProductName).FontSize(9).SemiBold();
                            table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignCenter().Text(item.Quantity.ToString()).FontSize(9);
                            table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignCenter().Text(item.Unit).FontSize(9);
                            table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignRight().Text($"{item.EstimatedUnitPrice:N2} ₺").FontSize(9);
                            table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignRight().Text($"{item.EstimatedTotalPrice:N2} ₺").FontSize(9).Bold();

                            index++;
                        }
                    });

                    // Summary Total Box
                    col.Item().PaddingTop(8).AlignRight().Column(c =>
                    {
                        c.Item().Border(1).BorderColor(Colors.Blue.Lighten3).Background(Colors.Blue.Lighten5).Padding(6).Row(r =>
                        {
                            r.AutoItem().Text("GENEL TOPLAM TUTAR: ").FontSize(10).Bold().FontColor(Colors.Blue.Darken3);
                            r.AutoItem().PaddingLeft(10).Text($"{requestDto.TotalEstimatedAmount:N2} ₺").FontSize(11).Bold().FontColor(Colors.Blue.Darken4);
                        });
                    });

                    // Approval History & Signatures
                    col.Item().PaddingTop(20).Column(signCol =>
                    {
                        signCol.Item().Text("ONAY VE İMZA AKIŞI").FontSize(10).Bold().FontColor(Colors.Grey.Darken4);
                        signCol.Item().PaddingTop(5).Row(signRow =>
                        {
                            signRow.RelativeItem().Border(1).BorderColor(Colors.Grey.Medium).Padding(8).Height(90).Column(s1 =>
                            {
                                s1.Item().Text("1. Kademe: Şube / Birim Müdürü").FontSize(9).Bold();
                                var app1 = requestDto.Approvals.FirstOrDefault(a => a.StepNumber == 1);
                                if (app1 != null)
                                {
                                    s1.Item().Text($"Onaylayan: {app1.ApproverName}").FontSize(8);
                                    s1.Item().Text($"Tarih: {app1.ActionDate:dd.MM.yyyy HH:mm}").FontSize(8);
                                    s1.Item().Text($"Durum: {app1.Action}").FontSize(8).Bold().FontColor(Colors.Green.Darken2);
                                }
                                else
                                {
                                    s1.Item().PaddingTop(35).AlignCenter().Text("İmza & Kaşe").FontSize(8).FontColor(Colors.Grey.Medium);
                                }
                            });

                            signRow.ConstantItem(20);

                            signRow.RelativeItem().Border(1).BorderColor(Colors.Grey.Medium).Padding(8).Height(90).Column(s2 =>
                            {
                                s2.Item().Text("2. Kademe: Genel Satın Alma Direktörü").FontSize(9).Bold();
                                var app2 = requestDto.Approvals.FirstOrDefault(a => a.StepNumber == 2);
                                if (app2 != null)
                                {
                                    s2.Item().Text($"Onaylayan: {app2.ApproverName}").FontSize(8);
                                    s2.Item().Text($"Tarih: {app2.ActionDate:dd.MM.yyyy HH:mm}").FontSize(8);
                                    s2.Item().Text($"Durum: {app2.Action}").FontSize(8).Bold().FontColor(Colors.Green.Darken2);
                                }
                                else
                                {
                                    s2.Item().PaddingTop(35).AlignCenter().Text("İmza & Kaşe").FontSize(8).FontColor(Colors.Grey.Medium);
                                }
                            });
                        });
                    });
                });

                // 3. Footer
                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                    col.Item().PaddingTop(5).Row(row =>
                    {
                        row.RelativeItem().Text("Bu belge Gökçe Kırtasiye ERP Sistemi tarafından elektronik olarak üretilmiştir.")
                            .FontSize(8)
                            .FontColor(Colors.Grey.Medium);

                        row.RelativeItem().AlignRight().Text(text =>
                        {
                            text.Span("Sayfa ");
                            text.CurrentPageNumber();
                            text.Span(" / ");
                            text.TotalPages();
                        });
                    });
                });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateStockReceiptPdf(StockReceiptPdfDto receiptDto)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5.Landscape());
                page.Margin(25);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Arial).FontColor(Colors.Grey.Darken3));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("GÖKÇE KIRTASİYE & OFİS SİSTEMLERİ").FontSize(13).Bold().FontColor(Colors.Blue.Darken3);
                            c.Item().Text("DEPO MAL KABUL & STOK HAREKET FİŞİ").FontSize(11).SemiBold().FontColor(Colors.Grey.Darken3);
                        });

                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text($"Fiş No: {receiptDto.ReceiptNumber}").FontSize(10).Bold().FontColor(Colors.Blue.Medium);
                            c.Item().Text($"Tarih: {receiptDto.TransactionDate:dd.MM.yyyy HH:mm}").FontSize(8);
                            c.Item().Text($"İşlem: {receiptDto.TransactionType}").FontSize(8).Bold().FontColor(Colors.Green.Darken2);
                        });
                    });
                    col.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Blue.Darken2);
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(80);
                            columns.RelativeColumn(2);
                            columns.ConstantColumn(60);
                            columns.ConstantColumn(50);
                            columns.ConstantColumn(70);
                            columns.ConstantColumn(80);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Darken3).Padding(4).Text("Ürün Kodu").FontSize(8).Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken3).Padding(4).Text("Ürün Adı").FontSize(8).Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken3).Padding(4).AlignCenter().Text("Miktar").FontSize(8).Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken3).Padding(4).AlignCenter().Text("Birim").FontSize(8).Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken3).Padding(4).AlignRight().Text("Birim Fiyat").FontSize(8).Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken3).Padding(4).AlignRight().Text("Toplam Değer").FontSize(8).Bold().FontColor(Colors.White);
                        });

                        table.Cell().Padding(4).Text(receiptDto.ProductCode);
                        table.Cell().Padding(4).Text(receiptDto.ProductName).Bold();
                        table.Cell().Padding(4).AlignCenter().Text(receiptDto.Quantity.ToString()).Bold();
                        table.Cell().Padding(4).AlignCenter().Text(receiptDto.Unit);
                        table.Cell().Padding(4).AlignRight().Text($"{receiptDto.UnitPrice:N2} ₺");
                        table.Cell().Padding(4).AlignRight().Text($"{receiptDto.TotalAmount:N2} ₺").Bold();
                    });

                    if (!string.IsNullOrWhiteSpace(receiptDto.Description))
                    {
                        col.Item().PaddingTop(8).Text($"İşlem Açıklaması: {receiptDto.Description}").FontSize(8).Italic();
                    }

                    col.Item().PaddingTop(15).Row(row =>
                    {
                        row.RelativeItem().Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(6).Height(55).Column(c =>
                        {
                            c.Item().Text("Teslim Eden (Tedarikçi / Personel)").FontSize(8).Bold();
                            c.Item().PaddingTop(25).AlignCenter().Text("İmza").FontSize(7).FontColor(Colors.Grey.Medium);
                        });

                        row.ConstantItem(20);

                        row.RelativeItem().Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(6).Height(55).Column(c =>
                        {
                            c.Item().Text($"Teslim Alan (Depo: {receiptDto.OperatorName ?? "Sorumlu"})").FontSize(8).Bold();
                            c.Item().PaddingTop(25).AlignCenter().Text("Kaşe & İmza").FontSize(7).FontColor(Colors.Grey.Medium);
                        });
                    });
                });

                page.Footer().AlignCenter().Text("Gökçe Kırtasiye ERP - Stok Yönetim Modülü").FontSize(7).FontColor(Colors.Grey.Medium);
            });
        });

        return document.GeneratePdf();
    }
}
