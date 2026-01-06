using GCTL.Core.ViewModels.POS.Purchase.Rfq;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.POS.Purchase.Rfq
{

    public class RfqComparisonController : BaseController
    {
        public RfqComparisonController(ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {
        }

        public IActionResult Index(List<int> subRfqIds = null)
        {
            // In real app, get data based on selected sub-RFQ IDs
            // For demo, use sample data

            var model = new RfqComparisonViewModel
            {
                MainRfqId = 1,
                MainRfqReference = "RFQ00001",
                MainRfqDate = DateTime.Now.AddDays(-10),
                MainRfqTotal = 12349.50m,
                ShowNegotiation = false,
                Vendors = new List<ComparisonVendorViewModel>
                {
                    new ComparisonVendorViewModel
                    {
                        Id = 1,
                        Name = "Gemini Furniture",
                        Reference = "P00022",
                        DisplayName = "Gemini Furniture P00022",
                        IsBestOffer = true,
                        SubTotal = 11143.00m,
                        TaxAmount = 1206.50m,
                        TotalAmount = 12349.50m,
                        Savings = 0,
                        SavingsPercentage = 0,
                        QuoteDate = DateTime.Now.AddDays(-2),
                        Status = "Quoted"
                    },
                    new ComparisonVendorViewModel
                    {
                        Id = 2,
                        Name = "Deco Addict",
                        Reference = "P00021",
                        DisplayName = "Deco Addict P00021",
                        IsBestOffer = false,
                        SubTotal = 11143.00m,
                        TaxAmount = 1206.50m,
                        TotalAmount = 12349.50m,
                        Savings = 0,
                        SavingsPercentage = 0,
                        QuoteDate = DateTime.Now.AddDays(-3),
                        Status = "Quoted"
                    },
                    new ComparisonVendorViewModel
                    {
                        Id = 3,
                        Name = "Azure Interior",
                        Reference = "P00020",
                        DisplayName = "Azure Interior P00020",
                        IsBestOffer = false,
                        SubTotal = 11143.00m,
                        TaxAmount = 1206.50m,
                        TotalAmount = 12349.50m,
                        Savings = 0,
                        SavingsPercentage = 0,
                        QuoteDate = DateTime.Now.AddDays(-4),
                        Status = "Quoted"
                    }
                },
                Items = new List<ComparisonItemViewModel>
                {
                    new ComparisonItemViewModel
                    {
                        Id = 1,
                        ItemCode = "[E-COM07]",
                        ProductName = "Large Cabinet",
                        Description = "Large wooden cabinet",
                        Quantity = 10,
                        Uom = "Units",
                        VendorPrices = new Dictionary<int, decimal>
                        {
                            { 1, 800.00m }, { 2, 800.00m }, { 3, 800.00m }
                        },
                        VendorTotals = new Dictionary<int, decimal>
                        {
                            { 1, 8000.00m }, { 2, 8000.00m }, { 3, 8000.00m }
                        },
                        NegotiationPrices = new Dictionary<int, decimal>
                        {
                            { 1, 800.00m }, { 2, 800.00m }, { 3, 800.00m }
                        },
                        NegotiationTotals = new Dictionary<int, decimal>
                        {
                            { 1, 8000.00m }, { 2, 8000.00m }, { 3, 8000.00m }
                        }
                    },
                    new ComparisonItemViewModel
                    {
                        Id = 2,
                        ItemCode = "[E-COM08]",
                        ProductName = "Storage Box",
                        Description = "Plastic storage box",
                        Quantity = 5,
                        Uom = "Units",
                        VendorPrices = new Dictionary<int, decimal>
                        {
                            { 1, 13.00m }, { 2, 13.00m }, { 3, 13.00m }
                        },
                        VendorTotals = new Dictionary<int, decimal>
                        {
                            { 1, 65.00m }, { 2, 65.00m }, { 3, 65.00m }
                        },
                        NegotiationPrices = new Dictionary<int, decimal>
                        {
                            { 1, 13.00m }, { 2, 13.00m }, { 3, 13.00m }
                        },
                        NegotiationTotals = new Dictionary<int, decimal>
                        {
                            { 1, 65.00m }, { 2, 65.00m }, { 3, 65.00m }
                        }
                    },
                    new ComparisonItemViewModel
                    {
                        Id = 3,
                        ItemCode = "[E-COM09]",
                        ProductName = "Large Desk",
                        Description = "Executive desk",
                        Quantity = 2,
                        Uom = "Units",
                        VendorPrices = new Dictionary<int, decimal>
                        {
                            { 1, 1399.00m }, { 2, 1399.00m }, { 3, 1399.00m }
                        },
                        VendorTotals = new Dictionary<int, decimal>
                        {
                            { 1, 2798.00m }, { 2, 2798.00m }, { 3, 2798.00m }
                        },
                        NegotiationPrices = new Dictionary<int, decimal>
                        {
                            { 1, 1399.00m }, { 2, 1399.00m }, { 3, 1399.00m }
                        },
                        NegotiationTotals = new Dictionary<int, decimal>
                        {
                            { 1, 2798.00m }, { 2, 2798.00m }, { 3, 2798.00m }
                        }
                    },
                    new ComparisonItemViewModel
                    {
                        Id = 4,
                        ItemCode = "[E-COM12]",
                        ProductName = "Conference Chair (Steel)",
                        Description = "Steel frame conference chair",
                        Quantity = 10,
                        Uom = "Units",
                        VendorPrices = new Dictionary<int, decimal>
                        {
                            { 1, 28.00m }, { 2, 28.00m }, { 3, 28.00m }
                        },
                        VendorTotals = new Dictionary<int, decimal>
                        {
                            { 1, 280.00m }, { 2, 280.00m }, { 3, 280.00m }
                        },
                        NegotiationPrices = new Dictionary<int, decimal>
                        {
                            { 1, 28.00m }, { 2, 28.00m }, { 3, 28.00m }
                        },
                        NegotiationTotals = new Dictionary<int, decimal>
                        {
                            { 1, 280.00m }, { 2, 280.00m }, { 3, 280.00m }
                        }
                    }
                }
            };

            // Calculate savings if we had different prices
            // In real app, this would be calculated from database

            return View(model);
        }

        [HttpPost]
        public IActionResult UpdateNegotiationPrice(int itemId, int vendorId, decimal newPrice)
        {
            // Update negotiation price in database
            // For now, return success

            return Json(new
            {
                success = true,
                message = "Price updated successfully",
                newTotal = newPrice * 10 // In real app, calculate based on quantity
            });
        }

        [HttpPost]
        public IActionResult SaveNegotiation([FromBody] NegotiationSaveRequest request)
        {
            // Save all negotiation prices
            // For now, return success

            return Json(new
            {
                success = true,
                message = "Negotiation saved successfully",
                data = request
            });
        }

        [HttpPost]
        public IActionResult SelectBestOffer(int vendorId)
        {
            // Mark vendor as best offer/selected
            // For now, return success

            return Json(new
            {
                success = true,
                message = $"Vendor {vendorId} selected as best offer"
            });
        }
    }


}
