using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AttendanceManagement.AttendenceReportAlls;
using GCTL.Data.Models;
using GCTL.Service.AttendanceManagement.EmployeeAttendenceReportAll.YearlyReports;
using GCTL.Service.CommonService;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.AttendanceManagement.AttentendceReports.YearlyReport
{
    public class YearlyReportController : BaseController
    {
        private readonly IYearlyReportService _yearlyReportService;
        private readonly ICommonService _commonService;
        private readonly IGenericRepository<Organization> _organizationRepository;
        public YearlyReportController(ITranslateService translateService, IUserProfileService userProfileService, IYearlyReportService yearlyReportService, ICommonService commonService, IGenericRepository<Organization> organizationRepository) : base(translateService, userProfileService)
        {
            _yearlyReportService = yearlyReportService;
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

