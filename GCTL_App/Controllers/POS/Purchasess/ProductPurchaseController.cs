using GCTL.Core.Repository;
using GCTL.Core.ViewModels.POS.Purchasess.ProductPurchase;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.POS.Purchasess.ProductPurchase;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace GCTL_App.Controllers.POS.Purchasess
{
    [Authorize]
    public class ProductPurchaseController : BaseController
    {
        private readonly IProductPurchaseService _productPurchaseService;
        private readonly IGenericRepository<Products> _productRepository;
        private readonly IGenericRepository<ProductTypes> _productTypeRepository;
        private readonly IGenericRepository<Suppliers> _supplierRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<Wirehouses> _warehouseRepository;
       // private readonly IGenericRepository<Projects> _projectRepository;

        public ProductPurchaseController(
            ITranslateService translateService,
            IUserProfileService userProfileService,
            IProductPurchaseService productPurchaseService,
            IGenericRepository<Products> productRepository,
            IGenericRepository<Suppliers> supplierRepository,
            IGenericRepository<GCTL.Data.Models.Employees> employeeRepository,
            IGenericRepository<Wirehouses> warehouseRepository,
            IGenericRepository<ProductTypes> productTypeRepository) : base(translateService, userProfileService)
        {
            _productPurchaseService = productPurchaseService;
            _productRepository = productRepository;
            _supplierRepository = supplierRepository;
            _employeeRepository = employeeRepository;
            _warehouseRepository = warehouseRepository;
            _productTypeRepository = productTypeRepository;
           // _projectRepository = projectRepository;
        }

        public IActionResult Index()
        {
           

            // Populate ViewBag for dropdowns
            ViewBag.ProductTypes = new SelectList(_productTypeRepository.AllActive().Select(e=> new {Id = e.ProductTypeID , Name = e.ProductTypeName}), "Id", "Name");
            ViewBag.ProductDD = new SelectList(_productRepository.AllActive().Select(e => new { Id = e.ProductID, Name = e.ProductName }), "Id", "Name");
            ViewBag.SupplierDD = new SelectList(_supplierRepository.AllActive().Select(e => new { Id = e.SupplierID, Name = e.FullName }), "Id", "Name");
            ViewBag.PurchaserDD = new SelectList(_employeeRepository.AllActive().Select(e => new { Id = e.EmployeeID, Name = e.FirstName + " " + e.LastName }), "Id", "Name");
            ViewBag.ReceiverDD = new SelectList(_employeeRepository.AllActive().Select(e => new { Id = e.EmployeeID, Name = e.FirstName + " " + e.LastName }), "Id", "Name");
            ViewBag.WareHouseDD = new SelectList(_warehouseRepository.AllActive().Select(e => new { Id = e.WirehouseID, Name = e.WirehouseName }), "Id", "Name");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetProductPurchases(int page = 1, int pageSize = 10, string searchTerm = "", string sortBy = "reqId", string sortOrder = "asc", string productType = "")
        {
            try
            {
                // Get filtered, sorted, and paginated purchases
                var (purchases, totalCount) = await _productPurchaseService.GetAllProductPurchasesAsync(page, pageSize, searchTerm, sortBy, sortOrder, productType);

                // Transform data for view
                var result = purchases.Select(p => new
                {
                    purchaseId = p.PurchasOrderID,
                    poid = p.POID,
                    suppiler = p.Supplier,
                    poDate = p.PODate?.ToString("dd/MM/yyyy") ?? "",
                    note = p.Note,
                    product = p.TotalProduct,
                    quentity = p.TotalQuentity ?? 0,
                    price = p.TotalPrice,
                    status = p.Received
                }).ToList();

                return Json(new
                {
                    success = true,
                    data = result,
                    totalCount,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

       

        [HttpPost]
        public async Task<IActionResult> CreatePurchase(PurchaseEntryViewModel purchase)
        {
            try
            {
                
                var result = await _productPurchaseService.CreateProductPurchaseAsync(purchase);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePurchase(PurchaseEntryViewModel purchase)
        {
            try
            {
               
                var result = await _productPurchaseService.UpdateProductPurchaseAsync(purchase);
                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeletePurchase(int id)
        {
            try
            {
                var result = await _productPurchaseService.DeleteProductPurchaseAsync(id);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApprovePurchase(PurchaseEntryViewModel purchase)
        {
            try
            {
                
                var result = await _productPurchaseService.UpdateProductPurchaseAsync(purchase);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeclinePurchase(int id)
        {
            try
            {
                var result = await _productPurchaseService.DeleteProductPurchaseAsync(id);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
