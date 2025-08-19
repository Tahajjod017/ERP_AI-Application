using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.OffDayRoster;
using GCTL.Data.Models;
using GCTL.Service.AttendanceManagement.ScheduleManagement.OffDayRoster;
using GCTL.Service.CommonService;
using GCTL.Service.Language;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using GCTL_App.ViewModels.AttendanceManagement.ScheduleManagement.OffDayRoster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;

namespace GCTL_App.Controllers.AttendanceManagement.ScheduleManagement
{
    public class OffDayRosterController : BaseController
    {
        #region Services & repositories
        private readonly ICommonService _commonService;
        private readonly IOffDayRosterService _offDayRosterService;


        public OffDayRosterController(ITranslateService translateService,
            IUserProfileService userProfileService,
            ICommonService commonService,
            IOffDayRosterService offDayRosterService) : base(translateService, userProfileService)
        {
            _commonService = commonService;
            _offDayRosterService = offDayRosterService;
        }
        #endregion


        #region Index
        public async Task<IActionResult> Index()
        {
            RosterInOffDayPageVM model = new RosterInOffDayPageVM();
            SetSmartPageCode(203200);

            ViewBag.OrganizationDD = new SelectList(await _commonService.GetOrganizations(), "Id", "Name");
            ViewBag.BrnchDD = new SelectList(await _commonService.GetBranches(), "Id", "Name");
            ViewBag.DepartmentDD = new SelectList(await _commonService.GetDepartments(), "Id", "Name");
            ViewBag.ShiftDD = new SelectList(await _commonService.GetShifts(), "Id", "Name");
            ViewBag.CompensationDD = new SelectList(await _commonService.GetCompensation(), "Id", "Name");
            ViewBag.EmployeeList = await _commonService.GetEmpGroupedByDep();

            return View(model);
        }
        #endregion


        #region Create
        //[Permission("Create", "OffDayRoster")]
        //[ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(RosterInOffDaySetupVM model)
        {
            if (model.DayDate != null && model.CompensationTypeID == 3)
            {
                if (model.ExchangeDate == null || model.ExchangeDate.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "Please select Exchange Date!" });
                }

                if (model.ExchangeDate.Count != model.DayDate.Count)
                {
                    return Json(new { isSuccess = false, message = "The number of Exchange Dates must match the number of Selected Dates." });
                }

                for (int i = 0; i < model.ExchangeDate.Count; i++)
                {
                    if (model.ExchangeDate[i] <= model.DayDate[i])
                    {
                        return Json(new { isSuccess = false, message = $"Exchange Date at index {i + 1} must be greater than the corresponding Day Date." });
                    }
                }
            }

            try
            {
                if (ModelState.IsValid)
                {
                    await _offDayRosterService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully." });
                }

                // Custom ordered validation message 
                var orderedKeys = new[] { "OrganizationID", "DayDate", "ShiftID", "CompensationTypeID" };

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


        #region UpdateEmpShiftAsync
        [Permission("Edit", "OffDayRoster")]
        //[ValidateAntiForgeryToken]
        [Route("OffDayRoster/UpdateEmpShiftAsync")]
        [HttpPost]
        public async Task<IActionResult> UpdateEmpShiftAsync(RosterInOffDayEditVM model)
        {
            try
            {
                if (model.RosterInHolyDayIdEdit == null || model.RosterInHolyDayIdEdit == 0)
                {
                    return Json(new { isSuccess = false, message = "Something went wrong!" });
                }

                var result = await _offDayRosterService.UpdateEmpShiftAsync(model);

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


        #region GetBranchByOrganization
        [HttpGet]
        public async Task<IActionResult> GetBranchByOrganization(int? id)
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


        #region GetShiftByOrganization
        public async Task<IActionResult> GetShiftByOrganization(int? id)
        {
            var result = await _commonService.GetShiftsByOrgId(id);
            return Json(result);
        }
        #endregion


        #region GetEmployeesByOrgBraDepId
        public async Task<IActionResult> GetEmployeesByOrgBraDepId(int? orgId, [FromQuery] List<int>? branchIds, [FromQuery] List<int>? depIds)
        {
            var result = await _commonService.GetEmployeesByOrgBraDepId(orgId, branchIds, depIds);
            return Json(result);
        }
        #endregion


        #region GetWeekendByOrganization
        [HttpGet]
        public async Task<IActionResult> GetWeekendByOrganization(int id)
        {
            try
            {
                var weekendSettings = await _commonService.GetWeekendByOrganization(id);
                return Json(weekendSettings);
            }
            catch (Exception ex)
            {
                // Log the error (optional) and return error response
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        #endregion


        #region GetWeekDaysByOrganization
        public async Task<IActionResult> GetWeekDaysByOrganization(int id)
        {
            try
            {
                var weekDaysSettings = await _commonService.GetWeekDaysByOrganization(id);
                return Json(weekDaysSettings);
            }
            catch (Exception ex)
            {
                // Log the error (optional) and return error response
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        #endregion

        
        #region GetAllAsync
        [Route("OffDayRoster/GetAllAsync")]
        [HttpGet]
        public async Task<IActionResult> GetAllAsync(
            int pageNumber = 1, 
            int pageSize = 5, 
            string searchTerm = "", 
            string sortColumn = "RosterInHolyDayID", 
            string sortOrder = "desc", 
            int daysToShow = 7,
            DateTime? startDate = null)
        {
            try
            {
                var (data, uniqueDates, pagination) = await _offDayRosterService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder, daysToShow, startDate);

                return Json(new
                {
                    isSuccess = true,
                    data,
                    uniqueDates,
                    pagination
                });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        #endregion
    }
}
