using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Purchase.PurchaseReceive;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.POS.Purchase.PurchaseReceive;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.POS.Purchase.PurchaseReceive
{
   
        public class PurchaseReceiveController : BaseController
        {
            private readonly IPurchaseReceiveService _purchaseReceiveService;
            private readonly IGenericRepository<Statuses> _statusRepository;
            private readonly IGenericRepository<Suppliers> _supplierRepository;

            public PurchaseReceiveController(
                ITranslateService translateService,
                IUserProfileService userProfileService,
                IPurchaseReceiveService purchaseReceiveService,
                IGenericRepository<Statuses> statusRepository,
                IGenericRepository<Suppliers> supplierRepository)
                : base(translateService, userProfileService)
            {
                _purchaseReceiveService = purchaseReceiveService;
                _statusRepository = statusRepository;
                _supplierRepository = supplierRepository;
            }

            #region Index
            public IActionResult Index()
            {
                ViewBag.Statuses = new SelectList(_statusRepository.AllActive()
                    .Select(s => new { Id = s.StatusID, Name = s.StatusName }).ToList(),
                    "Id", "Name");

                ViewBag.Suppliers = new SelectList(_supplierRepository.AllActive()
                    .Select(s => new { Id = s.SupplierID, Name = s.FullName }).ToList(),
                    "Id", "Name");

                return View();
            }
            #endregion

            #region Get Purchase Receives List
            [HttpGet]
            public async Task<IActionResult> GetPurchaseReceives(int page = 1, int pageSize = 10, string search = "", string sortColumn = "PurchaseReceiveID", string sortDirection = "desc", int? statusId = null, int? supplierId = null, string? fromDate = null, string? toDate = null)
            {
                int? orgID = await GetCurrentOrganizationIdAsync();

                var result = await _purchaseReceiveService.GetPurchaseReceivesAsync(orgID, page, pageSize, search, sortColumn, sortDirection, statusId, supplierId, fromDate, toDate);

                return Json(new
                {
                    data = result.Items,
                    totalRecords = result.TotalRecords,
                    page = page,
                    pageSize = pageSize
                });
            }
            #endregion

            #region Get Open Purchase Orders
            [HttpGet]
            public async Task<IActionResult> GetOpenPurchaseOrders()
            {
                int? orgID = await GetCurrentOrganizationIdAsync();
                var result = await _purchaseReceiveService.GetOpenPurchaseOrdersAsync(orgID);
                return Json(result);
            }
            #endregion

            #region Get PO Details for Receive
            [HttpGet]
            public async Task<IActionResult> GetPODetailsForReceive(int poId)
            {
                try
                {
                    var details = await _purchaseReceiveService.GetPODetailsForReceiveAsync(poId);

                    if (details == null)
                        return NotFound(new { message = "Purchase Order not found" });

                    return Ok(new { success = true, data = details });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
                }
            }
            #endregion

            #region Get Receive Details
            [HttpGet]
            public async Task<IActionResult> GetReceiveDetails(int id)
            {
                try
                {
                    var details = await _purchaseReceiveService.GetReceiveDetailsAsync(id);

                    if (details == null)
                        return NotFound(new { message = "Purchase Receive not found" });

                    return Ok(new { success = true, data = details });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
                }
            }
            #endregion

            #region Create Purchase Receive
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Create(CreatePurchaseReceiveViewModel model, BaseViewModel? baseView)
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
                var result = await _purchaseReceiveService.CreatePurchaseReceiveAsync(model, empID, baseView);

                return Json(result);
            }
            #endregion

            #region Edit Purchase Receive
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(EditPurchaseReceiveViewModel model, BaseViewModel? baseView)
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
                var result = await _purchaseReceiveService.UpdatePurchaseReceiveAsync(model, empID, baseView);

                return Json(result);
            }
            #endregion

            #region Delete Purchase Receive
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Delete(int id, BaseViewModel? baseView)
            {
                int? empID = await GetCurrentEmployeeIdAsync();
                var result = await _purchaseReceiveService.DeletePurchaseReceiveAsync(id, empID, baseView);

                return Json(result);
            }
            #endregion

            #region Get Next PR Number
            [HttpGet]
            public async Task<IActionResult> GetNextPRNumber()
            {
                var prNumber = await _purchaseReceiveService.GetNextPRNumberAsync();
                return Ok(prNumber);
            }
            #endregion

            #region Export
            [HttpPost]
            public async Task<IActionResult> GeneratePDF(string? fromDate = null, string? toDate = null)
            {
                int? orgID = await GetCurrentOrganizationIdAsync();

                var pdfBytes = await _purchaseReceiveService.GeneratePDF(orgID ?? 0, fromDate, toDate);

                if (pdfBytes == null || pdfBytes.Length == 0)
                    return NotFound("PDF generation failed");

                return File(pdfBytes, "application/pdf", "purchase_receives.pdf");
            }

            [HttpGet]
            public async Task<IActionResult> DownloadExcel(string? fromDate = null, string? toDate = null)
            {
                int? orgID = await GetCurrentOrganizationIdAsync();

                var excelBytes = await _purchaseReceiveService.GenerateExcel(orgID ?? 0, fromDate, toDate);

                return File(excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "purchase_receives.xlsx");
            }
            #endregion
        }

    }

