using GCTL.Service.AttendanceManagement.ScheduleManagement.EmployeeShiftView;
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
        #region Services & Repositories
        private readonly ICommonService _commonService;
        private readonly IEmployeeShiftViewService _employeeShiftViewService;

        public EmployeeShiftViewController(ITranslateService translateService, IUserProfileService userProfileService, ICommonService commonService, IEmployeeShiftViewService employeeShiftViewService) : base(translateService, userProfileService)
        {
            _commonService = commonService;
            _employeeShiftViewService = employeeShiftViewService;
        }
        #endregion


        #region Index
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
        #endregion


        #region GetAll
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", int daysToShow = 7, DateTime? startDate = null)
        {
            try
            {
                var result = await _employeeShiftViewService.GetAllAsync(pageNumber, pageSize, searchTerm, daysToShow, startDate);

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        #endregion
    }
}
