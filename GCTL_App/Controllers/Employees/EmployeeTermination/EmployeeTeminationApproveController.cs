using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeTermination;
using GCTL.Data.Models;
using GCTL.Service.Employees.EmployeeTermination;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.Employees.EmployeeTermination
{
    public class EmployeeTerminationApprovalController : BaseController
    {
        #region CTOR
        private readonly IGenericRepository<Departments> _departmentRepository;
        private readonly IGenericRepository<Designations> _designationRepository;
        private readonly IGenericRepository<TerminationTypes> _terminationTypeRepository;
        private readonly IEmployeeTermination _employeeTermination;

        public EmployeeTerminationApprovalController(
            ITranslateService translateService,
            IUserProfileService userProfileService,
            IGenericRepository<Departments> departmentRepository,
            IGenericRepository<Designations> designationRepository,
            IGenericRepository<TerminationTypes> terminationTypeRepository,
            IEmployeeTermination employeeTermination) : base(translateService, userProfileService)
        {
            _departmentRepository = departmentRepository;
            _designationRepository = designationRepository;
            _terminationTypeRepository = terminationTypeRepository;
            _employeeTermination = employeeTermination;
        }
        #endregion

        #region Index
        public IActionResult Index()
        {
            SetSmartPageCode(11144800);

            ViewBag.DepartmentDD = new SelectList(_departmentRepository.AllActive(), "DepartmentID", "DepartmentName");
            ViewBag.DesignationDD = new SelectList(_designationRepository.AllActive(), "DesignationID", "DesignationName");
            ViewBag.TerminationTypeDD = new SelectList(_terminationTypeRepository.AllActive(), "TerminationTypeID", "TerminationTypeName");

            return View();
        }

        #endregion

        #region Pending

        [HttpGet]
        public async Task<IActionResult> GetPendingTerminations(string dateRange, string department, string designation, string terminationType, int pageNumber = 1, int pageSize = 10, string searchTerm = "", string sortColumn = "", string sortDirection = "asc")
        {
            var currentUser = await GetCurrentEmployeeIdAsync();

            var terminations = await _employeeTermination.GetPendingTerminations(dateRange, department, designation, terminationType, imgSrcThumb, currentUser);

            // Apply search
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                terminations = terminations.Where(t =>
                    t.EmployeeName.ToLower().Contains(searchTerm) ||
                    t.Department.ToLower().Contains(searchTerm) ||
                    t.Position.ToLower().Contains(searchTerm) ||
                    t.Reason.ToLower().Contains(searchTerm) ||
                    t.TerminationType.ToLower().Contains(searchTerm)).ToList();
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(sortColumn))
            {
                terminations = sortColumn switch
                {
                    "employeeName" => sortDirection == "asc" ? terminations.OrderBy(t => t.EmployeeName).ToList() : terminations.OrderByDescending(t => t.EmployeeName).ToList(),
                    "department" => sortDirection == "asc" ? terminations.OrderBy(t => t.Department).ToList() : terminations.OrderByDescending(t => t.Department).ToList(),
                    "position" => sortDirection == "asc" ? terminations.OrderBy(t => t.Position).ToList() : terminations.OrderByDescending(t => t.Position).ToList(),
                    "terminationType" => sortDirection == "asc" ? terminations.OrderBy(t => t.TerminationType).ToList() : terminations.OrderByDescending(t => t.TerminationType).ToList(),
                    "reason" => sortDirection == "asc" ? terminations.OrderBy(t => t.Reason).ToList() : terminations.OrderByDescending(t => t.Reason).ToList(),
                    "noticeDate" => sortDirection == "asc" ? terminations.OrderBy(t => t.NoticeDate).ToList() : terminations.OrderByDescending(t => t.NoticeDate).ToList(),
                    "terminationDate" => sortDirection == "asc" ? terminations.OrderBy(t => t.TerminationDate).ToList() : terminations.OrderByDescending(t => t.TerminationDate).ToList(),
                    _ => terminations
                };
            }

            var totalCount = terminations.Count;
            var pagedTerminations = terminations.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            var result = new TerminationListViewModel
            {
                Terminations = pagedTerminations,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                SortColumn = sortColumn,
                SortDirection = sortDirection
            };

            return Json(result);

            
        }

        #endregion

        #region Approved

        [HttpGet]
        public async Task<IActionResult> GetProcessedTerminations(string dateRange, string department, string designation, string terminationType, int pageNumber = 1, int pageSize = 10, string searchTerm = "", string sortColumn = "", string sortDirection = "asc")
        {
            var currentUser = await GetCurrentEmployeeIdAsync();

            var terminations = await _employeeTermination.GetProcessedTerminations(dateRange, department, designation, terminationType, imgSrcThumb, currentUser);

            // Apply search
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                terminations = terminations.Where(t =>
                    t.EmployeeName.ToLower().Contains(searchTerm) ||
                    t.Department.ToLower().Contains(searchTerm) ||
                    t.Position.ToLower().Contains(searchTerm) ||
                    t.Reason.ToLower().Contains(searchTerm) ||
                    t.TerminationType.ToLower().Contains(searchTerm)).ToList();
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(sortColumn))
            {
                terminations = sortColumn switch
                {
                    "employeeName" => sortDirection == "asc" ? terminations.OrderBy(t => t.EmployeeName).ToList() : terminations.OrderByDescending(t => t.EmployeeName).ToList(),
                    "department" => sortDirection == "asc" ? terminations.OrderBy(t => t.Department).ToList() : terminations.OrderByDescending(t => t.Department).ToList(),
                    "position" => sortDirection == "asc" ? terminations.OrderBy(t => t.Position).ToList() : terminations.OrderByDescending(t => t.Position).ToList(),
                    "terminationType" => sortDirection == "asc" ? terminations.OrderBy(t => t.TerminationType).ToList() : terminations.OrderByDescending(t => t.TerminationType).ToList(),
                    "reason" => sortDirection == "asc" ? terminations.OrderBy(t => t.Reason).ToList() : terminations.OrderByDescending(t => t.Reason).ToList(),
                    "processedDate" => sortDirection == "asc" ? terminations.OrderBy(t => t.ProcessedDate).ToList() : terminations.OrderByDescending(t => t.ProcessedDate).ToList(),
                    "terminationDate" => sortDirection == "asc" ? terminations.OrderBy(t => t.TerminationDate).ToList() : terminations.OrderByDescending(t => t.TerminationDate).ToList(),
                    "status" => sortDirection == "asc" ? terminations.OrderBy(t => t.Status).ToList() : terminations.OrderByDescending(t => t.Status).ToList(),
                    _ => terminations
                };
            }

            var totalCount = terminations.Count;
            var pagedTerminations = terminations.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            var result = new TerminationListViewModel
            {
                Terminations = pagedTerminations,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                SortColumn = sortColumn,
                SortDirection = sortDirection
            };

            return Json(result);
        }

        #endregion

        #region Get By ID And Action

        [HttpGet]
        public async Task<IActionResult> GetTerminationDetails(int id)
        {
            var details = await _employeeTermination.GetAppTerminationById(id);
            if (details == null)
            {
                return NotFound();
            }
            return Json(details);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessTermination(int id, string action, string hrComments, string handoverStatus, bool assetReturned, bool clearanceCompleted, bool documentsPrepared, CommonBaseViewModel? baseModel)
        {
            try
            {
                var result = await _employeeTermination.ProcessTermination(id, action, hrComments, handoverStatus, assetReturned, clearanceCompleted, documentsPrepared, baseModel);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }
        #endregion
    }
}