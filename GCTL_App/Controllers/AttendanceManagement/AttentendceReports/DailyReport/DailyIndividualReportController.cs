using GCTL.Service.AttendanceManagement.EmployeeAttendenceReportAll.DailyReports;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.AttendanceManagement.AttentendceReports.DailyReport
{
    public class DailyIndividualReportController : BaseController
    {
        private readonly IDailyReportService _dailyReportService;
        public DailyIndividualReportController(ITranslateService translateService, IUserProfileService userProfileService, IDailyReportService dailyReportService) : base(translateService, userProfileService)
        {
            _dailyReportService = dailyReportService;
        }

        public IActionResult Index()
        {
            return View();
        }
        #region get one employee attendance report
        public async Task<IActionResult> GetEmployeeAttendance(int employeeId, int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "HolidayID", string sortOrder = "desc")
        {
            int? employeeIdss = await GetCurrentEmployeeIdAsync();
            if (employeeIdss.HasValue)
            {
                // If the employeeId parameter is not provided, use the current employee's ID
                employeeIdss = employeeId;
            }
            else
            {
                // If no employee ID is found, return an error response
                return Json(new { success = false, message = "Employee ID not found." });
            }
                // Call the service method to get the paginated attendance data for a specific employee
                var result = await _dailyReportService.GetIndividualEmployee(employeeId, pageNumber, pageSize, searchTerm, sortColumn, sortOrder);
            return Json(result);
        }

        #endregion
    }
}
