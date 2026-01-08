using GCTL.Core.ViewModels.POS.Purchase.Rfq;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.POS.Purchase.Rfq
{
    public class RfqListController : BaseController
    {
        public RfqListController(ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {
        }

        public IActionResult Index()
        {
            var model = new RfqListViewModel();

            // Sample data - in real app, get from database
            model.ParentRfqs = new List<ParentRfqViewModel>
            {
                new ParentRfqViewModel
                {
                    Id = 1,
                    Reference = "RFQ00001",
                    VendorName = "Azure Interior",
                    Date = DateTime.Now.AddDays(-10),
                    TotalAmount = 621.00m,
                    Status = "RFQ_SENT",
                    StatusColor = "warning",
                    SubRfqs = new List<SubRfqViewModel>
                    {
                        new SubRfqViewModel
                        {
                            Id = 101,
                            ParentId = 1,
                            Reference = "ALT0001",
                            VendorName = "Deco Addict",
                            Date = DateTime.Now.AddDays(-8),
                            TotalAmount = 678.50m,
                            Status = "SENT",
                            StatusColor = "info",
                            Savings = -57.50m,
                            Notes = "Slightly higher price but better quality"
                        },
                        new SubRfqViewModel
                        {
                            Id = 102,
                            ParentId = 1,
                            Reference = "ALT0002",
                            VendorName = "Office Supplies Co.",
                            Date = DateTime.Now.AddDays(-7),
                            TotalAmount = 595.75m,
                            Status = "CONFIRMED",
                            StatusColor = "success",
                            IsBestOffer = true,
                            Savings = 25.25m,
                            Notes = "Best offer with 4% discount"
                        }
                    }
                },
                new ParentRfqViewModel
                {
                    Id = 2,
                    Reference = "RFQ00002",
                    VendorName = "Ready Mat",
                    Date = DateTime.Now.AddDays(-5),
                    TotalAmount = 1250.50m,
                    Status = "RFQ",
                    StatusColor = "primary",
                    SubRfqs = new List<SubRfqViewModel>
                    {
                        new SubRfqViewModel
                        {
                            Id = 201,
                            ParentId = 2,
                            Reference = "ALT0003",
                            VendorName = "Azure Interior",
                            Date = DateTime.Now.AddDays(-4),
                            TotalAmount = 1195.00m,
                            Status = "DRAFT",
                            StatusColor = "secondary",
                            Savings = 55.50m,
                            Notes = "Pending vendor confirmation"
                        }
                    }
                },
                new ParentRfqViewModel
                {
                    Id = 3,
                    Reference = "RFQ00003",
                    VendorName = "Deco Addict",
                    Date = DateTime.Now.AddDays(-2),
                    TotalAmount = 850.25m,
                    Status = "PURCHASE_ORDER",
                    StatusColor = "success",
                    SubRfqs = new List<SubRfqViewModel>(), // No alternatives yet
                    IsExpanded = true
                }
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult GetComparisonData([FromBody] List<int> subRfqIds)
        {
            // Get data for selected sub-RFQs for comparison
            var data = new List<object>();

            // In real app, fetch from database
            foreach (var id in subRfqIds)
            {
                data.Add(new
                {
                    Id = id,
                    Reference = $"ALT{id:0000}",
                    Vendor = id == 101 ? "Deco Addict" : "Office Supplies Co.",
                    Products = new[]
                    {
                new { Name = "[FURN_5800] Cable Management Box", Qty = 2, Price = (id == 101 ? 85.00 : 82.50), Total = (id == 101 ? 195.50 : 189.75) },
                new { Name = "[FURN_7800] Office Desk", Qty = 1, Price = (id == 101 ? 420.00 : 410.00), Total = (id == 101 ? 483.00 : 471.50) }
            },
                    TotalAmount = (id == 101 ? 678.50 : 661.25),
                    Status = id == 101 ? "SENT" : "CONFIRMED"
                });
            }

            return Json(new { success = true, data });
        }

    }
}
