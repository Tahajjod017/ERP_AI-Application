using GCTL.Core.Repository;
using GCTL.Core.ViewModels.POS.Purchasess.Purchase.CreatePO;
using GCTL.Core.ViewModels.POS.Purchasess.Purchase.PurchaseReceived;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.CommonService;
using GCTL.Service.Language;
using GCTL.Service.POS.Purchasess.CreatePO;
using GCTL.Service.POS.Purchasess.PurchaseReceived;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace GCTL_App.Controllers.POS.Purchasess.ProductPurchase
{
    [Authorize]
    public class PurchaseReceivedController : BaseController
    {
        private readonly ICreatePurchaseOrder _createPurchaseOrder;
        private readonly IGenericRepository<PurchasOrders> _purchaseOrderRepository;
        //private readonly IGenericRepository<ProductMovements> _productMovementRepository;
        private readonly IUserInfoService _userInfoService;
        private readonly IPurchaseReceivedService _purchaseReceivedService;
        private readonly ICommonService _commonService;


        public PurchaseReceivedController(ITranslateService translateService, IUserProfileService userProfileService, ICreatePurchaseOrder createPurchaseOrder, IGenericRepository<PurchasOrders> purchaseOrderRepository, IUserInfoService userInfoService, IPurchaseReceivedService purchaseReceivedService,  ICommonService commonService) : base(translateService, userProfileService)
        {
            _createPurchaseOrder = createPurchaseOrder;
            _purchaseOrderRepository = purchaseOrderRepository;
            _userInfoService = userInfoService;
            _purchaseReceivedService = purchaseReceivedService;
            //_productMovementRepository = productMovementRepository;
            _commonService = commonService;
        }

        public async Task<IActionResult> Index(int id)
        {
            var data = _purchaseOrderRepository.AllActive()
                .Include(e => e.PurchasOrderItems)

                .Where(e=>e.PurchasOrderID == id)

             .Select(e => new PurchaseReceivedViewModel
             {
                 POID = e.POID,
                 VendorBillNO = "",
                 PRID = "",
                 TaxRate = e.TaxPercent,
                 PurchaseOrderID = e.PurchasOrderID,
                 PurchaseDate = e.PurchaseDate,
                 DueDate = e.DueDate,
                 BillDate = DateTime.Now,
                 PRDate = DateTime.Now,
                // Tolocation = e.ToLocation,
                 VendorId = e.SupplierID,
                 Note = e.Note,
                 TermNcondition = e.TermsAndConditions,


                 Items = e.PurchasOrderItems.Select(i => new PurchaseRecItemViewModel
                 {
                     PurchasOrderItemID = i.PurchasOrderItemID,
                     ProductID = i.ProductID,
                     //ProductID = 1001,
                     ItemName = i.Product.ProductName,
                     Unit = i.Product.UnitType.UnitTypeName,
                     Quantity = i.Quantity,
                     UnitDistribute = i.Quantity,
                     UnitPrice = i.UnitPrice,
                     TotalPrice = i.UnitPrice * i.Quantity,

                 }).ToList()


             }).FirstOrDefault();


            return View(data);
        }


        #region Vendor and Shipping Address Management

        [HttpPost("PurchaseReceived/vendors")]
        public async Task<IActionResult> AddVendor([FromBody] VendorViewModel vendor)
        {
            var result = await _createPurchaseOrder.AddVendorAsync(vendor);
            return result ? Ok(new { success = true, message = "Vendor added successfully" }) : BadRequest(new { success = false, message = "Failed to add vendor" });
        }

        [HttpGet("PurchaseReceived/vendors")]
        public async Task<IActionResult> GetVendors()
        {
            var vendors = await _createPurchaseOrder.GetVendorsAsync();
            return Ok(vendors);
        }

        [HttpGet("PurchaseReceived/shipping-addresses")]
        public async Task<IActionResult> GetShippingAddresses()
        {
            var addresses = await _createPurchaseOrder.GetShippingAddressesAsync();
            return Ok(addresses);
        }

        [HttpPost("PurchaseReceived/shipping-addresses")]
        public async Task<IActionResult> AddShippingAddress([FromBody] ShippingAddressViewModel address)
        {
            var result = await _createPurchaseOrder.AddShippingAddressAsync(address);
            return result ? Ok() : BadRequest("Failed to add shipping address");
        }


        [HttpGet]
        [Route("PurchaseReceived/GetReqValue")]
        public async Task<IActionResult> GetReqValue(int id)
        {
            //var data = await _productMovementRepository.AllActive().Where(e => e.ToLocationID == id && e.IsFinal == true && e.Quantity > (e.DistributedQuantity ?? 0))
            //     .Select(e => new {
            //         reqItemId = e.RequisitionItemID,
            //         productId = e.ProductID,
            //         reqQty = e.Quantity ?? 0,
            //         disQty= (e.DistributedQuantity ?? 0),
            //         notDistribute = e.Quantity - (e.DistributedQuantity ?? 0)
            //     }).ToListAsync();

            //return Ok(new { message = "data saved" });
            return Ok();
        }

        #endregion

        [HttpPost("PurchaseReceived/save")]
        public async Task<IActionResult> SavePurchaseReceived( PurchaseReceivedViewModel model)
        {
           
            try
            {
                if (model == null)
                {
                    return BadRequest(new { success = false, message = "Invalid data received" });
                }

                var result = await _purchaseReceivedService.SavePurchaseReceivedAsync(model);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message,
                       // purchaseReceivedId = result.PurchaseReceivedId,
                        redirectUrl = "/ProductPurchase/Index"
                    });
                }

                return BadRequest(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error in SavePurchaseReceived: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while saving. Please try again."
                });
            }
        }


    }
}
