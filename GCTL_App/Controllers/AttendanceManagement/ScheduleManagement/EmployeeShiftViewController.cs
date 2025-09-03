using GCTL.Service.CommonService;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using GCTL_App.ViewModels.AttendanceManagement.ScheduleManagement.EmployeeShiftView;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.AttendanceManagement.ScheduleManagement
{
    [Authorize]
    public class EmployeeShiftViewController : BaseController
    {
        private readonly ICommonService _commonService;

        public EmployeeShiftViewController(ITranslateService translateService, IUserProfileService userProfileService, ICommonService commonService) : base(translateService, userProfileService)
        {
            _commonService = commonService;
        }

        public async Task<IActionResult> Index()
        {
            EmployeeShiftViewPageView model = new EmployeeShiftViewPageView();
            SetSmartPageCode(203500);

            ViewBag.OrganizationDD = new SelectList(await _commonService.GetOrganizations(), "Id", "Name");
            ViewBag.BrnchDD = new SelectList(await _commonService.GetBranches(), "Id", "Name");
            ViewBag.DepartmentDD = new SelectList(await _commonService.GetDepartments(), "Id", "Name");
            ViewBag.ShiftDD = new SelectList(await _commonService.GetShifts(), "Id", "Name");
            ViewBag.EmployeeList = await _commonService.GetEmpGroupedByDep();
            return View(model);
        }
    }
}
