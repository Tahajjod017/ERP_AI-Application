using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.RequisitionDistribution.Purchase.CreatePO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GCTL.Service.ReportService
{
    public class ReportService : IReportService
    {
        private readonly HttpClient _httpClient;

        public ReportService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }




        public async Task<byte[]> GeneratePOPdfAsync(PurchaseOrderSubmissionViewModel model)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x
                        .FontFamily("Segoe UI")
                        .FontSize(11)
                        .LineHeight(1.3f)
                        .Fallback(y => y.FontFamily(Fonts.Tahoma)));

                    page.Content().Border(1).BorderColor("#ddd").MaxWidth(800).Element(content =>
                    {
                        content.Column(column =>
                        {
                            // Header
                            column.Item().Background(Colors.Grey.Darken4).Padding(10).Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("GCTL InfoSys")
                                        .FontSize(20).FontColor(Colors.White).Bold();

                                    col.Item().Text(text =>
                                    {
                                        text.Span("House-42 (5th Floor) Road-10, Sector-4, Uttara, Dhaka-1230\n");
                                        text.Span("info@gctlinfosys.com | +88 01795-788488").FontSize(10);
                                    });
                                });

                                row.RelativeItem().AlignRight().Column(col =>
                                {
                                    col.Item().PaddingTop(13)
                                        .Text("PURCHASE ORDER")
                                        .FontSize(26).FontColor(Colors.White).Bold();

                                    col.Item().Text(model.PoNumber ?? "PO-2025-0001")
                                        .FontSize(15).FontColor("#f39c12").Bold();

                                    col.Item().Padding(4)
                                        .Background("#555555")
                                        .Text("DRAFT")
                                        .FontSize(10).FontColor(Colors.White).Bold();
                                });
                            });

                            // Vendor & Shipping Info
                            column.Item().Background("#f9fafc").Padding(15).Row(row =>
                            {
                                row.RelativeItem().Border(1).BorderColor("#e1e5e9").Padding(12).Column(col =>
                                {
                                    col.Item().BorderBottom(1).BorderColor("#3498db")
                                        .Text("Vendor Information")
                                        .FontSize(12).FontColor("#2c3e50");

                                    col.Item().PaddingTop(8).Text(text =>
                                    {
                                        text.Span(model.BillingInfo?.Name ?? "ABC Supplies Ltd.").Bold();
                                        text.Span("\n" + (model.BillingInfo?.Address ?? "123 Business Street, Dhaka-1212, Bangladesh"));
                                        text.Span($"\nContact: {model.BillingInfo?.Name ?? "John Doe"}");
                                        text.Span($"\nPhone: {model.BillingInfo?.Phone ?? "+88 01234-567890"}");
                                    });
                                });

                                row.ConstantItem(20);

                                row.RelativeItem().Border(1).BorderColor("#e1e5e9").Padding(12).Column(col =>
                                {
                                    col.Item().BorderBottom(1).BorderColor("#3498db")
                                        .Text("Ship To")
                                        .FontSize(12).FontColor("#2c3e50");

                                    col.Item().PaddingTop(8).Text(text =>
                                    {
                                        text.Span(model.ShippingInfo.Address ?? "GCTL InfoSys - Warehouse").Bold();
                                        text.Span("\n" + (model.ShippingInfo?.Address ?? "House-42 (5th Floor), Sector-4, Uttara, Dhaka-1230"));
                                        text.Span($"\nContact: {model.ShippingInfo?.Contact ?? "Warehouse Manager"}");
                                    });
                                });
                            });

                            // PO Details
                            column.Item().Padding(15).Element(cont => //, 20
                            {
                                cont.Row(row =>
                                {
                                    var details = new[]
                                    {
                                        ("PO Date", model.PoDate ?? "Sept 23, 2025"),
                                        ("Due Date", model.DueDate ?? "Oct 15, 2025"),
                                        ("Work Order", model.WorkOrderNo ?? "WO-2025-045"),
                                        ("Reference", model.OtherReference ?? "REF-IT-EQUIPMENT")
                                    };

                                    foreach (var detail in details)
                                    {
                                        row.AutoItem().MinWidth(140).BorderLeft(3).BorderColor("#3498db")
                                            .Background(Colors.White).Padding(8).Column(col =>
                                            {
                                                col.Item().Text(detail.Item1.ToUpperInvariant())
                                                    .FontSize(9).FontColor("#777");
                                                col.Item().Text(detail.Item2)
                                                    .FontSize(11).FontColor("#2c3e50").Bold();
                                            });
                                    }
                                });
                            });

                            // Items
                            column.Item().Background("#f9fafc").Padding(15).Column(col =>
                            {
                                col.Item().BorderBottom(1).BorderColor("#3498db")
                                    .Text("Order Items").FontSize(14);

                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.ConstantColumn(50);
                                        columns.ConstantColumn(80);
                                        columns.ConstantColumn(80);
                                    });

                                    // Table Header
                                    table.Header(header =>
                                    {
                                        header.Cell().Background("#2c3e50").Padding(8)
                                            .Text("Item").FontSize(9).FontColor(Colors.White);
                                        header.Cell().Background("#2c3e50").Padding(8)
                                            .Text("Description").FontSize(9).FontColor(Colors.White);
                                        header.Cell().Background("#2c3e50").Padding(8)
                                            .Text("Unit").FontSize(9).FontColor(Colors.White);
                                        header.Cell().Background("#2c3e50").Padding(8).AlignCenter()
                                            .Text("Qty").FontSize(9).FontColor(Colors.White);
                                        header.Cell().Background("#2c3e50").Padding(8).AlignRight()
                                            .Text("Price").FontSize(9).FontColor(Colors.White);
                                        header.Cell().Background("#2c3e50").Padding(8).AlignRight()
                                            .Text("Total").FontSize(9).FontColor(Colors.White);
                                    });

                                    // Table Rows
                                    foreach (var item in model.Items )
                                    {
                                        table.Cell().Padding(6).Background("#ecf0f1")
                                            .Text(item.SerialNumber).FontSize(9).FontFamily(Fonts.Courier);
                                        table.Cell().Padding(6).Text(item.ItemName).FontSize(10).FontColor("#555");
                                        table.Cell().Padding(6).Text(item.Unit).FontSize(10).FontColor("#555");
                                        table.Cell().Padding(6).AlignCenter().Text(item.Quantity.ToString()).FontSize(10).FontColor("#555");
                                        table.Cell().Padding(6).AlignRight().Text($"৳{item.UnitPrice:N0}").FontSize(10).FontColor("#555");
                                        table.Cell().Padding(6).AlignRight().Text($"৳{item.TotalAmount:N0}").FontSize(10).FontColor("#555");
                                    }
                                });

                                // Summary
                                col.Item().AlignRight().PaddingTop(10).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.ConstantColumn(80);
                                    });

                                    var subtotal = model.Items?.Sum(i => i.TotalAmount) ?? 495000;
                                    var taxAmount = subtotal * (model.TaxRate / 100);
                                    var grandTotal = subtotal + taxAmount;

                                    table.Cell().Padding(6).Text("Subtotal:").FontSize(10).FontColor("#555");
                                    table.Cell().Padding(6).AlignRight().Text($"৳{subtotal:N0}").FontSize(10).FontColor("#555");

                                    table.Cell().Padding(6).Text($"Tax ({model.TaxRate}%):").FontSize(10).FontColor("#555");
                                    table.Cell().Padding(6).AlignRight().Text($"৳{taxAmount:N0}").FontSize(10).FontColor("#555");

                                    table.Cell().Padding(6).Background("#27ae60")
                                        .Text("Grand Total:").FontSize(10).FontColor(Colors.White).Bold();
                                    table.Cell().Padding(6).Background("#27ae60")
                                        .AlignRight().Text($"৳{grandTotal:N0}").FontSize(10).FontColor(Colors.White).Bold();
                                });
                            });

                            // Notes & Terms
                            column.Item().Padding(12).Row(row =>
                            {
                                row.RelativeItem().Background("#fff5f5").Border(1).BorderColor("#fed7d7").Padding(10).Column(col =>
                                {
                                    col.Item().Text("Notes").FontSize(10).FontColor("#2c3e50");
                                    col.Item().Text(model.Note ?? "Please deliver items in original packaging. Setup required for desktops.").FontSize(10).FontColor("#555");
                                });

                                row.ConstantItem(20);

                                row.RelativeItem().Background("#f7fafc").Border(1).BorderColor("#e2e8f0").Padding(10).Column(col =>
                                {
                                    col.Item().Text("Terms").FontSize(10).FontColor("#2c3e50");
                                    col.Item().Text(model.Terms ?? "Payment Net 30 | 1yr Warranty | Free Dhaka delivery").FontSize(10).FontColor("#555");
                                });
                            });

                            // Barcode & Signature
                            column.Item().Background(Colors.White).Padding(10).Row(row =>
                            {
                                row.RelativeItem().AlignLeft().Border(1).BorderColor("#3498db").Padding(8).Background("#f9fafc").Element(async cont =>
                                {
                                    try
                                    {
                                        var barcodeUrl = $"https://bwipjs-api.metafloor.com/?bcid=code128&text={model.PoNumber ?? "PO-2025-0001"}&scale=2&height=50";
                                        var response = await _httpClient.GetAsync(barcodeUrl);

                                        if (response.IsSuccessStatusCode)
                                        {
                                            var barcodeBytes = await response.Content.ReadAsByteArrayAsync();
                                            cont.Width(180).Height(70).Image(barcodeBytes).FitArea();
                                        }
                                        else
                                        {
                                            cont.Text("Barcode unavailable").FontSize(10).FontColor(Colors.Red.Medium);
                                        }
                                    }
                                    catch
                                    {
                                        cont.Text("Barcode unavailable").FontSize(10).FontColor(Colors.Red.Medium);
                                    }
                                });

                                row.RelativeItem().AlignRight().PaddingTop(8).BorderTop(1).BorderColor("#aaa")
                                    .Text("Authorized Signature __________________")
                                    .FontSize(9).FontColor("#555");
                            });

                            // Footer
                            column.Item().Background("#2c3e50").Padding(10).AlignCenter()
                                .Text($"Generated on {DateTime.Now:MMM dd, yyyy} | Page 1 of 1")
                                .FontSize(9).FontColor(Colors.White);
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }
    }

   
    
}
