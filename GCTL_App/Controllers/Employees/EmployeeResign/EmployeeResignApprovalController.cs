
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeResign;
using GCTL.Data.Models;
using GCTL.Service.Employees.EmployeeResign;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.Employees.EmployeeResign
{
    public class EmployeeResignApprovalController : BaseController
    {
        private readonly IGenericRepository<Departments> _departmentRepository;
        private readonly IGenericRepository<Designations> _designationRepository;
        private readonly IEmployeeResign _employeeResign;

        public EmployeeResignApprovalController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<Departments> departmentRepository, IGenericRepository<Designations> designationRepository, IEmployeeResign employeeResign) : base(translateService, userProfileService)
        {
            _departmentRepository = departmentRepository;
            _designationRepository = designationRepository;
            _employeeResign = employeeResign;
        }

        public IActionResult Index()
        {

            SetSmartPageCode(1115800);

            ViewBag.DepartmentDD = new SelectList(_departmentRepository.AllActive(), "DepartmentID", "DepartmentName");
            ViewBag.DesignationDD = new SelectList(_designationRepository.AllActive(), "DesignationID", "DesignationName");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPendingResignations(string dateRange, string department, string designation, int pageNumber = 1, int pageSize = 10, string searchTerm = "", string sortColumn = "", string sortDirection = "asc")
        {
            var resignations = await _employeeResign.GetPendingResignations(dateRange, department, designation);

            // Apply search
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                resignations = resignations.Where(r =>
                    r.EmployeeName.ToLower().Contains(searchTerm) ||
                    r.Department.ToLower().Contains(searchTerm) ||
                    r.Position.ToLower().Contains(searchTerm) ||
                    r.Reason.ToLower().Contains(searchTerm)).ToList();
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(sortColumn))
            {
                resignations = sortColumn switch
                {
                    "employeeName" => sortDirection == "asc" ? resignations.OrderBy(r => r.EmployeeName).ToList() : resignations.OrderByDescending(r => r.EmployeeName).ToList(),
                    "department" => sortDirection == "asc" ? resignations.OrderBy(r => r.Department).ToList() : resignations.OrderByDescending(r => r.Department).ToList(),
                    "position" => sortDirection == "asc" ? resignations.OrderBy(r => r.Position).ToList() : resignations.OrderByDescending(r => r.Position).ToList(),
                    "reason" => sortDirection == "asc" ? resignations.OrderBy(r => r.Reason).ToList() : resignations.OrderByDescending(r => r.Reason).ToList(),
                    "noticeDate" => sortDirection == "asc" ? resignations.OrderBy(r => DateTime.Parse(r.NoticeDate)).ToList() : resignations.OrderByDescending(r => DateTime.Parse(r.NoticeDate)).ToList(),
                    "lastWorkingDay" => sortDirection == "asc" ? resignations.OrderBy(r => DateTime.Parse(r.LastWorkingDay)).ToList() : resignations.OrderByDescending(r => DateTime.Parse(r.LastWorkingDay)).ToList(),
                    _ => resignations
                };
            }

            var totalCount = resignations.Count;
            var pagedResignations = resignations.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            var result = new ResignationListViewModel
            {
                Resignations = pagedResignations,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                SortColumn = sortColumn,
                SortDirection = sortDirection
            };

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetProcessedResignations(string dateRange, string department, string designation, int pageNumber = 1, int pageSize = 10, string searchTerm = "", string sortColumn = "", string sortDirection = "asc")
        {
            var resignations = await _employeeResign.GetProcessedResignations(dateRange, department, designation);

            // Apply search
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                resignations = resignations.Where(r =>
                    r.EmployeeName.ToLower().Contains(searchTerm) ||
                    r.Department.ToLower().Contains(searchTerm) ||
                    r.Position.ToLower().Contains(searchTerm) ||
                    r.Reason.ToLower().Contains(searchTerm)).ToList();
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(sortColumn))
            {
                resignations = sortColumn switch
                {
                    "employeeName" => sortDirection == "asc" ? resignations.OrderBy(r => r.EmployeeName).ToList() : resignations.OrderByDescending(r => r.EmployeeName).ToList(),
                    "department" => sortDirection == "asc" ? resignations.OrderBy(r => r.Department).ToList() : resignations.OrderByDescending(r => r.Department).ToList(),
                    "position" => sortDirection == "asc" ? resignations.OrderBy(r => r.Position).ToList() : resignations.OrderByDescending(r => r.Position).ToList(),
                    "reason" => sortDirection == "asc" ? resignations.OrderBy(r => r.Reason).ToList() : resignations.OrderByDescending(r => r.Reason).ToList(),
                    "processedDate" => sortDirection == "asc" ? resignations.OrderBy(r => DateTime.Parse(r.ProcessedDate)).ToList() : resignations.OrderByDescending(r => DateTime.Parse(r.ProcessedDate)).ToList(),
                    "lastWorkingDay" => sortDirection == "asc" ? resignations.OrderBy(r => DateTime.Parse(r.LastWorkingDay)).ToList() : resignations.OrderByDescending(r => DateTime.Parse(r.LastWorkingDay)).ToList(),
                    "status" => sortDirection == "asc" ? resignations.OrderBy(r => r.Status).ToList() : resignations.OrderByDescending(r => r.Status).ToList(),
                    _ => resignations
                };
            }

            var totalCount = resignations.Count;
            var pagedResignations = resignations.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            var result = new ResignationListViewModel
            {
                Resignations = pagedResignations,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                SortColumn = sortColumn,
                SortDirection = sortDirection
            };

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetResignationDetails(int id)
        {
            var details = await _employeeResign.GetAppResignationById(id);
            if (details == null)
            {
                return NotFound();
            }
            return Json(details);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessResignation(int id, string action, string hrComments, string handoverStatus, bool assetReturned, bool clearanceCompleted, bool documentsPrepared)
        {
            var result = await _employeeResign.ProcessResignation(id, action, hrComments, handoverStatus, assetReturned, clearanceCompleted, documentsPrepared);
            if (result.Success)
            {
                return Ok(new { success = true, message = "Action completed successfully." });
            }
            return BadRequest(new { success = false, message = result.Message });
        }


    }
}
