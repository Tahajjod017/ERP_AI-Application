using GCTL.Service.CommonService;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using GCTL_App.ViewModels.AttendanceManagement.ScheduleManagement.AssignSpiralPattern;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.AttendanceManagement.ScheduleManagement
{
    public class AssignSpiralPatternController : BaseController
    {
        private readonly ICommonService _commonService;

        public AssignSpiralPatternController(ITranslateService translateService, IUserProfileService userProfileService, ICommonService commonService) : base(translateService, userProfileService)
        {
            _commonService = commonService;
        }


        #region Index
        public async Task<IActionResult> Index()
        {
            AssignSpiralPatternPageVM model = new AssignSpiralPatternPageVM();
            SetSmartPageCode(203400);

            ViewBag.OrganizationDD = new SelectList(await _commonService.GetOrganizations(), "Id", "Name");
            ViewBag.DepartmentDD = await _commonService.GetDepartments();
            ViewBag.EmployeeList = await _commonService.GetEmpGroupedByDep();
            ViewBag.SpiralPatternTypeDD = new SelectList(await _commonService.GetSpiralPatternTypes(), "Id", "Name");
            ViewBag.SpiralPatternDD = await _commonService.GetSpiralPatterns();

            return View(model);
        }
        #endregion


        #region GetDepartmentByOrganization
        [HttpGet]
        public async Task<IActionResult> GetDepartmentByOrganization(int? id)
        {
            var result = await _commonService.GetDepartmentsByOrgId(id);
            return Json(result);
        }
        #endregion


        #region GetEmployeeByOrganization
        [HttpGet]
        public async Task<IActionResult> GetEmployeeByOrganization(int? id)
        {
            var result = await _commonService.GetEmployeesByOrgId(id);
            return Json(result);
        }
        #endregion


        #region GetSpiralPatternsByOrgPatternType
        [HttpGet]
        public async Task<IActionResult> GetSpiralPatternsByOrgPatternType(int orgId, int? typeId)
        {
            var result = await _commonService.GetSpiralPatternsByOrgPatternType(orgId, typeId);
            return Json(result);
        }
        #endregion
    }
}
