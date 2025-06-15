using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.AssignDefaultShift;
using GCTL.Service.AttendanceManagement.ScheduleManagement.AssignDefaultShift;
using GCTL.Service.Language;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using GCTL_App.ViewModels.AttendanceManagement.ScheduleManagement.AssignDefaultShift;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.AttendanceManagement.ScheduleManagement
{
    public class AssignDefaultShiftController : BaseController
    {
        #region Services & Repositories
        private readonly IAssignDefaultShiftService _assignDefaultShiftService;

        public AssignDefaultShiftController(ITranslateService translateService, IUserProfileService userProfileService, IAssignDefaultShiftService assignDefaultShiftService) : base(translateService, userProfileService)
        {
            _assignDefaultShiftService = assignDefaultShiftService;
        }
        #endregion


        #region Index
        public async Task<IActionResult> Index()
        {
            AssignDefaultShiftPageVM model = new AssignDefaultShiftPageVM();

            SetSmartPageCode(202900);

            ViewBag.OrganizationDD = new SelectList(await _assignDefaultShiftService.GetCompanies(), "Id", "Name");
            ViewBag.DepartmentDD = new SelectList(await _assignDefaultShiftService.GetDepartments(), "Id", "Name");
            ViewBag.ShiftDD = new SelectList(await _assignDefaultShiftService.GetShift(), "Id", "Name");
            ViewBag.EmployeeList = await _assignDefaultShiftService.GetGroupedEmployees();

            return View(model);
        }
        #endregion


        #region Create
        [Permission("Create", "AssignDefaultShift")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(AssignDefaultShiftSetupVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if(model.EmployeeIDs == null || !model.EmployeeIDs.Any())
                    {
                        return Json(new { isSuccess = false, message = "Please choose an employee!" });
                    }
                    if(model.ShiftID == null)
                    {
                        return Json(new { isSuccess = false, message = "Please choose a shift!" });
                    }

                    //// userInfoService.SetUserInfo(model, User, HttpContext);
                    //var uniqueName = await _assignDefaultShiftService.IsNameUniqueAsync(model.ActionTakenName);
                    //if (!uniqueName)
                    //{
                    //    return Json(new { isSuccess = false, message = "This name already exists!" });
                    //}

                    await _assignDefaultShiftService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.ShiftID });
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


        #region GetAll
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "DefaultShiftID", string sortOrder = "desc", int? organizationID = null)
        {
            var result = await _assignDefaultShiftService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder, organizationID);

            return Json(result);
        }
        #endregion


        #region GetEmployeesByFilters
        [HttpGet]
        public async Task<JsonResult> GetEmployeesByFilters(string organizationIds, string departmentIds)
        {
            var orgIds = !string.IsNullOrEmpty(organizationIds)
                ? organizationIds.Split(',').Select(int.Parse).ToList()
                : new List<int>();

            var deptIds = !string.IsNullOrEmpty(departmentIds)
                ? departmentIds.Split(',').Select(int.Parse).ToList()
                : new List<int>();

            var data = await _assignDefaultShiftService.GetFilteredEmployees(orgIds, deptIds);
            return Json(data);
        }
        #endregion
    }
}
