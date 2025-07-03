using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GCTL.Service.FileHandler
{
    public class PdfFileHandler : IPdfFileHandler
    {
        public void ComposeHeader(IContainer container, string companyName, string companyAddress , bool showOnce = false)
        {
            if (showOnce)
            {
                container.ShowOnce().PaddingBottom(10).Column(column =>
                {
                    column.Item().Row(row =>
                    {
                        row.ConstantItem(50).Height(50).Element(logo =>
                        {
                            logo.Padding(5).Image("wwwroot/img/No-Image-Placeholder.svg.png", ImageScaling.FitArea);
                        });

                        row.RelativeItem().AlignCenter().Column(centerCol =>
                        {
                            centerCol.Item().AlignCenter().Text(companyName)
                                .FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

                            centerCol.Item().AlignCenter().Text(companyAddress)
                                .FontSize(10).Light().FontColor(Colors.Grey.Darken2).LineHeight(1.3f);
                        });

                        row.ConstantItem(90);
                    });

                    column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });
            }
            else
            {
                container.PaddingBottom(10).Column(column =>
                {
                    column.Item().Row(row =>
                    {
                        row.ConstantItem(90).Height(90).Element(logo =>
                        {
                            logo.Padding(5).Image("wwwroot/img/No-Image-Placeholder.svg.png", ImageScaling.FitArea);
                        });

                        row.RelativeItem().AlignCenter().Column(centerCol =>
                        {
                            centerCol.Item().AlignCenter().Text(companyName)
                                .FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

                            centerCol.Item().AlignCenter().Text(companyAddress)
                                .FontSize(10).Light().FontColor(Colors.Grey.Darken2).LineHeight(1.3f);
                        });

                        row.ConstantItem(90);
                    });

                    column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });
            }
            
        }



    }
}
