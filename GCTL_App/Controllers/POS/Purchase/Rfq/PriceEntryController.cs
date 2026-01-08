using GCTL.Core.ViewModels.POS.Purchase.Rfq;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.POS.Purchase.Rfq
{
    public class PriceEntryController : BaseController
    {
        private readonly ILogger<PriceEntryController> _logger;

      

        public PriceEntryController(ILogger<PriceEntryController> logger , ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {
            _logger = logger;
        }

        // GET: /PriceEntry
        public IActionResult Index()
        {
            var viewModel = GetSampleData();
            return View(viewModel);
        }

        // POST: /PriceEntry/SavePrices
        [HttpPost]
        public IActionResult SavePrices([FromBody] SavePricesRequest request)
        {
            try
            {
                if (request == null || request.VendorPrices == null)
                {
                    return BadRequest(new { success = false, message = "Invalid request data" });
                }

                // Here you would typically save to database
                foreach (var vendorUpdate in request.VendorPrices)
                {
                    _logger.LogInformation($"Saving prices for vendor {vendorUpdate.VendorCode}: {string.Join(", ", vendorUpdate.Prices)}");

                    // Save logic would go here
                    // Example: _vendorService.UpdateVendorPrices(vendorUpdate);
                }

                return Ok(new
                {
                    success = true,
                    message = "Prices saved successfully",
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving prices");
                return StatusCode(500, new { success = false, message = "Error saving prices: " + ex.Message });
            }
        }

        private PriceEntryViewModel GetSampleData()
        {
            return new PriceEntryViewModel
            {
                Items = new List<ItemViewModel>
                {
                    new ItemViewModel { Code = "E-COM07", Name = "Large Cabinet", Quantity = 10 },
                    new ItemViewModel { Code = "E-COM08", Name = "Storage Box", Quantity = 5 },
                    new ItemViewModel { Code = "E-COM09", Name = "Large Desk", Quantity = 2 },
                    new ItemViewModel { Code = "E-COM12", Name = "Conference Chair (Steel)", Quantity = 10 }
                },
                Vendors = new List<VendorViewModel>
                {
                    new VendorViewModel
                    {
                        Name = "Gemini Furniture",
                        Code = "P00022",
                        Prices = new List<decimal> { 800.00m, 13.00m, 1399.00m, 28.00m },
                        VatIncluded = true,
                        SubTotal = 11143.00m,
                        VatAmount = 1114.30m,
                        Total = 12257.30m
                    },
                    new VendorViewModel
                    {
                        Name = "Deco Added",
                        Code = "P00021",
                        Prices = new List<decimal> { 850.00m, 15.00m, 1350.00m, 26.00m },
                        VatIncluded = true,
                        SubTotal = 11535.00m,
                        VatAmount = 1153.50m,
                        Total = 12688.50m
                    },
                    new VendorViewModel
                    {
                        Name = "Azure Interior",
                        Code = "P00020",
                        Prices = new List<decimal> { 820.00m, 14.00m, 1400.00m, 30.00m },
                        VatIncluded = true,
                        SubTotal = 11370.00m,
                        VatAmount = 1137.00m,
                        Total = 12507.00m
                    }
                }
            };
        }
    }

}
