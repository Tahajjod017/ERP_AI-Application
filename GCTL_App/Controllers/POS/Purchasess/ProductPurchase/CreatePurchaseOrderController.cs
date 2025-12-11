using GCTL.Core.Repository;
using GCTL.Core.ViewModels.POS.Purchasess.Purchase.CreatePO;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.ReportService;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using GCTL.Service.POS.Purchasess.CreatePO;

namespace GCTL_App.Controllers.POS.Purchasess.ProductPurchase
{
    public class CreatePurchaseOrderController : BaseController
    {
        private readonly ICreatePurchaseOrder _createPurchaseOrder;
        private readonly IReportService _reportRepo;
        private readonly IGenericRepository<PurchasOrders> _purchaseOrderRepository;
        public CreatePurchaseOrderController(ITranslateService translateService, IUserProfileService userProfileService, ICreatePurchaseOrder createPurchaseOrder, IGenericRepository<PurchasOrders> purchaseOrderRepository, IReportService reportRepo) : base(translateService, userProfileService)
        {
            _createPurchaseOrder = createPurchaseOrder;
            _purchaseOrderRepository = purchaseOrderRepository;
            _reportRepo = reportRepo;
        }

        [HttpGet("CreatePurchaseOrder/Index/{purchaseId}")]
        public IActionResult Index(int purchaseId)
        {
            var data = _purchaseOrderRepository.AllActive()
                .Include(p => p.PurchasOrderItems)
              //  .Include(p => p.ProductMovements)
                .Where(p => p.PurchasOrderID == purchaseId)
                .Select(e => new PurchaseOrderViewModel
                {
                    PurchaseOrderID = e.PurchasOrderID,
                    POID = e.POID,
                    PurchaseDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(10),

                    Tolocation = 1,

                    //SupplierID = e.SupplierID,
                    //OBBillingAddressID = e.OBBillingAddressID,
                    //OBShipingAddressID = e.OBShipingAddressID,
                    //OtherReference = e.OtherReference,
                    //Note = e.Note,
                    //AttachmentLink = e.AttachmentLink,
                    //TermsAndConditions = e.TermsAndConditions,

                    Items = e.PurchasOrderItems.Select(item => new PurchaseItemViewModel
                    {
                        PurchasOrderItemID = item.PurchasOrderItemID,
                        ProductID = item.ProductID,
                        ItemName = item.Product.ProductName,
                        Unit = item.Product.UnitType.UnitTypeName,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    }).ToList()
                }).FirstOrDefault();



        

            return View(data);



           
        }




        [HttpPost("CreatePurchaseOrder/vendors")]
        public async Task<IActionResult> AddVendor([FromBody] VendorViewModel vendor)
        {
            var result = await _createPurchaseOrder.AddVendorAsync(vendor);
            return result ? Ok(new { success = true, message = "Vendor added successfully" }) : BadRequest(new { success = false, message = "Failed to add vendor" });
        }

        [HttpGet("CreatePurchaseOrder/vendors")]
        public async Task<IActionResult> GetVendors()
        {
            var vendors = await _createPurchaseOrder.GetVendorsAsync();
            return Ok(vendors);
        }

        [HttpGet("CreatePurchaseOrder/shipping-addresses")]
        public async Task<IActionResult> GetShippingAddresses()
        {
            var addresses = await _createPurchaseOrder.GetShippingAddressesAsync();
            return Ok(addresses);
        }

        [HttpPost("CreatePurchaseOrder/shipping-addresses")]
        public async Task<IActionResult> AddShippingAddress([FromBody] ShippingAddressViewModel address)
        {
            var result = await _createPurchaseOrder.AddShippingAddressAsync(address);
            return result ? Ok() : BadRequest("Failed to add shipping address");
        }

        [HttpPost("CreatePurchaseOrder/save")]
        public async Task<IActionResult> SavePurchaseOrder([FromBody] PurchaseOrderSubmissionViewModel model)
        {
            try
            {
                var (success, message, purchaseId) = await _createPurchaseOrder.SavePurchaseOrderAsync(model);
                return Ok(new
                {
                    success,
                    message,
                    purchaseId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while saving the purchase order: " + ex.Message,
                    purchaseId = 0
                });
            }

        }







        [HttpPost("CreatePurchaseOrder/upload-attachments")]
        public async Task<IActionResult> UploadAttachments(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded." });
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { uniqueFileName });
        }



    }
}
