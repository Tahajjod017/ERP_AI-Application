using GCTL.Core.Helpers.AttendenceHelper;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.AttendanceManagement.AttentendceReports.MonthlyReport
{
    public class MonthlyIndividualReportController : BaseController
    {
        private readonly HolidayHelper _holidayHelper;
        private readonly WeekendHelper _weekendHelper;
        public MonthlyIndividualReportController(ITranslateService translateService, IUserProfileService userProfileService, HolidayHelper holidayHelper, WeekendHelper weekendHelper) : base(translateService, userProfileService)
        {
            _holidayHelper = holidayHelper;
            _weekendHelper = weekendHelper;
        }

        public IActionResult Index()
        {
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
        public IActionResult SomeAction2()
        {
            int organizationId = 1; // Example organization ID
            int organizationBranch = 3;
            DateTime startDate = new DateTime(2025, 08, 08); // Start date
            DateTime endDate = new DateTime(2025, 12, 31); // End date

            // Get the active holidays
            var activeHolidays = _weekendHelper.GetWeekendWeekdayNumbers(organizationId, organizationBranch);

            // Process and return the holidays (you can display them in a view or return as JSON)
            return View(activeHolidays); // or return Json(activeHolidays);
        }

    }
}
