using GCTL.Core.ViewModels.AttendanceManagement.AttendenceReportAlls;
using GCTL.Service.AttendanceManagement.EmployeeAttendenceReportAll.YearlyReports;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.AttendanceManagement.AttentendceReports.YearlyReport
{
    public class YearlyReportController : BaseController
    {
        private readonly IYearlyReportService _yearlyReportService;
        public YearlyReportController(ITranslateService translateService, IUserProfileService userProfileService, IYearlyReportService yearlyReportService) : base(translateService, userProfileService)
        {
            _yearlyReportService = yearlyReportService;
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<ActionResult<List<YearlySpecialDayReportVM>>> GetYearlySpecialDaysReport(
          )
        {
           
            int? organizationIds = 1;
            int? employeeIds = await GetCurrentEmployeeIdAsync();

            var result = await _yearlyReportService.GetYearlySpecialDaysReport(null, organizationIds, employeeIds, 2025);

            if (result == null || result.Count == 0)
            {
                return NotFound("No data found for the provided year.");
            }

            return Ok(result);
        }
    }

}

