using GCTL.Core.Helpers.AttendenceHelper;
using GCTL.Core.Repository;
using GCTL.Data.Models;
using GCTL.Service.AttendanceManagement.EmployeeAttendenceReportAll.MonthlyReports;
using GCTL.Service.CommonService;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.AttendanceManagement.AttentendceReports.MonthlyReport
{
    public class MonthlyIndividualReportController : BaseController
    {
        private readonly HolidayHelper _holidayHelper;
        private readonly WeekendHelper _weekendHelper;
        private readonly IMonthlyReportService _monthlyReportService;
        private readonly ICommonService _commonService;
        private readonly IGenericRepository<Organization> _organizationRepository;
        public MonthlyIndividualReportController(ITranslateService translateService, IUserProfileService userProfileService, HolidayHelper holidayHelper, WeekendHelper weekendHelper, IMonthlyReportService monthlyReportService, ICommonService commonService, IGenericRepository<Organization> organizationRepository) : base(translateService, userProfileService)
        {
            _holidayHelper = holidayHelper;
            _weekendHelper = weekendHelper;
            _monthlyReportService = monthlyReportService;
            _commonService = commonService;
            _organizationRepository = organizationRepository;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.OrganizationDD = new SelectList(_organizationRepository.AllActive(), "OrganizationID", "OrganizationName");

            ViewBag.DepartmentDD = await _commonService.GetDepartments();
            ViewBag.EmployeeList = await _commonService.GetEmpGroupedByDep();

            return View();
        }
        public IActionResult SomeAction()
        {
            int organizationId = 1; // Example organization ID
           
            DateTime startDate = new DateTime(2025, 08, 08); // Start date
            DateTime endDate = new DateTime(2025, 12, 31); // End date

            // Get the active holidays
            var activeHolidays = _holidayHelper.GetActiveHolidays(organizationId, startDate, endDate);

            // Process and return the holidays (you can display them in a view or return as JSON)
            return View(activeHolidays); // or return Json(activeHolidays);
        }
        public async Task<IActionResult> GetMonthlyIndividual(
                                int pageNumber = 1,
                                int pageSize = 5,
                                string searchTerm = "",
                                string sortColumn = "AttendanceID",
                                string sortOrder = "desc",
                                int? organizationID = null,
                                List<int>? departmentIds = null,
                                List<int>? employeeIds = null,
                                string? monthYear = null,
                                int? employeeId = null)
        {
            // Use current employee ID if not provided
            var resolvedEmployeeId = employeeId ?? await GetCurrentEmployeeIdAsync();

            // Use current month if not provided
            var resolvedMonthYear = monthYear ?? DateTime.Now.ToString("yyyy-MM");

            var dataOfMonth = await _monthlyReportService.GetMonthlyAttendanceReport(
                pageNumber,
                pageSize,
                searchTerm,
                sortColumn,
                sortOrder,
                organizationID,
                departmentIds,
                employeeIds,
                resolvedMonthYear,
                resolvedEmployeeId);

            return Json(dataOfMonth);
        }
        //[HttpGet]
        //public IActionResult SomeAction2()
        //{
        //    var departmentId = (int?)null;
        //    var organizationId = 1;
        //    var employeeId = 14;
        //    var monthyear = "2025-07"; // Example month-year string
        //    var dataOfMonth = _monthlyReportService.GetMonthlyAttendanceReport(departmentId, organizationId, employeeId, monthyear);

        //    return Json(dataOfMonth);
        //}


    }
}
