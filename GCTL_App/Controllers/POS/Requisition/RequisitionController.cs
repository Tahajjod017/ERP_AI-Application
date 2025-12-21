using GCTL.Core.Enums;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Requsition.AddRequisition;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.POS.Requsition;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;

namespace GCTL_App.Controllers.POS.Requisition
{
    public class RequisitionController : BaseController
    {

        #region CTOR

        private readonly IGenericRepository<Requisitions> _requisitionRepository;
        private readonly IGenericRepository<ReqItemApprovalHistory> _reqItemHistoryRepository;
        private readonly IGenericRepository<ProductTypes> _productTypesRepository;
        private readonly IGenericRepository<Products> _productRepository;
       
        private readonly IGenericRepository<UnitTypes> _unitRepository;

        private readonly INewRequisitionService _newRequisitionService;

        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<Statuses> _statusRepository;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IGenericRepository<OrganizationBranches> _organizationBrancRepository;


        public RequisitionController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<Requisitions> requisitionRepository, IGenericRepository<ReqItemApprovalHistory> reqItemHistoryRepository, IGenericRepository<ProductTypes> productTypesRepository, IGenericRepository<Products> productRepository, IGenericRepository<UnitTypes> unitRepository, INewRequisitionService newRequisitionService, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository, IGenericRepository<Statuses> statusRepository, IGenericRepository<Organization> organizationRepository, IGenericRepository<OrganizationBranches> organizationBrancRepository) : base(translateService, userProfileService)
        {
            _requisitionRepository = requisitionRepository;
            _reqItemHistoryRepository = reqItemHistoryRepository;
            _productTypesRepository = productTypesRepository;
            _productRepository = productRepository;
            _unitRepository = unitRepository;
            _newRequisitionService = newRequisitionService;
            _employeeRepository = employeeRepository;
            _statusRepository = statusRepository;
            _organizationRepository = organizationRepository;
            _organizationBrancRepository = organizationBrancRepository;
        }
        #endregion

        #region Index
        public IActionResult Index()
        {
            ViewBag.ProductTypes = new SelectList(_productTypesRepository.AllActive().Select(e => new { Id = e.ProductTypeID, Name = e.ProductTypeName }).ToList(), "Id", "Name");
          
            ViewBag.Products = new SelectList(_productRepository.AllActive().Select(e => new { Id = e.ProductID, Name = e.ProductName }).ToList(), "Id", "Name");

            ViewBag.Units = new SelectList(_unitRepository.AllActive().Select(e => new { Id = e.UnitTypeID, Name = e.UnitTypeName }).ToList(), "Id", "Name");
           
            ViewBag.SubCategories = new SelectList(_statusRepository.AllActive().Select(e => new { Id = e.StatusID, Name = e.StatusName }).ToList(), "Id", "Name");

            ViewBag.Company = new SelectList(_organizationRepository.AllActive().Select(e => new { Id = e.OrganizationID, Name = e.OrganizationName }).ToList(), "Id", "Name");

            ViewBag.OrganizationBranches = new SelectList(_organizationBrancRepository.AllActive().Select(e => new { Id = e.OrganizationBranchID, Name = e.OrganizationBranchName }).ToList(), "Id", "Name");

            var parotiy = new List<object>
                    {
                        new { Id = 1, Name = "Normal" },
                        new { Id = 2, Name = "Advance" },
                        new { Id = 3, Name = "Urgent" }
                    };

            var priorities = Enum.GetValues(typeof(Priority))
                    .Cast<Priority>()
                    .Select(p => new
                    {
                        Id = (int)p,
                        Name = p.ToString()
                    });



            ViewBag.Priorities = new SelectList(priorities, "Id", "Name");



            int? empID = GetCurrentEmployeeIdAsync().Result;

            ViewBag.empIDInti = empID;

            ViewBag.Supervisors = new SelectList(_employeeRepository.AllActive()
                     .Include(e => e.EmployeeOfficeInfoEmployee)
                     .ThenInclude(r => r.Designation)
                     .Where(e => e.EmployeeID == empID)
                     .Select(e => new
                     {
                         id = e.EmployeeID,
                         name = e.FirstName + " " + e.LastName +
                                (e.EmployeeOfficeInfoEmployee.FirstOrDefault().Designation != null
                                    ? " (" + e.EmployeeOfficeInfoEmployee.FirstOrDefault().Designation.DesignationName + ")"
                                    : "")
                     }).ToList(),
                 "id", "name");

           

            return View(new CreateRequisitionViewModel());
        }

