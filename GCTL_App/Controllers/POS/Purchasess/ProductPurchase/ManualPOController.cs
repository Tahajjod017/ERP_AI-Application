using GCTL.Core.Repository;
using GCTL.Core.ViewModels.POS.Purchasess.Purchase.CreatePO;
using GCTL.Data.Models;
using GCTL.Service.CommonService;
using GCTL.Service.Language;
using GCTL.Service.POS.Purchasess.CreatePO;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers.POS.Purchasess.ProductPurchase
{
    public class ManualPOController : BaseController
    {
        private readonly ICreatePurchaseOrder _createPurchaseOrder;
       // private readonly IRequisitionApprovalService _reqApprovalSvc;
        private readonly IGenericRepository<Products> _productRepository;
        private readonly ICommonService _commonService;

        public ManualPOController(ITranslateService translateService, IUserProfileService userProfileService, ICreatePurchaseOrder createPurchaseOrder,  IGenericRepository<Products> productRepository, ICommonService commonService) : base(translateService, userProfileService)
        {
            _createPurchaseOrder = createPurchaseOrder;
            //_reqApprovalSvc = reqApprovalSvc;
            _productRepository = productRepository;
            _commonService = commonService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var dara = new PurchaseOrderViewModel
                {

                    POID = "TODO", // _reqApprovalSvc.GenerateNextPoNumberAsync().Result,
                    PurchaseDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(10),

                    Items = new List<PurchaseItemViewModel>()

                };

                ViewBag.BankAccDD = await _commonService.GetBankAccounts();
                ViewBag.PaymentTypeDD = new SelectList(await _commonService.GetPaymentMethods(), "Id", "Name", 1);

                return View(dara);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _productRepository.AllActive().Include(e => e.UnitType).ToListAsync();

            return Json(products.Select(p => new
            {
                id = p.ProductID,
                name = p.ProductName ?? string.Empty,
                unit = p.UnitType?.UnitTypeName ?? string.Empty
            }));
        }



        [HttpPost("ManualPO/save")]
        public async Task<IActionResult> SavePurchaseOrder( PurchaseOrderSubmissionViewModel model)
        {
            try
            {
                var (success, message, purchaseId) = await _createPurchaseOrder.SaveManualPurchaseOrderAsync(model);
                return Ok(new { success, message, purchaseId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while saving the purchase order: " + ex.Message, purchaseId = 0 });
            }
        }
    }
}
