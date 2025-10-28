using GCTL.Core.Repository;
using GCTL.Data.Models;
using GCTL.Service.CommonService;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.AttendanceManagement.AttentendceReports.AvailabilityReport
{
    public class AvailabilityReportController : BaseController
    {
        private readonly ICommonService _commonService;
        private readonly IGenericRepository<Organization> _organizationRepository;
        public AvailabilityReportController(ITranslateService translateService, IUserProfileService userProfileService, ICommonService commonService, IGenericRepository<Organization> organizationRepository) : base(translateService, userProfileService)
        {
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
    }
}