        #endregion

        #region Get All

        [HttpGet]
        public async Task<IActionResult> GetRequisitions(int page = 1, int pageSize = 10, string search = "",
          string sortColumn = "reqId", string sortDirection = "asc", int? projectId = null, int? productTypeId = null, string? FromDate = null, string? ToDate = null)
        {

            int? empID = GetCurrentEmployeeIdAsync().Result;
            var result = await _newRequisitionService.GetRequisitionListAsync(page, pageSize, search, sortColumn, sortDirection, projectId, productTypeId, empID, FromDate, ToDate);

            return Json(new
            {
                data = result.Items,
                totalRecords = result.TotalRecords,
                page = page,
                pageSize = pageSize
            });
        }

        #endregion

        #region GetBy Id

        [HttpGet]
        public JsonResult GetProductsByType(int productTypeId)
        {
            var products = _productRepository.AllActive()
                .Where(p => p.ProductTypeID == productTypeId)
                .Select(p => new { id = p.ProductID, productName = p.ProductName })
                .ToList();

            return Json(products);
        }


        [HttpGet]
        public JsonResult GetUnitByProduct(int productId)
        {

            var product = _productRepository.AllActive().Include(e => e.UnitType).FirstOrDefault(e => e.ProductID == productId);
            if (product == null) return Json(new { unitName = "" });
            var unitName = product.UnitType?.UnitTypeName ?? "";
            return Json(new { unitName });


        }


        [HttpGet]
        public IActionResult GetBranchesByOrganization(int organizationId)
        {
            var branches = _organizationBrancRepository.AllActive()
                .Where(b => b.OrganizationID == organizationId)
                .Select(b => new
                {
                    b.OrganizationBranchID,
                    b.OrganizationBranchName
                })
                .ToList();

            return Json(branches);
        }



        #endregion


        #region Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRequisitionViewModel model)
        {
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

            var result = await _newRequisitionService.SaveRequsitionAsync(model);
            return Ok(result);

        }

        #endregion

        #region Edit 

        [HttpGet]
        public async Task<IActionResult> GetRequisitionById(int id)
        {
            try
            {
                //return Ok(new { success = true }); 
                var empID = await GetCurrentEmployeeIdAsync();
                var requisition = await _newRequisitionService.GetRequisitionByIdAsync(id, empID);

                if (requisition == null)
                    return NotFound(new { message = "Requisition not found" });

                return Ok(new { success = true , data = requisition, message = "data get." });
            }
            catch (Exception ex)
            {
                // Log the exception
                // _logger.LogError(ex, "Error fetching requisition {Id}", id);

                return StatusCode(500, new { message = "An unexpected error occurred.", detail = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditRequisitionViewModel model, BaseViewModel? baseView)
        {
            var empID = await GetCurrentEmployeeIdAsync();


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

            var result = await _newRequisitionService.UpdateRequisitionAsync(model, empID, baseView);
            return Json(result);
        }

        #endregion

        #region Delete

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, BaseViewModel? baseView)
        {
            var empID = await GetCurrentEmployeeIdAsync();
            var result = await _newRequisitionService.DeleteRequisitionAsync(id, baseView, empID);
            return Json(result);
        }

        #endregion


        #region GeneratePDF
        [HttpPost]
        public async Task<IActionResult> GeneratePDF(string? FromDate = null, string? ToDate = null)
        {
            int employeeID = await GetCurrentEmployeeIdAsync() ?? 0;

            var pdfBytes = await _newRequisitionService.GeneratePDF(await GetCurrentOrganizationIdAsync() ?? 0, employeeID, FromDate, ToDate);
            if (pdfBytes == null || pdfBytes.Length == 0)
                return NotFound("PDF generation failed");

            return File(pdfBytes, "application/pdf", "report.pdf");
        }
        #endregion

        #region download Excel
        public async Task<IActionResult> DownloadExcel(string? FromDate = null, string? ToDate = null)
        {
            int employeeID = await GetCurrentEmployeeIdAsync() ?? 0;

            var excelBytes = await _newRequisitionService.GenerateXL(await GetCurrentOrganizationIdAsync() ?? 0, employeeID, FromDate, ToDate);
            return File(excelBytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "ProjectReport.xlsx");
        }
        #endregion

    }
}
