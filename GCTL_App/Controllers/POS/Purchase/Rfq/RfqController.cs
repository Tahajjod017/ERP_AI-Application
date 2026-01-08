using GCTL.Core.ViewModels.POS.Purchase.Rfq;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.POS.Purchase.Rfq
{
    public class RfqController : BaseController
    {
        public RfqController(ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {
        }

        public IActionResult Index()
        {
            var model = new RfqViewModel();
            // Add sample items for demonstration
            model.Items = new List<RfqItemViewModel>
            {
                new RfqItemViewModel { Id = 1, ProductId = 1, ProductName = "[FURN_5800] Cable Management Box", Description = "Cable Management Box", Quantity = 2, UnitPrice = 90.00m, TaxRate = 0.15m, Subtotal = 207.00m },
                new RfqItemViewModel { Id = 2, ProductId = 2, ProductName = "[FURN_7800] Office Desk", Description = "Office Desk", Quantity = 1, UnitPrice = 450.00m, TaxRate = 0.15m, Subtotal = 517.50m }
            };
            model.UntaxedAmount = 540.00m;
            model.TaxAmount = 81.00m;
            model.TotalAmount = 621.00m;

            var vendor  = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Azure Interior" },
                new SelectListItem { Value = "2", Text = "Deco Addict" },
                new SelectListItem { Value = "3", Text = "Ready Mat" },
                new SelectListItem { Value = "4", Text = "Office Supplies Co." }
            };

            ViewBag.Vendors = new SelectList(vendor, "Value" , "Text");
            ViewBag.Products = new SelectList( GetProducts1(), "Value", "Text");
            ViewBag.PurchaseAgreements = new SelectList( GetPurchaseAgreements() , "Value", "Text");

            return View(model);
        }

        public IActionResult GetNextRfqNumber()
        {
            // In real app, get from database
            return Json("RFQ00002");
        }

        public IActionResult GetVendors()
        {
            var vendors = GetVendors1();
            return Json(vendors);
        }

        public IActionResult GetProducts()
        {
            var products = GetProducts1();
            return Json(products);
        }

        public IActionResult CreateAlternativeRfq(int mainRfqId, string vendorId)
        {
            // Create alternative RFQ based on main RFQ
            var alternative = new AlternativeRfqViewModel
            {
                Id = 1,
                Reference = $"ALT{Random.Shared.Next(1000, 9999)}",
                VendorId = vendorId,
                VendorName = GetVendors1().FirstOrDefault(v => v.Value == vendorId)?.Text ?? "Unknown Vendor",
                Date = DateTime.Now,
                Items = new List<RfqItemViewModel>
                {
                    new RfqItemViewModel { ProductId = 1, ProductName = "[FURN_5800] Cable Management Box", Quantity = 2, UnitPrice = 85.00m, TaxRate = 0.15m, Subtotal = 195.50m },
                    new RfqItemViewModel { ProductId = 2, ProductName = "[FURN_7800] Office Desk", Quantity = 1, UnitPrice = 420.00m, TaxRate = 0.15m, Subtotal = 483.00m }
                },
                TotalAmount = 678.50m
            };

            return Json(new { success = true, data = alternative });
        }

        public IActionResult GetAlternativeRfqDetails(int id)
        {
            // Get alternative RFQ details
            var alternative = new AlternativeRfqViewModel
            {
                Id = id,
                Reference = $"ALT{id:0000}",
                VendorName = id == 1 ? "Azure Interior" : "Deco Addict",
                Date = DateTime.Now.AddDays(-id),
                Items = new List<RfqItemViewModel>
                {
                    new RfqItemViewModel { ProductName = "[FURN_5800] Cable Management Box", Quantity = 2, UnitPrice = id == 1 ? 85.00m : 90.00m, Subtotal = id == 1 ? 195.50m : 207.00m },
                    new RfqItemViewModel { ProductName = "[FURN_7800] Office Desk", Quantity = 1, UnitPrice = id == 1 ? 420.00m : 450.00m, Subtotal = id == 1 ? 483.00m : 517.50m }
                },
                TotalAmount = id == 1 ? 678.50m : 724.50m
            };

            return Json(new { success = true, data = alternative });
        }

        [HttpPost]
        public IActionResult Save(RfqViewModel model)
        {
            // Save RFQ logic
            if (model.Id == 0)
            {
                model.Id = Random.Shared.Next(1000, 9999);
            }

            return Json(new { success = true, message = "RFQ saved successfully!", id = model.Id });
        }

        [HttpPost]
        public IActionResult SendByEmail(int rfqId)
        {
            return Json(new { success = true, message = "RFQ sent by email!" });
        }

        [HttpPost]
        public IActionResult ConfirmOrder(int rfqId)
        {
            return Json(new { success = true, message = "Order confirmed! Status changed to Purchase Order." });
        }

        [HttpPost]
        public IActionResult CancelRfq(int rfqId)
        {
            return Json(new { success = true, message = "RFQ cancelled." });
        }

        // Helper methods for static data
        private List<SelectListItem> GetVendors1()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Azure Interior" },
                new SelectListItem { Value = "2", Text = "Deco Addict" },
                new SelectListItem { Value = "3", Text = "Ready Mat" },
                new SelectListItem { Value = "4", Text = "Office Supplies Co." }
            };
        }

        private List<SelectListItem> GetProducts1()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "[FURN_5800] Cable Management Box - $90.00" },
                new SelectListItem { Value = "2", Text = "[FURN_7800] Office Desk - $450.00" },
                new SelectListItem { Value = "3", Text = "[FURN_6741] Desk Chair - $150.00" },
                new SelectListItem { Value = "4", Text = "[CONS_89957] Bookshelf - $220.00" },
                new SelectListItem { Value = "5", Text = "Car Leasing (SUB) - Service" }
            };
        }

        private List<SelectListItem> GetPurchaseAgreements()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Select..." },
                new SelectListItem { Value = "PA001", Text = "PA001 - Annual Contract" },
                new SelectListItem { Value = "PA002", Text = "PA002 - Quarterly Agreement" }
            };
        }
    }

}
