using Bogus.DataSets;
using GCTL.Core.ViewModels.CRM;
using GCTL.Service.FileHandler;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace GCTL.Service.CRM.LeadsActivities
{
    public class ActivityDocument : IDocument
    {
        public ActivityPDFModel Model { get; }
        private readonly IPdfFileHandler _pdfFileHandlerService;

        public ActivityDocument(ActivityPDFModel model, IPdfFileHandler pdfFileHandlerService)
        {
            Model = model;
            _pdfFileHandlerService = pdfFileHandlerService;
        }
        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(15);
                    //page.Header().Element(ComposeHeader);
                    page.Header().Element(header =>
                    {
                        //if (company != null)
                        //{
                        //    if (company.OrganizationID > 0)
                        //    {


                        //    }
                        //}
                        header.Column(column =>
                        {
                            column.Item().Element(h => _pdfFileHandlerService.ComposeHeader(h, 2, true));
                            column.Item().Element(container =>
                            {
                                container.PaddingBottom(10).Text("Upcoming Activity").AlignCenter().FontSize(16).FontColor(Colors.Blue.Medium);
                            });
                        });

                    });
                    //page.Header().Height(100).Background(Colors.Grey.Lighten3);
                    page.Content().Element(ComposeContent);
                    //page.Content().Background(Colors.Grey.Lighten3);
                    page.Footer().Row(row =>
                    {
                        row.RelativeItem().Column(column =>
                        {
                            // Left-aligned created date
                            column.Item().Element(Cellstyle).AlignLeft().Text($"Created: {DateTime.Now:g}");

                            // Right-aligned page numbers
                            column.Item().Element(Cellstyle).AlignRight().Text(text =>
                            {
                                text.Span("Page ").SemiBold();
                                text.CurrentPageNumber();
                                text.Span(" of ");
                                text.TotalPages();
                            });

                            static IContainer Cellstyle(IContainer container)
                            {
                                return container.DefaultTextStyle(x => x.FontSize(8))
                                                .PaddingVertical(4);
                            }
                        });
                    });
                });

            void ComposeHeader(IContainer container)
            {
                container.Row(row =>
                {
                    row.RelativeItem().AlignCenter().AlignCenter().Column(column =>
                    {
                        column.Item().Text($"{Model.CompanyName}").FontSize(22).SemiBold().AlignCenter();
                        column.Item().Text($"{Model.CompanyAddress}").FontSize(12);
                        column.Item().Text($"Upcoming Activity List").FontSize(8);
                        column.Item().Text($"Date: ").FontSize(8);
                    });
                    if (!string.IsNullOrWhiteSpace(Model.CompanyLogo))
                        row.ConstantItem(50).AlignRight().Image(Model.CompanyLogo);
                });

            }
            void ComposeContent(IContainer container)
            {
                container.Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(25);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });
                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("#").AlignCenter();
                        header.Cell().Element(CellStyle).Text("Lead Name").AlignCenter();
                        header.Cell().Element(CellStyle).Text("Customer Name").AlignCenter();
                        header.Cell().Element(CellStyle).Text("Service Name").AlignCenter();
                        header.Cell().Element(CellStyle).Text("Date & Time").AlignCenter();
                        header.Cell().Element(CellStyle).Text("Note").AlignCenter();
                        header.Cell().Element(CellStyle).Text("Owner Name").AlignCenter();

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.DefaultTextStyle(x => x.SemiBold().FontSize(9)).Background(Colors.Grey.Lighten1).Border(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                        }
                    });

                    foreach( var item in Model.Activities)
                    {
                        table.Cell().Element(CellStyle).Text((Model.Activities.IndexOf(item) + 1).ToString());
                        table.Cell().Element(CellStyle).Text(item.LeadName);
                        table.Cell().Element(CellStyle).Text(item.CustomerName);
                        table.Cell().Element(CellStyle).Text(item.ActivityType);
                        table.Cell().Element(CellStyle).Text(item.ActivityDateTime.Value.ToString("dd/MM/yyyy hh:mm tt"));
                        table.Cell().Element(CellStyle).Text(item.ActivityNote);
                        table.Cell().Element(CellStyle).Text(item.LeadOwner);
                        
                        static IContainer CellStyle(IContainer container)
                        {
                            return container.DefaultTextStyle(x=> x.FontSize(8)).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignMiddle();
                        }
                    }
                });
            }
        }
    }
}
