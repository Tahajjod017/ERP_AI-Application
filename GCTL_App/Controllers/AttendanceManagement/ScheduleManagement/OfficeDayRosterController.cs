using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.OfficeDayRoster;
using GCTL.Service.AttendanceManagement.ScheduleManagement.OfficeDayRoster;
using GCTL.Service.CommonService;
using GCTL.Service.Language;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using GCTL_App.ViewModels.AttendanceManagement.ScheduleManagement.OfficeDayRoster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.AttendanceManagement.ScheduleManagement
{
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


        #region GetAllFromStoredProc
        [HttpGet]
        public async Task<IActionResult> GetAllFromStoredProc(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "RosterInOfficeDayID", string sortOrder = "desc", int daysToShow = 7)
        {
            var result = await _assignDefaultShiftService.GetAllFromSPAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder, daysToShow);

            // Generate date headers (same as before)
            var startDate = DateTime.Today;
            var dateList = Enumerable.Range(0, daysToShow).Select(offset => startDate.AddDays(offset)).ToList();


            return Json(new
            {
                result = new
                {
                    data = result,
                    paginationInfo = new
                    {
                        currentPage = pageNumber,
                        pageSize,
                        totalItems = result.Count, // Update if your SP returns total count separately
                        totalPages = (int)Math.Ceiling((double)result.Count / pageSize),
                        startItem = (pageNumber - 1) * pageSize + 1,
                        endItem = Math.Min(pageNumber * pageSize, result.Count)
                    }
                },
                headers = dateList.Select(date => new
                {
                    day = date.ToString("ddd"),
                    date = date.ToString("yyyy-MM-dd")
                })
            });
        }
        #endregion


        #region GetAll
        //public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "RosterInOfficeDayID", string sortOrder = "desc", int daysToShow = 7, DateTime? startDate = null)
        //{
        //    var start = startDate ?? DateTime.Today; // default to today if not provided

        //    // Pass start date to service
        //    var result = await _assignDefaultShiftService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder, daysToShow, start);

        //    // Generate header dates based on provided start date
        //    var dateList = Enumerable.Range(0, daysToShow).Select(offset => start.AddDays(offset)).ToList();

        //    return Json(new
        //    {
        //        result,
        //        headers = dateList.Select(date => new
        //        {
        //            day = date.ToString("ddd"),
        //            date = date.ToString("dd MMM yyyy")
        //        }).ToList()
        //    });
        //}


        //public async Task<IActionResult> GetGrouped(int pageNumber = 1, int pageSize = 10, string searchTerm = "", string sortColumn = "RosterInOfficeDayID", string sortOrder = "desc", int daysToShow = 7, DateTime? startDate = null)
        //{
        //    var (data, pagination) = await _assignDefaultShiftService.GetAllGroupedAsync(
        //        pageNumber, pageSize, searchTerm, sortColumn, sortOrder, daysToShow, startDate);

        //    var headers = Enumerable.Range(0, daysToShow)
        //        .Select(i => (startDate ?? DateTime.Today).AddDays(i))
        //        .Select(date => new { day = date.ToString("ddd"), date = date.ToString("dd MMM yyyy") })
        //        .ToList();

        //    return Json(new
        //    {
        //        result = data,
        //        paginationInfo = pagination,
        //        headers = headers
        //    });
        //}
        #endregion


        #region UpdateEmpShiftAsync
        //[Permission("Edit", "OfficeDayRoster")]
        //[ValidateAntiForgeryToken]
        //[Route("OfficeDayRosterRoute/UpdateEmpShiftAsync")]
        //[HttpPost]
        //public async Task<IActionResult> UpdateEmpShiftAsync(RosterInOfficeDaysOverrideSetupVM model)
        //{
        //    try
        //    {
        //        if(model.RosterInOfficeDayID == null || model.RosterInOfficeDayID == 0)
        //        {
        //            return Json(new { isSuccess = false, message = "Something went wrong!" });
        //        }

        //        var result = await _assignDefaultShiftService.UpdateEmpShiftAsync(model);

        //        if(result == false)
        //        {
        //            return Json(new { isSuccess = false, message = "Something went wrong!" });
        //        }

        //        return Json(new { isSuccess = true, message = "Updated Successfully." });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { isSuccess = false, message = ex.Message });
        //    }
        //}
        #endregion


        #region Delete
        //public async Task<IActionResult> Delete(RosterDelVM model)
        //{
        //    try
        //    {
        //        if(model.Id == null || model.Id == 0)
        //        {
        //            return Json(new { isSuccess = false, message = "Something went wrong!" });
        //        }

        //        var result = await _assignDefaultShiftService.SoftDeleteAsync(model);
        //        if(result == null)
        //        {
        //            return Json(new { isSuccess = false, message = "Something went wrong!" });
        //        }
        //        return Json(new { isSuccess = true, message = "Deleted Successfully." });
        //    }
        //    catch(Exception ex)
        //    {
        //        return Json(new { isSuccess = false, message = ex.Message });
        //    }
        //}
        #endregion


        #region GerBranchByOrganization
        [HttpGet]
        public async Task<IActionResult> GerBranchByOrganization(int? id)
        {
            var result = await _commonService.GetBranchesByOrgId(id);
            return Json(result);
        }
        #endregion


        #region GetDepartmentByOrganization
        public async Task<IActionResult> GetDepartmentByOrganization(int? id)
        {
            var result = await _commonService.GetDepartmentsByOrgId(id);
            return Json(result);
        }
        #endregion


        #region GetEmployeeByOrganization
        public async Task<IActionResult> GetEmployeeByOrganization(int? id)
        {
            var result = await _commonService.GetEmployeesByOrgId(id);
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


        #region GetEmployeeByDepartment
        public async Task<IActionResult> GetEmployeeByDepartment(int? orgId, [FromQuery] List<int>? branchIds, [FromQuery] List<int>? depIds)
        {
            var result = await _commonService.GetEmployeesByOrgBraDepId(orgId, branchIds, depIds);
            return Json(result);
        }
        #endregion


        #region GetEmployeeByBranch
        public async Task<IActionResult> GetEmployeeByBranch(int? orgId, [FromQuery] List<int>? ids)
        {
            var result = await _commonService.GetEmployeesByOrgBraId(orgId, ids);
            return Json(result);
        }
        #endregion
    }
}
