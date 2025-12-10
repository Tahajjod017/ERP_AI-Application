using GCTL.Core.ViewModels.Services;
using GCTL.Service.FileHandler;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GCTL.Service.ProductService
{
    public class PDFService : IDocument
    {
        private readonly PDFServiceModel _model;
        private readonly IPdfFileHandler _pdfFileHandlerService;
        private readonly int? _orgId;

        public PDFService(PDFServiceModel model, IPdfFileHandler pdfFileHandlerService, int? orgId)
        {
            _model = model;
            _pdfFileHandlerService = pdfFileHandlerService;
            _orgId = orgId;
        }

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(_model.View?.ToLower() == "landscape" ? PageSizes.A4.Landscape() : PageSizes.A4.Portrait());
                page.Margin(15);

                if (_pdfFileHandlerService != null && _orgId.HasValue)
                {
                    page.Header().Element(h =>
                        _pdfFileHandlerService.ComposeHeader( h, _orgId.Value, true));
                }

                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.Column(column =>
            {
                // Top Box Table
                if (_model.TopBox?.KeyAndValues?.Any() ?? false)
                {
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns => columns.RelativeColumn());

                        table.Cell().Element(c =>
                        {
                            var kvPairs = _model.TopBox.KeyAndValues;
                            float approxWidth = kvPairs.Any() ? kvPairs.Max(kv => (kv.Key ?? "").Length) * 6 : 10;

                            c.Column(col =>
                            {
                                // Title
                                if (!string.IsNullOrEmpty(_model.TopBox.Title))
                                {
                                    col.Item().Text(_model.TopBox.Title).SemiBold().FontSize(11);
                                    col.Item().PaddingVertical(2).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                                }

                                // Key-Value Pairs
                                foreach (var kv in kvPairs)
                                {
                                    col.Item().Table(inner =>
                                    {
                                        // Always define columns
                                        inner.ColumnsDefinition(innerCols =>
                                        {
                                            if (kv.Show?.ToLower() == "both")
                                            {
                                                innerCols.ConstantColumn(approxWidth);
                                                innerCols.ConstantColumn(5);
                                                innerCols.RelativeColumn();
                                            }
                                            else
                                            {
                                                innerCols.RelativeColumn();
                                            }
                                        });

                                        if (kv.Show?.ToLower() == "both")
                                        {
                                            inner.Cell().Text(kv.Key ?? "").FontSize(10).WrapAnywhere();
                                            inner.Cell().Text(":").FontSize(9).AlignCenter();
                                            inner.Cell().PaddingLeft(3).Text(kv.Value ?? "").FontSize(10).WrapAnywhere();
                                        }
                                        else
                                        {
                                            inner.Cell().Text(kv.Value ?? "").FontSize(9).WrapAnywhere();
                                        }
                                    });
                                }
                            });
                        });
                    });
                }

                // Left & Right Boxes
                if ((_model.LeftBox?.KeyAndValues?.Any() ?? false) || (_model.RightBox?.KeyAndValues?.Any() ?? false))
                {
                    column.Spacing(8);
                    column.Item().Table(table =>
                    {
                        // Ensure at least 3 columns
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.ConstantColumn(25);
                            columns.RelativeColumn();
                        });

                        // Left Box
                        if (_model.LeftBox?.KeyAndValues?.Any(x => !string.IsNullOrWhiteSpace(x.Value)) ?? false)
                        {
                            table.Cell().Element(c =>
                            {
                                var kvPairs = _model.LeftBox.KeyAndValues;
                                float approxWidth = kvPairs.Any() ? kvPairs.Max(kv => (kv.Key ?? "").Length) * 6 : 10;

                                c.Column(col =>
                                {
                                    if (!string.IsNullOrEmpty(_model.LeftBox.Title))
                                    {
                                        col.Item().Text(_model.LeftBox.Title).SemiBold().FontSize(11);
                                        col.Item().PaddingVertical(2).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                                    }

                                    foreach (var kv in kvPairs)
                                    {
                                        col.Item().Table(inner =>
                                        {
                                            inner.ColumnsDefinition(innerCols =>
                                            {
                                                if (kv.Show?.ToLower() == "both")
                                                {
                                                    innerCols.ConstantColumn(approxWidth);
                                                    innerCols.ConstantColumn(5);
                                                    innerCols.RelativeColumn();
                                                }
                                                else
                                                {
                                                    innerCols.RelativeColumn();
                                                }
                                            });

                                            if (kv.Show?.ToLower() == "both")
                                            {
                                                inner.Cell().Text(kv.Key ?? "").FontSize(9).WrapAnywhere();
                                                inner.Cell().Text(":").FontSize(9).AlignCenter();
                                                inner.Cell().PaddingLeft(3).Text(kv.Value ?? "").FontSize(9).WrapAnywhere();
                                            }
                                            else
                                            {
                                                inner.Cell().Text(kv.Value ?? "").FontSize(9).WrapAnywhere();
                                            }
                                        });
                                    }
                                });
                            });
                        }
                        else
                        {
                            table.Cell(); // empty left cell
                        }

                        table.Cell(); // middle spacer

                        // Right Box
                        if (_model.RightBox?.KeyAndValues?.Any(x => !string.IsNullOrWhiteSpace(x.Value)) ?? false)
                        {
                            table.Cell().AlignRight().Element(c =>
                            {
                                var kvPairs = _model.RightBox.KeyAndValues;
                                float approxWidth = kvPairs.Any() ? kvPairs.Max(kv => (kv.Key ?? "").Length) * 6 : 10;

                                c.Column(col =>
                                {
                                    if (!string.IsNullOrEmpty(_model.RightBox.Title))
                                    {
                                        col.Item().Text(_model.RightBox.Title).SemiBold().FontSize(11);
                                        col.Item().PaddingVertical(2).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                                    }

                                    foreach (var kv in kvPairs)
                                    {
                                        col.Item().Table(inner =>
                                        {
                                            inner.ColumnsDefinition(innerCols =>
                                            {
                                                if (kv.Show?.ToLower() == "both")
                                                {
                                                    innerCols.ConstantColumn(approxWidth);
                                                    innerCols.ConstantColumn(5);
                                                    innerCols.RelativeColumn();
                                                }
                                                else
                                                {
                                                    innerCols.RelativeColumn();
                                                }
                                            });

                                            if (kv.Show?.ToLower() == "both")
                                            {
                                                inner.Cell().Text(kv.Key ?? "").FontSize(9).WrapAnywhere();
                                                inner.Cell().Text(":").FontSize(9).AlignCenter();
                                                inner.Cell().PaddingLeft(3).Text(kv.Value ?? "").FontSize(9).WrapAnywhere();
                                            }
                                            else
                                            {
                                                inner.Cell().Text(kv.Value ?? "").FontSize(9).WrapAnywhere();
                                            }
                                        });
                                    }
                                });
                            });
                        }
                        else
                        {
                            table.Cell(); // empty right cell
                        }
                    });
                }
                if (!string.IsNullOrEmpty(_model.SortBy))
                {
                     var groupedData = _model.Rows
                    .GroupBy(row =>
                    {
                        
                        var index = _model.Headers.FindIndex(h => h.Text.Equals(_model.SortBy, StringComparison.OrdinalIgnoreCase));
                        return index >= 0 && index < row.Count ? row[index].Text : "Others";
                    })
                    .ToList();
                }
               


                // Main Table (Headers & Rows)
                if (_model.Headers?.Any() ?? false)
                {
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            foreach (var header in _model.Headers)
                            {
                                if (header.Width > 0)
                                    columns.ConstantColumn(header.Width);
                                else
                                    columns.RelativeColumn();
                            }
                        });

                        // Header Row
                        table.Header(header =>
                        {
                            foreach (var col in _model.Headers)
                            {
                                header.Cell().Element(HeaderStyle)
                                      .Text(col.Text)
                                      .AlignCenter();
                            }
                        });

                        // Data Rows
                        foreach (var dataRow in _model.Rows)
                        {
                            foreach (var cell in dataRow)
                            {
                                table.Cell().Element(c =>
                                {
                                    var align = CellStyle(c);
                                    switch (cell.Align?.ToLower())
                                    {
                                        case "center": align = align.AlignCenter(); break;
                                        case "right": align = align.AlignRight(); break;
                                        default: align = align.AlignLeft(); break;
                                    }

                                    align.Text(cell.Text);
                                });
                            }
                        }
                    });
                }

                // Footer Box
                if (_model.FooterBox?.KeyAndValues?.Any() ?? false)
                {
                    column.Item().AlignRight().PaddingTop(4).Row(row =>
                    {
                        row.AutoItem()
                           .Padding(10)
                           .Column(col =>
                           {
                               foreach (var kv in _model.FooterBox.KeyAndValues)
                               {
                                   col.Item().AlignLeft().Row(r =>
                                   {
                                       r.AutoItem().Text($"{kv.Key ?? ""} : {kv.Value ?? ""}").SemiBold();
                                   });
                               }
                           });
                    });
                }
            });
        }

        private static IContainer HeaderStyle(IContainer container) =>
            container.DefaultTextStyle(x => x.SemiBold().FontSize(9))
                     .Background(Colors.Grey.Lighten1)
                     .Border(1)
                     .BorderColor(Colors.Grey.Lighten2)
                     .PaddingVertical(5)
                     .AlignMiddle();

        private static IContainer CellStyle(IContainer container) =>
            container.DefaultTextStyle(x => x.FontSize(8))
                     .Border(1)
                     .BorderColor(Colors.Grey.Lighten2)
                     .Padding(5)
                     .AlignMiddle();

        private void ComposeFooter(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().DefaultTextStyle(x => x.FontSize(8))
                    .AlignLeft().Text($"Created: {DateTime.Now:g}");

                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.Span("Page ").FontSize(8);
                    text.CurrentPageNumber().FontSize(8);
                    text.Span(" of ").FontSize(8);
                    text.TotalPages().FontSize(8);
                });
            });
        }
    }
}
