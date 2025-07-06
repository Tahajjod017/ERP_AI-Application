using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Data.Models;
using GCTL.Service.ImageFileHandler;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;


namespace GCTL.Service.FileHandler
{
    public class PdfFileHandler : IPdfFileHandler
    {

        private readonly IGenericRepository<EmployeeOfficeInfo> _employeeOfficialRepository;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IImageFileHandlerService _imageHelper;

        public PdfFileHandler(IGenericRepository<EmployeeOfficeInfo> employeeOfficialRepository, IGenericRepository<Organization> organizationRepository, IImageFileHandlerService imageHelper)
        {
            _employeeOfficialRepository = employeeOfficialRepository;
            _organizationRepository = organizationRepository;
            _imageHelper = imageHelper;
        }



        public void ComposeHeader(IContainer container, int companyId, bool showOnce = false)
        {
            // Define default values for fields not directly available
            string companyName = "";
            string companyAddress = "";
            string companyImage = ""; // Default image path
            string imagePath = "";    // Full image path for checking

            var company = _organizationRepository.AllActive()
                .Where(e => e.OrganizationID == companyId)
                .FirstOrDefault();

            if (company != null)
            {
                companyName = company.OrganizationName;
                companyAddress = company.Address;
                companyImage = company.LogoLink;
            }

            imagePath = "wwwroot/uploads/company/logo/" + companyImage;
            bool imageExists = !string.IsNullOrEmpty(companyImage) && File.Exists(imagePath);

            if (showOnce)
            {
                container.ShowOnce().PaddingBottom(10).Column(column =>
                {
                    column.Item().Row(row =>
                    {
                        if (imageExists)
                        {
                            row.ConstantItem(50).Height(50).Element(logo =>
                            {
                                logo.Padding(5).Image(imagePath, ImageScaling.FitArea);
                            });
                        }

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
                        if (imageExists)
                        {
                            row.ConstantItem(90).Height(90).Element(logo =>
                            {
                                logo.Padding(5).Image(imagePath, ImageScaling.FitArea);
                            });
                        }

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


        public void ComposeWatermark(IContainer container, int companyId, float opacity = 0.1f)
        {
            var company = _organizationRepository.AllActive()
                .FirstOrDefault(e => e.OrganizationID == companyId);
            if (company == null || string.IsNullOrEmpty(company.OrganizationName))
                return;
            var companyName = company.OrganizationName.ToUpper();

            container.Layers(layers =>
            {
                layers.PrimaryLayer()
                 .AlignCenter()
                    .AlignMiddle()
                    .Rotate(-45)
                    .Text(text =>
                    {
                        text.DefaultTextStyle(style => style
                            .FontFamily("Arial")
                            .FontSize(48)
                            .FontColor(Color.FromHex("#000406").WithAlpha(1)));

                        text.Span(companyName);
                    })
                   ; // Rotate -45 degrees for diagonal effect
            });
        }
    }
}
