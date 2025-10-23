using GCTL.Service.AttendanceManagement.EmployeeAttendenceReportAll.DailyReports;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.AttendanceManagement.AttentendceReports.DailyReport
{
    public class DailyReportForAllController : BaseController
    {
        private readonly IDailyReportService _dailyReportService;
        public DailyReportForAllController(ITranslateService translateService, IUserProfileService userProfileService, IDailyReportService dailyReportService) : base(translateService, userProfileService)
        {
            _dailyReportService = dailyReportService;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region get all employee attendance report
        public async Task<IActionResult> GetAllEmployeeAttendanceReport(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "AttendanceID", string sortOrder = "desc", int? organizationID = null)
        {
            var result = await _dailyReportService.GetAllEmployee(pageNumber, pageSize, searchTerm, sortColumn, sortOrder, organizationID);
            return Json(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetSummary()
        {
            var summary = await _dailyReportService.GetSummaryAll();
            return new JsonResult(summary); // Explicit JSON return
        }

        #endregion
        //public async Task<IActionResult> GetOrganization()
        //{

        //}
    }
}
