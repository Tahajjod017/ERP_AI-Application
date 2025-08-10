using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.AssignSpiralPattern;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.CreateSpiralPattern;
using GCTL.Service.AttendanceManagement.ScheduleManagement.AssignSpiralPattern;
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
        #region Services & Repositories
        private readonly ICommonService _commonService;
        private readonly IAssignSpiralPatternService _assignSpiralPatternService;

        public AssignSpiralPatternController(ITranslateService translateService, IUserProfileService userProfileService, ICommonService commonService, IAssignSpiralPatternService assignSpiralPatternService) : base(translateService, userProfileService)
        {
            _commonService = commonService;
            _assignSpiralPatternService = assignSpiralPatternService;
        }
        #endregion


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


        #region Create
        //[Permission("Create", "AssignSpiralPattern")]
        //[ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(AssignSpiralPatternSetupVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _assignSpiralPatternService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully." });
                }

                // Custom ordered validation message 
                var orderedKeys = new[] { "OrganizationID", "SpiralPatternTypeID", "SpiralPatternName" };

                foreach (var key in orderedKeys)
                {
                    if (ModelState.TryGetValue(key, out var entry) && entry.Errors.Any())
                    {
                        return Json(new { isSuccess = false, field = key, message = entry.Errors.First().ErrorMessage });
                    }
                }

                var errorMessage = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
                return Json(new { isSuccess = false, message = errorMessage ?? "Something went wrong." });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
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
