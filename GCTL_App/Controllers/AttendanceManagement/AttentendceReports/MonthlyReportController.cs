using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.AttendanceManagement.AttentendceReports
{
    public class MonthlyReportController : BaseController
    {
        public MonthlyReportController(ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ReportForAll()
        {
            // Add logic to fetch data here
            return View();
        }
        // Action for "Individual Report"
        public IActionResult IndividualReport()
        {
            // Add logic to fetch data here
            return View();
        }
    }
}
