using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Inventory;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.POS.Inventory;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.POS.Inventory
{
    public class InventoryController : BaseController
    {
        private readonly IInventoryService _inventoryService;
        private readonly IGenericRepository<Locations> _locationRepository;
        private readonly IGenericRepository<Products> _productRepository;
        private readonly IGenericRepository<ProductTypes> _productTypeRepository;

        public InventoryController(
            ITranslateService translateService,
            IUserProfileService userProfileService,
            IInventoryService inventoryService,
            IGenericRepository<Locations> locationRepository,
            IGenericRepository<Products> productRepository,
            IGenericRepository<ProductTypes> productTypeRepository)
            : base(translateService, userProfileService)
        {
            _inventoryService = inventoryService;
            _locationRepository = locationRepository;
            _productRepository = productRepository;
            _productTypeRepository = productTypeRepository;
        }

        #region Dashboard
        public async Task<IActionResult> Dashboard()
        {
            int? orgID = await GetCurrentOrganizationIdAsync();
            var dashboardData = await _inventoryService.GetDashboardDataAsync(orgID);
            return View(dashboardData);
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardChartData()
        {
            int? orgID = await GetCurrentOrganizationIdAsync();
            var chartData = await _inventoryService.GetStockByLocationChartAsync(orgID);
            return Json(chartData);
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentTransactions(int count = 10)
        {
            int? orgID = await GetCurrentOrganizationIdAsync();
            var transactions = await _inventoryService.GetRecentTransactionsAsync(orgID, count);
            return Json(transactions);
        }
        #endregion

        #region Stock View
        public IActionResult Index()
        {
            ViewBag.Locations = new SelectList(_locationRepository.AllActive()
                .Select(l => new { Id = l.LocationID, Name = l.LocationName }).ToList(),
                "Id", "Name");

            ViewBag.ProductTypes = new SelectList(_productTypeRepository.AllActive()
                .Select(p => new { Id = p.ProductTypeID, Name = p.ProductTypeName }).ToList(),
                "Id", "Name");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetInventoryStock(int page = 1, int pageSize = 10, string search = "", string sortColumn = "ProductName",
        string sortDirection = "asc", int? locationId = null, int? productTypeId = null, bool? lowStockOnly = null)
        {
            int? orgID = await GetCurrentOrganizationIdAsync();

            var result = await _inventoryService.GetInventoryStockAsync(
                orgID, page, pageSize, search, sortColumn, sortDirection,
                locationId, productTypeId, lowStockOnly);

            return Json(new
            {
                data = result.Data,
                totalRecords = result.TotalRecords,
                page = page,
                pageSize = pageSize
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetProductStock(int productId)
        {
            int? orgID = await GetCurrentOrganizationIdAsync();
            var stock = await _inventoryService.GetProductStockByLocationAsync(productId, orgID);
            return Json(new { success = true, data = stock });
        }
        #endregion

        #region Transaction History
        public IActionResult Transactions()
        {
            ViewBag.Locations = new SelectList(_locationRepository.AllActive()
                .Select(l => new { Id = l.LocationID, Name = l.LocationName }).ToList(),
                "Id", "Name");

            ViewBag.Products = new SelectList(_productRepository.AllActive()
                .Select(p => new { Id = p.ProductID, Name = p.ProductName }).ToList(),
                "Id", "Name");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetTransactionHistory(
            int page = 1,
            int pageSize = 10,
            string search = "",
            string sortColumn = "TransactionDate",
            string sortDirection = "desc",
            int? locationId = null,
            int? productId = null,
            string transactionType = null,
            string? fromDate = null,
            string? toDate = null)
        {
            int? orgID = await GetCurrentOrganizationIdAsync();

            var result = await _inventoryService.GetTransactionHistoryAsync(
                orgID, page, pageSize, search, sortColumn, sortDirection,
                locationId, productId, transactionType, fromDate, toDate);

            return Json(new
            {
                data = result.Data,
                totalRecords = result.TotalRecords,
                page = page,
                pageSize = pageSize
            });
        }
        #endregion

        #region Stock Adjustment
        public IActionResult Adjustment()
        {
            ViewBag.Locations = new SelectList(_locationRepository.AllActive()
                .Select(l => new { Id = l.LocationID, Name = l.LocationName }).ToList(),
                "Id", "Name");

            ViewBag.Products = new SelectList(_productRepository.AllActive()
                .Select(p => new { Id = p.ProductID, Name = p.ProductName }).ToList(),
                "Id", "Name");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdjustment(StockAdjustmentViewModel model, BaseViewModel? baseView)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    );
                return Json(new { success = false, errors = errors });
            }

            int? empID = await GetCurrentEmployeeIdAsync();
            var result = await _inventoryService.CreateStockAdjustmentAsync(model, empID, baseView);

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAdjustmentHistory(
            int page = 1,
            int pageSize = 10,
            string search = "",
            string? fromDate = null,
            string? toDate = null)
        {
            int? orgID = await GetCurrentOrganizationIdAsync();

            var result = await _inventoryService.GetAdjustmentHistoryAsync(
                orgID, page, pageSize, search, fromDate, toDate);

            return Json(new
            {
                data = result.Data,
                totalRecords = result.TotalRecords,
                page = page,
                pageSize = pageSize
            });
        }
        #endregion

        #region Reports
        public IActionResult Reports()
        {
            ViewBag.Locations = new SelectList(_locationRepository.AllActive()
                .Select(l => new { Id = l.LocationID, Name = l.LocationName }).ToList(),
                "Id", "Name");

            ViewBag.ProductTypes = new SelectList(_productTypeRepository.AllActive()
                .Select(p => new { Id = p.ProductTypeID, Name = p.ProductTypeName }).ToList(),
                "Id", "Name");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GenerateStockReport(
            int? locationId = null,
            int? productTypeId = null,
            bool? lowStockOnly = null)
        {
            int? orgID = await GetCurrentOrganizationIdAsync();

            var pdfBytes = await _inventoryService.GenerateStockReportPDF(
                orgID, locationId, productTypeId, lowStockOnly);

            if (pdfBytes == null || pdfBytes.Length == 0)
                return NotFound("PDF generation failed");

            return File(pdfBytes, "application/pdf", "stock_report.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> DownloadStockExcel(
            int? locationId = null,
            int? productTypeId = null,
            bool? lowStockOnly = null)
        {
            int? orgID = await GetCurrentOrganizationIdAsync();

            var excelBytes = await _inventoryService.GenerateStockReportExcel(
                orgID, locationId, productTypeId, lowStockOnly);

            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "stock_report.xlsx");
        }

        [HttpPost]
        public async Task<IActionResult> GenerateMovementReport(
            string? fromDate = null,
            string? toDate = null,
            int? locationId = null,
            int? productId = null)
        {
            int? orgID = await GetCurrentOrganizationIdAsync();

            var pdfBytes = await _inventoryService.GenerateMovementReportPDF(
                orgID, fromDate, toDate, locationId, productId);

            if (pdfBytes == null || pdfBytes.Length == 0)
                return NotFound("PDF generation failed");

            return File(pdfBytes, "application/pdf", "movement_report.pdf");
        }
        #endregion
    }

}
