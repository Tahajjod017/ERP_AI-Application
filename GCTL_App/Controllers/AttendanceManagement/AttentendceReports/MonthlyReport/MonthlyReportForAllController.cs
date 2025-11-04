using GCTL.Core.Enums;
using GCTL.Core.Repository;
using GCTL.Data.Models;
using GCTL.Service.CommonService;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace GCTL_App.Controllers.AttendanceManagement.AttentendceReports.MonthlyReport
{
    public class MonthlyReportForAllController : BaseController
    {
        private readonly ICommonService _commonService;
        private readonly IGenericRepository<Organization> _organizationRepository;
        public MonthlyReportForAllController(ITranslateService translateService, IUserProfileService userProfileService, ICommonService commonService, IGenericRepository<Organization> organizationRepository) : base(translateService, userProfileService)
        {
            _commonService = commonService;
            _organizationRepository = organizationRepository;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.OrganizationDD = new SelectList(_organizationRepository.AllActive(), "OrganizationID", "OrganizationName", 1);

            ViewBag.DepartmentDD = await _commonService.GetDepartments();
            ViewBag.EmployeeList = await _commonService.GetEmpGroupedByDep();

            return View();
        }
        public async Task<IActionResult> GetOrganizationId()
        {
           
            return Json(new { });
        }
        public async Task<IActionResult> GetMonthDropdown()
        {


            return Json(new {});
        }
        //public async Task<SelectListItem> GetMonthAsync()
        //{
        //    // Get all months from the enum
        //    var months = Enum.GetValues(typeof(Month))
        //                     .Cast<Month>()
        //                     .Select(m => new SelectListItem
        //                     {
        //                         Text = m.ToString(), // Month name (e.g., January)
        //                         Value = ((int)m).ToString() // Month number (e.g., 1 for January)
        //                     })
        //                     .ToList();

        //    return Json();
        //}

    }
}
