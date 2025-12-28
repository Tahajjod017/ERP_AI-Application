using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Requsition.RequisitionApprover;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.POS.Requsition.RequisitionApprover;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.POS.Requisition
{
    public class RequisitionApproverController : BaseController
    {
        #region CTOR
        private readonly IRequisitionApproverService _approverService;
        private readonly IGenericRepository<Statuses> _statusRepository;
        private readonly IGenericRepository<ProductTypes> _productTypesRepository;

        public RequisitionApproverController(
            ITranslateService translateService,
            IUserProfileService userProfileService,
            IRequisitionApproverService approverService,
            IGenericRepository<Statuses> statusRepository,
            IGenericRepository<ProductTypes> productTypesRepository)
            : base(translateService, userProfileService)
        {
            _approverService = approverService;
            _statusRepository = statusRepository;
            _productTypesRepository = productTypesRepository;
        }

        #endregion

        #region Index
        public IActionResult Index()
        {
            ViewBag.Statuses = new SelectList(_statusRepository.AllActive()
                .Select(e => new { Id = e.StatusID, Name = e.StatusName }).ToList(), "Id", "Name");

            ViewBag.ProductTypes = new SelectList(_productTypesRepository.AllActive()
                .Select(e => new { Id = e.ProductTypeID, Name = e.ProductTypeName }).ToList(), "Id", "Name");

            return View();
        }
        #endregion

        #region Get Pending Approvals
        [HttpGet]
        public async Task<IActionResult> GetPendingApprovals(int page = 1, int pageSize = 10, string search = "", string sortColumn = "RequisitionId", string sortDirection = "asc", int? productTypeId = null, string? fromDate = null, string? toDate = null)
        {
            int? empID = await GetCurrentEmployeeIdAsync();

            var result = await _approverService.GetPendingApprovalsAsync(empID, page, pageSize, search, sortColumn, sortDirection, productTypeId, fromDate, toDate);

            return Json(new
            {
                data = result.Items,
                totalRecords = result.TotalRecords,
                page = page,
                pageSize = pageSize
            });
        }
        #endregion

        #region Get Approved History
        [HttpGet]
        public async Task<IActionResult> GetApprovedHistory(int page = 1, int pageSize = 10, string search = "", string sortColumn = "RequisitionId",            string sortDirection = "asc",
            int? productTypeId = null,
            string? fromDate = null,
            string? toDate = null)
        {
            int? empID = await GetCurrentEmployeeIdAsync();

            var result = await _approverService.GetApprovedHistoryAsync(
                empID, page, pageSize, search, sortColumn, sortDirection,
                productTypeId, fromDate, toDate);

            return Json(new
            {
                data = result.Items,
                totalRecords = result.TotalRecords,
                page = page,
                pageSize = pageSize
            });
        }
        #endregion

        #region Get Requisition Details
        [HttpGet]
        public async Task<IActionResult> GetRequisitionDetails(int id)
        {
            try
            {
                int? empID = await GetCurrentEmployeeIdAsync();
                var details = await _approverService.GetRequisitionDetailsAsync(id, empID);

                if (details == null)
                    return NotFound(new { message = "Requisition not found" });

                return Ok(new { success = true, data = details });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
            }
        }
        #endregion

        #region Approve Requisition
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveRequisition(ApproveRequisitionViewModel model, BaseViewModel? baseView)
        {
            int? empID = await GetCurrentEmployeeIdAsync();

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

            var result = await _approverService.ApproveRequisitionAsync(model, empID, baseView);
            return Json(result);
        }
        #endregion

        #region Decline Requisition
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeclineRequisition(DeclineRequisitionViewModel model, BaseViewModel? baseView)
        {
            int? empID = await GetCurrentEmployeeIdAsync();

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

            var result = await _approverService.DeclineRequisitionAsync(model, empID, baseView);
            return Json(result);
        }
        #endregion

        #region Edit Approved Requisition
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditApprovedRequisition(EditApprovedRequisitionViewModel model, BaseViewModel? baseView)
        {
            int? empID = await GetCurrentEmployeeIdAsync();

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    );
                var messages = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .SelectMany(x => x.Value.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();


                return Json(new { success = false, errors = errors, message = messages });

                
            }

            var result = await _approverService.EditApprovedRequisitionAsync(model, empID, baseView);
            return Json(result);
        }
        #endregion

        #region Export
        [HttpPost]
        public async Task<IActionResult> GeneratePDF(string? fromDate = null, string? toDate = null, bool approved = false)
        {
            int? empID = await GetCurrentEmployeeIdAsync();
            int? orgID = await GetCurrentOrganizationIdAsync();

            var pdfBytes = await _approverService.GeneratePDF(orgID ?? 0, empID ?? 0, fromDate, toDate, approved);

            if (pdfBytes == null || pdfBytes.Length == 0)
                return NotFound("PDF generation failed");

            return File(pdfBytes, "application/pdf", "requisition_approvals.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> DownloadExcel(string? fromDate = null, string? toDate = null, bool approved = false)
        {
            int? empID = await GetCurrentEmployeeIdAsync();
            int? orgID = await GetCurrentOrganizationIdAsync();

            var excelBytes = await _approverService.GenerateExcel(orgID ?? 0, empID ?? 0, fromDate, toDate, approved);

            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "requisition_approvals.xlsx");
        }
        #endregion
    }
}
