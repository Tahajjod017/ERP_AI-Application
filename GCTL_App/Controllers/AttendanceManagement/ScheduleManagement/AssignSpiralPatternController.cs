using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.AssignSpiralPattern;
using GCTL.Service.AttendanceManagement.ScheduleManagement.AssignSpiralPattern;
using GCTL.Service.CommonService;
using GCTL.Service.Language;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using GCTL_App.ViewModels.AttendanceManagement.ScheduleManagement.AssignSpiralPattern;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.AttendanceManagement.ScheduleManagement
{
    [Authorize]
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
        [Permission("View", "AssignSpiralPattern")]
        public async Task<IActionResult> Index()
        {
            AssignSpiralPatternPageVM model = new AssignSpiralPatternPageVM();
            SetSmartPageCode(203400);

            var organizations = await _commonService.GetOrganizations();
            if (organizations.Count == 1)
            {
                model.Create.OrganizationID = organizations[0].Id;
            }
            ViewBag.OrganizationDD = new SelectList(organizations, "Id", "Name", model.Create.OrganizationID);
            //ViewBag.OrganizationDD = new SelectList(await _commonService.GetOrganizations(), "Id", "Name");
            ViewBag.DepartmentDD = await _commonService.GetDepartments();
            ViewBag.EmployeeList = await _commonService.GetEmpGroupedByDep();
            ViewBag.SpiralPatternTypeDD = new SelectList(await _commonService.GetSpiralPatternTypes(), "Id", "Name");
            ViewBag.SpiralPatternDD = await _commonService.GetSpiralPatterns();

            return View(model);
        }
        #endregion


        #region Create
        [Permission("Create", "AssignSpiralPattern")]
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
                var orderedKeys = new[] { "OrganizationID", "SpiralPatternTypeID", "SpiralPatternID", "StartDate", "EndDate" };

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


        #region GetByIdAsync
        [Route("AssignSpiralPattern/GetByIdAsync")]
        [HttpGet]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            try
            {
                var result = await _assignSpiralPatternService.GetByIdAsync(id);
                return Json(result);
            }
            catch(Exception ex)
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


        #region GetSpiralPatternsByOrgPatternType
        [HttpGet]
        public async Task<IActionResult> GetSpiralPatternsByOrgPatternType(int orgId, int? typeId)
        {
            var result = await _commonService.GetSpiralPatternsByOrgPatternType(orgId, typeId);
            return Json(result);
        }
        #endregion


        #region GetEmployeesByOrgBraDepId
        public async Task<IActionResult> GetEmployeesByOrgBraDepId(int? orgId, [FromQuery] List<int>? branchIds, [FromQuery] List<int>? depIds, string? search, int? page = 1, int? pageSize = 10)
        {
            var result = await _commonService.GetEmployeesByOrgBraDepId(orgId, branchIds, depIds, search, page, pageSize);
            return Json(result);
        }
        #endregion


        #region GetAllAsync
        [Route("AssignSpiralPattern/GetAllAsync")]
        [HttpGet]
        public async Task<IActionResult> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "SpiralPatternAssignListID", string sortOrder = "desc")
        {
            try
            {
                var result = await _assignSpiralPatternService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        #endregion


        #region GetSpiralPatternDetails
        public async Task<IActionResult> GetSpiralPatternDetails(int typeId, int id)
        {
            object result = null;
            if(typeId == 1)
            {
                result = await _assignSpiralPatternService.GetAllSpiralWeeklyPatternAsync(id);
            }
            else if (typeId == 2)
            {
                result = await _assignSpiralPatternService.GetAllSpiralFortnightlyPatternAsync(id);
            }
            else if (typeId == 3)
            {
                result = await _assignSpiralPatternService.GetAllSpiralMonthlyPatternAsync(id);
            }

            return Json(result);
        }
        #endregion


        #region Delete
        [Permission("Delete", "AssignSpiralPattern")]
        [Route("AssignSpiralPattern/Delete")]
        [HttpPost]
        public async Task<IActionResult> Delete(DeleteRequestVM requestVM)
        {
            try
            {
                if (requestVM.Ids == null || !requestVM.Ids.Any() || requestVM.Ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No data selected to delete." });
                }

                var result = await _assignSpiralPatternService.SoftDeleteAsync(requestVM);
                if (result == null)
                {
                    return Json(new { isSuccess = false, message = "No data found to delete." });
                }

                return Json(new { isSuccess = true, message = "Deleted Successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        #endregion
    }
}
