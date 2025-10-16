using GCTL.Core.ViewModels.CRM;
using GCTL.Service.FileHandler;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GCTL.Service.CRM.LeadsActivities
{
    public class ActivityDocument : IDocument
    {
        public List<ActivityPDFModel> Model { get; }
        private readonly IPdfFileHandler _pdfFileHandlerService;

        public ActivityDocument(List<ActivityPDFModel> model, IPdfFileHandler pdfFileHandlerService)
        {
            Model = model;
            _pdfFileHandlerService = pdfFileHandlerService;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(20);

                // Header
                //page.Header().Element(header =>
                //{
                //    header.Column(column =>
                //    {
                //        column.Item().Element(h => _pdfFileHandlerService.ComposeHeader(h, 2, true));
                //        column.Item().Element(container =>
                //        {
                //            container
                //                .PaddingBottom(10)
                //                .Text("Upcoming Activities Report")
                //                .AlignCenter()
                //                .FontSize(18)
                //                .FontColor(Colors.Blue.Medium)
                //                .Bold();
                //        });
                //    });
                //});

                // Content (each employee section)
                page.Content().PaddingVertical(10).Column(column =>
                {
                    foreach (var model in Model)
                    {
                        column.Item().Element(_ => ComposeEmployeeSection(_, model));
                        column.Item().PageBreak(); // add page break between employees
                    }
                });

                // Footer
                page.Footer().Row(row =>
                {
                    row.RelativeItem().Column(column =>
                    {
                        column.Item().Element(CellStyle).AlignLeft().Text($"Created: {DateTime.Now:g}");
                        column.Item().Element(CellStyle).AlignRight().Text(text =>
                        {
                            text.Span("Page ").SemiBold();
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                        });

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.DefaultTextStyle(x => x.FontSize(8)).PaddingVertical(4);
                        }
                    });
                });
            });
        }

        // Renders each employee's activities
        private void ComposeEmployeeSection(IContainer container, ActivityPDFModel model)
        {
            container.PaddingBottom(15).Column(column =>
            {
                column.Item().Background(Colors.Grey.Lighten4).Row(row =>
                {
                    // Left text: take all remaining space
                    row.RelativeItem().Padding(10).AlignMiddle().Element(el =>
                    {
                        el.Column(col =>
                        {
                            col.Item().Text($"{model.CompanyName}")
                                  .FontSize(16).SemiBold().FontColor(Colors.Blue.Darken1);
                            col.Item().Text($"{model.CompanyAddress}")
                                  .FontSize(10).FontColor(Colors.Grey.Darken1);

                            col.Item().Text($"Team: {model.TeamName}")
                                  .FontSize(11).FontColor(Colors.Grey.Darken2);

                            col.Item().Text($"Employee: {model.EmployeeName}")
                                  .FontSize(11).Bold().FontColor(Colors.Black);

                            col.Item().Text("Subject: Upcoming Activities")
                                  .FontSize(11).FontColor(Colors.Grey.Darken2);
                        });
                    });

                    // Right logo: fixed size, vertically centered
                    row.ConstantItem(100)
                       .Height(60)
                       .AlignMiddle()
                       .AlignCenter()
                       .Element(el =>
                       {
                           try
                           {
                               if (model.CompanyLogo != null)
                                   el.PaddingTop(15).Image(model.CompanyLogo, ImageScaling.FitArea);
                               else
                                   el.PaddingTop(15).Placeholder();
                           }
                           catch (Exception e)
                           {
                               el.Placeholder();
                           }

                       });
                });



                column.Item().PaddingTop(10);

                // Activities
                if (model.SubEmployees?.Any() == true)
                {
                    foreach (var sub in model.SubEmployees)
                    {
                        column.Item().Text($"Activities for: {sub.EmployeeName}").Bold();
                        column.Item().Element(_ => ComposeActivitiesTable(_, sub));
                        column.Item().PaddingTop(10);
                    }
                }
                else if (model.Activities?.Any() == true)
                {
                    column.Item().Element(_ => ComposeActivitiesTable(_, model));
                }
                else
                {
                    column.Item().Text("No upcoming activities found.")
                          .FontColor(Colors.Grey.Darken1)
                          .Italic()
                          .AlignCenter();
                }
            });
        }


        // Renders table of activities
        private void ComposeActivitiesTable(IContainer container, ActivityPDFModel model)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(25);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                });

                // Header Row
                table.Header(header =>
                {
                    header.Cell().Element(CellHeader).Text("#").AlignCenter();
                    header.Cell().Element(CellHeader).Text("Lead Name").AlignCenter();
                    header.Cell().Element(CellHeader).Text("Customer Name").AlignCenter();
                    header.Cell().Element(CellHeader).Text("Activity Type").AlignCenter();
                    header.Cell().Element(CellHeader).Text("Date & Time").AlignCenter();
                    header.Cell().Element(CellHeader).Text("Note").AlignCenter();
                    header.Cell().Element(CellHeader).Text("Owner Name").AlignCenter();

                    static IContainer CellHeader(IContainer container)
                    {
                        return container.DefaultTextStyle(x => x.SemiBold().FontSize(9))
                         .Background(Colors.Grey.Lighten1)
                         .Border(1)
                         .BorderColor(Colors.Grey.Lighten2)
                         .PaddingVertical(5)
                         .AlignMiddle();
                    }
                });

                // Data Rows
                int index = 1;
                foreach (var item in model.Activities)
                {
                    table.Cell().Element(CellBody).Text(index++.ToString()).AlignCenter();
                    table.Cell().Element(CellBody).Text(item.LeadName);
                    table.Cell().Element(CellBody).Text(item.CustomerName);
                    table.Cell().Element(CellBody).Text(item.ActivityType);
                    table.Cell().Element(CellBody).Text(item.ActivityDateTime?.ToString("dd/MM/yyyy hh:mm tt"));
                    table.Cell().Element(CellBody).Text(item.ActivityNote ?? "-");
                    table.Cell().Element(CellBody).Text(item.LeadOwner);

                    static IContainer CellBody(IContainer container)
                    {
                        return container.DefaultTextStyle(x => x.FontSize(8))
                         .Border(1)
                         .BorderColor(Colors.Grey.Lighten2)
                         .Padding(5)
                         .AlignMiddle();
                    }
                }
            });
        }
    }
}
