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

                //var organizations = await _commonService.GetOrganizations();
                //if (organizations.Count == 1)
                //{
                //    model.Setup.OrganizationID = organizations[0].Id;  
                //}
                //ViewBag.OrganizationDD = new SelectList(organizations, "Id", "Name", model.Setup.OrganizationID);
                var result = await _commonService.GetOrganizations(search: "", page: 1, pageSize: 50);
                var organizations = result.Items;
                if (organizations.Count == 1)
                {
                    model.Setup.OrganizationID = organizations[0].Id;
                }
                ViewBag.OrganizationDD = new SelectList(organizations, "Id", "Name");

                var branches = await _commonService.GetBranches();
                if(branches.Count == 1)
                {
                    model.Setup.BranchIDs = branches[0].Id.HasValue ? new List<int> { branches[0].Id.Value } : new List<int>();
                }
                ViewBag.BrnchDD = new SelectList(branches, "Id", "Name", model.Setup.BranchIDs);

                var departments = await _commonService.GetDepartments();
                if(departments.Count == 1)
                {
                    model.Setup.DepartmentIDs = departments[0].Id.HasValue ? new List<int> { departments[0].Id.Value } : new List<int>();
                }
                ViewBag.DepartmentDD = new SelectList(departments, "Id", "Name");

                var shifts = await _commonService.GetShifts();
                if(shifts.Count == 1)
                {
                    model.Setup.ShiftID = shifts[0].Id;
                }
                ViewBag.ShiftDD = new SelectList(shifts, "Id", "Name", model.Setup.ShiftID);

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
            try
            {
                var result = await _assignDefaultShiftService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder, organizationID);
                
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        #endregion


        #region GetBranchesByOrgId
        [Route("AssignDefaultShift/GetBranchesByOrgId")]
        [HttpGet]
        public async Task<IActionResult> GetBranchesByOrgId(int? orgId)
        {
            var result = await _commonService.GetBranchesByOrgId(orgId);
            return Json(result);
        }
        #endregion


        #region GetDepartmentByOrganization
        [Route("AssignDefaultShift/GetDepartmentByOrganization")]
        [HttpGet]
        public async Task<IActionResult> GetDepartmentByOrganization(int? id)
        {
            var result = await _commonService.GetDepartmentsByOrgId(id);
            return Json(result);
        }
        #endregion


        #region GetEmployeesByOrgBraDepId
        [Route("AssignDefaultShift/GetEmployeesByOrgBraDepId")]
        [HttpGet]
        public async Task<IActionResult> GetEmployeesByOrgBraDepId(int? orgId, [FromQuery] List<int>? branchIds, [FromQuery] List<int>? depIds, string? search, int? page = 1, int? pageSize = 50)
        {
            var result = await _commonService.GetEmployeesByOrgBraDepId(orgId, branchIds, depIds, search, page, pageSize);
            return Json(new
            {
                items = result.Items.Select(x => new {
                    value = x.Id,
                    label = x.Name,
                    group = x.GroupName
                }),
                hasMore = result.HasMore
            });
        }
        #endregion


        #region GetShiftByCompany
        [Route("AssignDefaultShift/GetShiftByCompany")]
        [HttpGet]
        public async Task<JsonResult> GetShiftByCompany(int id)
        {
            var data = await _commonService.GetShiftsByOrgId(id);
            return Json(data);
        }
        #endregion
    }
}
