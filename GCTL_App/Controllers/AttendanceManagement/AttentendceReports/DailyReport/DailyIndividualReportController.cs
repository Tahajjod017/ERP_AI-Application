using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AttendanceManagement.AttendenceReportAlls;
using GCTL.Data.Models;
using GCTL.Service.AttendanceManagement.EmployeeAttendenceReportAll.DailyReports;
using GCTL.Service.CommonService;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using GCTL_App.ViewModels.AttendanceManagement.ScheduleManagement.AssignSpiralPattern;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.AttendanceManagement.AttentendceReports.DailyReport
{
    public class DailyIndividualReportController : BaseController
    {
        private readonly IDailyReportService _dailyReportService;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly ICommonService _commonService;
        public DailyIndividualReportController(ITranslateService translateService, IUserProfileService userProfileService, IDailyReportService dailyReportService, IGenericRepository<Organization> organizationRepository, ICommonService commonService) : base(translateService, userProfileService)
        {
            _dailyReportService = dailyReportService;
            _organizationRepository = organizationRepository;
            _commonService = commonService;
        }

        public async Task<IActionResult> Index()
        {
            AttendanceEmployeeReportVM model = new AttendanceEmployeeReportVM();

            var result = await _commonService.GetOrganizations(search: "", page: 1, pageSize: 50);
            var organizations = result.Items;
            if (organizations.Count == 1)
            {
                model.OrganizationID = organizations[0].Id; // Fixed the issue by directly assigning to the OrganizationID property of the model.  
            }
            ViewBag.OrganizationDD = new SelectList(organizations, "Id", "Name");

           // ViewBag.OrganizationDD = new SelectList(_organizationRepository.AllActive(), "OrganizationID", "OrganizationName");
            ViewBag.DepartmentDD = await _commonService.GetDepartments();
            ViewBag.EmployeeList = await _commonService.GetEmpGroupedByDep();
            return View();
        }

        public IActionResult GetEmployeeData()
        {
            // Example data for developers and designers
            var developers = new List<EmployeeVM>
            {
            new EmployeeVM { Id = 2, Name = "Alam" },
            new EmployeeVM { Id = 3, Name = "Momin" }
            };

            var designers = new List<EmployeeVM>
            {
            new EmployeeVM { Id = 4, Name = "Name" },
            new EmployeeVM { Id = 5, Name = "Name" }
            };

            // Create the response object with the developer and designer lists
            var response = new
            {
                developers = developers,
                designers = designers
            };

            // Return JSON response
            return Json(response);
        }
        #region get one employee attendance report
        public async Task<IActionResult> GetEmployeeAttendance(int employeeId, int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "HolidayID", string sortOrder = "desc")
        {
            int? currentEmployeeId = await GetCurrentEmployeeIdAsync();
            int resolvedEmployeeId;

            if (currentEmployeeId.HasValue)
            {
                // If the employeeId parameter is not provided, use the current employee's ID
                resolvedEmployeeId = employeeId != 0 ? employeeId : currentEmployeeId.Value;
            }
            else
            {
                // If no employee ID is found, return an error response
                return Json(new { success = false, message = "Employee ID not found." });
            }

            // Call the service method to get the paginated attendance data for a specific employee
            var result = await _dailyReportService.GetIndividualEmployee(resolvedEmployeeId, pageNumber, pageSize, searchTerm, sortColumn, sortOrder);
            return Json(result);
        }

        #endregion

        #region
        public async Task<IActionResult> GetDepartments(int? organizationId)
        {
            var departments = await _dailyReportService.GetDepartmentByOrgId(organizationId);
            return Json(departments);
        }

        public async Task<IActionResult> GetEmployees(int? departmentId)
        {
            var departments = await _dailyReportService.GetEmployeeByDepId(departmentId);
            return Json(departments);
        }
        #endregion
    }
}
