using GCTL.Service.AttendanceManagement.ScheduleManagement.CreateSpiralPattern;
using GCTL.Service.CommonService;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using GCTL_App.ViewModels.AttendanceManagement.ScheduleManagement.CreateSpiralPattern;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.AttendanceManagement.ScheduleManagement
{
    public class CreateSpiralPatternController : BaseController
    {
        #region 
        private readonly ICreateSpiralPatternService _createSpiralPatternService;
        private readonly ICommonService _commonService;


        public CreateSpiralPatternController(
            ITranslateService translateService,
            IUserProfileService userProfileService,
            ICreateSpiralPatternService createSpiralPatternService,
            ICommonService commonService) : base(translateService, userProfileService)
        {
            _createSpiralPatternService = createSpiralPatternService;
            _commonService = commonService;
        }
        #endregion


        public async Task<IActionResult> Index()
        {
            CreateSpiralPatternPageVM model = new CreateSpiralPatternPageVM();
            SetSmartPageCode(203300);

            ViewBag.OrganizationDD = new SelectList(await _commonService.GetOrganizations(), "Id", "Name");
            ViewBag.BrnchDD = new SelectList(await _commonService.GetBranches(), "Id", "Name");
            ViewBag.DepartmentDD = new SelectList(await _commonService.GetDepartments(), "Id", "Name");
            ViewBag.ShiftDD = new SelectList(await _commonService.GetShifts(), "Id", "Name");
            ViewBag.CompensationDD = new SelectList(await _commonService.GetCompensation(), "Id", "Name");
            ViewBag.EmployeeList = await _commonService.GetEmpGroupedByDep();
            ViewBag.SpiralPatternTypeDD = new SelectList(await _commonService.GetSpiralPatternTypes(), "Id", "Name");

            return View(model);
        }


        #region GetShiftByOrganization
        [HttpGet]
        public async Task<IActionResult> GetShiftByOrganization(int? id)
        {
            var result = await _commonService.GetShiftsByOrgId(id);
            return Json(result);
        }
        #endregion
    }
}
