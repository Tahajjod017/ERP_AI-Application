using GCTL.Service.CommonService;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using GCTL_App.ViewModels.AttendanceManagement.ScheduleManagement.OfficeDayRoster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.AttendanceManagement.ScheduleManagement
{
    public class OffDayRosterController : BaseController
    {
        private readonly ICommonService _commonService;

        public OffDayRosterController(ITranslateService translateService, IUserProfileService userProfileService, ICommonService commonService) : base(translateService, userProfileService)
        {
            _commonService = commonService;
        }

        public async Task<IActionResult> Index()
        {
            RosterInOfficeDaysPageVM model = new RosterInOfficeDaysPageVM();
            SetSmartPageCode(203200);

            ViewBag.OrganizationDD = new SelectList(await _commonService.GetOrganizations(), "Id", "Name");
            ViewBag.BrnchDD = new SelectList(await _commonService.GetBranches(), "Id", "Name");
            ViewBag.DepartmentDD = new SelectList(await _commonService.GetDepartments(), "Id", "Name");
            ViewBag.ShiftDD = new SelectList(await _commonService.GetShifts(), "Id", "Name");
            ViewBag.EmployeeList = await _commonService.GetEmpGroupedByDep();

            return View(model);
        }
    }
}
