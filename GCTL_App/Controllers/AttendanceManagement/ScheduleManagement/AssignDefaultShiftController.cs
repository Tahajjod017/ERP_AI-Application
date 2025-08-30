using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.AssignDefaultShift;
using GCTL.Service.AttendanceManagement.ScheduleManagement.AssignDefaultShift;
using GCTL.Service.CommonService;
using GCTL.Service.Language;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using GCTL_App.ViewModels.AttendanceManagement.ScheduleManagement.AssignDefaultShift;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.AttendanceManagement.ScheduleManagement
{
    [Authorize]
    public class AssignDefaultShiftController : BaseController
    {
        #region Services & Repositories
        private readonly IAssignDefaultShiftService _assignDefaultShiftService;
        private readonly ICommonService _commonService;

        public AssignDefaultShiftController(ITranslateService translateService, IUserProfileService userProfileService, IAssignDefaultShiftService assignDefaultShiftService, ICommonService commonService) : base(translateService, userProfileService)
        {
            _assignDefaultShiftService = assignDefaultShiftService;
            _commonService = commonService;
        }
        #endregion


        #region Index
        public async Task<IActionResult> Index()
        {
            try
            {
                AssignDefaultShiftPageVM model = new AssignDefaultShiftPageVM();

                SetSmartPageCode(202900);

                ViewBag.OrganizationDD = new SelectList(await _commonService.GetOrganizations(), "Id", "Name");
                ViewBag.DepartmentDD = new SelectList(await _commonService.GetDepartments(), "Id", "Name");
                ViewBag.ShiftDD = new SelectList(await _commonService.GetShifts(), "Id", "Name");
                ViewBag.EmployeeList = await _commonService.GetEmpGroupedByDep();

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(new AssignDefaultShiftPageVM());
            }
        }
        #endregion


        #region SearchEmployees
        [HttpGet("SearchEmployees")]
        public async Task<IActionResult> SearchEmployees(string search, int pageSize = 50)
        {
            var result = await _commonService.SearchEmployees(search, pageSize);
            return Json(result);
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
                    //// userInfoService.SetUserInfo(model, User, HttpContext);
                    //var uniqueName = await _assignDefaultShiftService.IsNameUniqueAsync(model.ActionTakenName);
                    //if (!uniqueName)
                    //{
                    //    return Json(new { isSuccess = false, message = "This name already exists!" });
                    //}

                    await _assignDefaultShiftService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.ShiftID });
                }
                var orderedKeys = new[] { "OrganizationID", "ShiftID" };

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


        #region CheckConflicts
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckConflicts(AssignDefaultShiftSetupVM model)
        {
            var conflictList = await _assignDefaultShiftService.CheckConflictsAsync(model);

            if (conflictList.Any())
            {
                return Json(new { hasConflicts = true, conflicts = conflictList });
            }

            return Json(new { hasConflicts = false });
        }
        #endregion


        #region Update
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Update(AssignDefaultShiftSetupVM model)
        {
            try
            {
                await _assignDefaultShiftService.UpdateAsync(model);
                return Json(new { isSuccess = true, message = "Updated Successfully." });
            }
            catch(Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        #endregion


        #region UpdateEmpShift
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> UpdateEmpShift(AssignDefaultShiftSetupVM model)
        {
            try
            {
                await _assignDefaultShiftService.UpdateEmpShiftAsync(model);
                return Json(new { isSuccess = true, message = "Updated Successfully." });
            }
            catch(Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        #endregion


        #region GetById
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _assignDefaultShiftService.GetByIdAsync(id);
                if (result == null)
                {
                    return Json(new { isSuccess = false, message = "No data found!" });
                }

                return Json(new { isSuccess = true, data = result });
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


        #region GetDepartmentByOrganization
        [HttpGet]
        public async Task<IActionResult> GetDepartmentByOrganization(int? id)
        {
            var result = await _commonService.GetDepartmentsByOrgId(id);
            return Json(result);
        }
        #endregion


        #region GetEmployeesByOrgBraDepId
        [HttpGet]
        public async Task<IActionResult> GetEmployeesByOrgBraDepId(int? orgId, [FromQuery] List<int>? branchIds, [FromQuery] List<int>? depIds)
        {
            var result = await _commonService.GetEmployeesByOrgBraDepId(orgId, branchIds, depIds);
            return Json(result);
        }
        #endregion


        #region GetShiftByCompany
        [HttpGet]
        public async Task<JsonResult> GetShiftByCompany(int id)
        {
            var data = await _commonService.GetShiftsByOrgId(id);
            return Json(data);
        }
        #endregion
    }
}
