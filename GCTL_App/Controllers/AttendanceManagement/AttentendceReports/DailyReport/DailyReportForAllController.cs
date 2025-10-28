using GCTL.Core.Repository;
using GCTL.Data.Models;
using GCTL.Service.AttendanceManagement.EmployeeAttendenceReportAll.DailyReports;
using GCTL.Service.CommonService;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.AttendanceManagement.AttentendceReports.DailyReport
{
    public class DailyReportForAllController : BaseController
    {
        private readonly IDailyReportService _dailyReportService;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly ICommonService _commonService;
        public DailyReportForAllController(ITranslateService translateService, IUserProfileService userProfileService, IDailyReportService dailyReportService, IGenericRepository<Organization> organizationRepository, ICommonService commonService) : base(translateService, userProfileService)
        {
            _dailyReportService = dailyReportService;
            _organizationRepository = organizationRepository;
            _commonService = commonService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.OrganizationDD = new SelectList(_organizationRepository.AllActive(), "OrganizationID", "OrganizationName");

            ViewBag.DepartmentDD = await _commonService.GetDepartments();
            ViewBag.EmployeeList = await _commonService.GetEmpGroupedByDep();
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
