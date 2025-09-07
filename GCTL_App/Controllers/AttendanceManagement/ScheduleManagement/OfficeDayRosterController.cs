using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.OfficeDayRoster;
using GCTL.Service.AttendanceManagement.ScheduleManagement.OfficeDayRoster;
using GCTL.Service.CommonService;
using GCTL.Service.Language;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using GCTL_App.ViewModels.AttendanceManagement.ScheduleManagement.OfficeDayRoster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.AttendanceManagement.ScheduleManagement
{
    [Authorize]
    public class OfficeDayRosterController : BaseController
    {
        #region Services & Repositories
        private readonly IOfficeDayRosterService _assignDefaultShiftService;
        private readonly ICommonService _commonService;

        public OfficeDayRosterController(ITranslateService translateService,
            IUserProfileService userProfileService,
            IOfficeDayRosterService assignDefaultShiftService,
            ICommonService commonService) : base(translateService, userProfileService)
        {
            _assignDefaultShiftService = assignDefaultShiftService;
            _commonService = commonService;
        }
        #endregion


        #region Index
        public async Task<IActionResult> Index()
        {
            RosterInOfficeDaysPageVM model = new RosterInOfficeDaysPageVM();
            SetSmartPageCode(203100);

            ViewBag.OrganizationDD = new SelectList(await _commonService.GetOrganizations(), "Id", "Name");
            ViewBag.BrnchDD = new SelectList(await _commonService.GetBranches(), "Id", "Name");
            ViewBag.DepartmentDD = new SelectList(await _commonService.GetDepartments(), "Id", "Name");
            ViewBag.ShiftDD = new SelectList(await _commonService.GetShifts(), "Id", "Name");
            ViewBag.EmployeeList = await _commonService.GetEmpGroupedByDep();

            return View(model);
        }
        #endregion


        #region SearchOrganizations / OrganizationDD
        [HttpGet]
        public async Task<IActionResult> SearchOrganizations(string search, int page = 1, int pageSize = 50)
        {
            var result = await _commonService.SearchOrganizations(search, page, pageSize);
            return Json(new
            {
                items = result.Items.Select(x => new {
                    value = x.Id,
                    label = x.Name,
                    group = x.GroupName // Optional: only if you want to group
                }),
                hasMore = result.HasMore
            });
        }
        #endregion


        #region Create
        [Permission("Create", "OfficeDayRoster")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(RosterInOfficeDaysSetupVM model)
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
                    return Json(new { isSuccess = true, message = "Saved Successfully." });
                }

                // Custom ordered validation message 
                var orderedKeys = new[] { "OrganizationID", "ShiftID", "StartDate", "EndDate" };

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


        #region GetAll
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", int daysToShow = 7, DateTime? startDate = null)
        {
            try
            {
                var result = await _assignDefaultShiftService.GetAllAsync(pageNumber, pageSize, searchTerm, daysToShow, startDate);

                return Json(result);
            }
            catch(Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }

        #region GetEmployeesPaged
        [HttpGet]
        public async Task<IActionResult> GetEmployeesPaged(int pageNumber = 1, int pageSize = 5, string searchTerm = "", int daysToShow = 7, DateTime? startDate = null)
        {
            var result = await _assignDefaultShiftService.GetPagedEmployeesAsync(pageNumber, pageSize, searchTerm);

            int totalPages = (int)Math.Ceiling((double)result.TotalCount / pageSize);

            return Json(new
            {
                data = result.Data,
                totalCount = result.TotalCount,
                pageNumber,
                pageSize,
                totalPages,
                currentPage = pageNumber,
                pageNumbers = Enumerable.Range(1, totalPages).ToList()
            });
            //return Json(new
            //{
            //    data = result.Data,
            //    totalCount = result.TotalCount,
            //    result = result
            //});
        }
        #endregion

        #endregion


        #region UpdateEmpShiftAsync
        [Permission("Edit", "OfficeDayRoster")]
        //[ValidateAntiForgeryToken]
        [Route("OfficeDayRoster/UpdateEmpShiftAsync")]
        [HttpPost]
        public async Task<IActionResult> UpdateEmpShiftAsync(RosterInOfficeDayEditVM model)
        {
            try
            {
                if (model.RosterInOfficeDayIdEdit == null || model.RosterInOfficeDayIdEdit == 0)
                {
                    return Json(new { isSuccess = false, message = "Something went wrong!" });
                }

                var result = await _assignDefaultShiftService.UpdateEmpShiftAsync(model);

                if (result == false)
                {
                    return Json(new { isSuccess = false, message = "Something went wrong!" });
                }

                return Json(new { isSuccess = true, message = "Updated Successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        #endregion


        #region AddEmpShiftAsync
        [Permission("Create", "OfficeDayRoster")]
        //[ValidateAntiForgeryToken]
        [Route("OfficeDayRoster/AddEmpShiftAsync")]
        [HttpPost]
        public async Task<IActionResult> AddEmpShiftAsync(RosterInOfficeDayModalAddVM model)
        {
            try
            {
                if (model.RosterInOfficeDayIdAdd == null || model.RosterInOfficeDayIdAdd == 0)
                {
                    return Json(new { isSuccess = false, message = "Something went wrong!" });
                }

                var result = await _assignDefaultShiftService.AddEmpShiftAsync(model);

                if (result == false)
                {
                    return Json(new { isSuccess = false, message = "Something went wrong!" });
                }

                return Json(new { isSuccess = true, message = "Updated Successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        #endregion


        #region GetBranchesByOrgId
        [HttpGet]
        public async Task<IActionResult> GetBranchesByOrgId(int? id)
        {
            var result = await _commonService.GetBranchesByOrgId(id);
            return Json(result);
        }
        #endregion


        #region GetDepartmentsByOrgId
        public async Task<IActionResult> GetDepartmentsByOrgId(int? id)
        {
            var result = await _commonService.GetDepartmentsByOrgId(id);
            return Json(result);
        }
        #endregion


        #region GetShiftByOrganization
        public async Task<IActionResult> GetShiftByOrganization(int? id)
        {
            var result = await _commonService.GetShiftsByOrgId(id);
            return Json(result);
        }
        #endregion


        #region GetEmployeesByOrgBraDepId
        [Route("OfficeDayRoster/GetEmployeesByOrgBraDepId")]
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
    }
}
