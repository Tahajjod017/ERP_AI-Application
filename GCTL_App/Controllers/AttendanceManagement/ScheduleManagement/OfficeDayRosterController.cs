using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.OfficeDayRoster;
using GCTL.Service.AttendanceManagement.ScheduleManagement.OfficeDayRoster;
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
        private readonly IOfficeDayRosterService _assignDefaultShiftService;

        public OfficeDayRosterController(ITranslateService translateService, 
            IUserProfileService userProfileService, 
            IOfficeDayRosterService assignDefaultShiftService) : base(translateService, userProfileService)
        {
            _assignDefaultShiftService = assignDefaultShiftService;
        }


        #region Index
        public async Task<IActionResult> Index()
        {
            RosterInOfficeDaysPageVM model = new RosterInOfficeDaysPageVM();
            SetSmartPageCode(203100);

            ViewBag.OrganizationDD = new SelectList(await _assignDefaultShiftService.GetCompanies(), "Id", "Name");
            ViewBag.DepartmentDD = new SelectList(await _assignDefaultShiftService.GetDepartments(), "Id", "Name");
            ViewBag.ShiftDD = new SelectList(await _assignDefaultShiftService.GetShift(), "Id", "Name");
            ViewBag.EmployeeList = await _assignDefaultShiftService.GetGroupedEmployees();

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
                    if (model.OrganizationID == null)
                    {
                        return Json(new { isSuccess = false, message = "Please choose an Organization!" });
                    }

                    if (model.ShiftID == null)
                    {
                        return Json(new { isSuccess = false, message = "Please choose a shift!" });
                    }

                    if(model.StartDate == null && model.EndDate == null)
                    {
                        return Json(new { isSuccess = false, message = "Please select start date & end date!" });
                    }

                    //var hasData = 

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
        public async Task<IActionResult> GetAll(int daysToShow = 7)
        {
            var result = await _assignDefaultShiftService.GetAllAsync(daysToShow);

            var startDate = DateTime.Today;
            var dateList = Enumerable.Range(0, daysToShow).Select(offset => startDate.AddDays(offset)).ToList();

            return Json(new
            {
                result,
                headers = dateList.Select(date => new
                {
                    day = date.ToString("ddd"),
                    date = date.ToString("dd MMM yyyy")
                }).ToList()
            });

            //return Json(result);
        }

        public async Task<IActionResult> GetAllPaging(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "RosterInOfficeDayID", string sortOrder = "desc", int daysToShow = 7)
        {
            var result = await _assignDefaultShiftService.GetAllPaging(pageNumber, pageSize, searchTerm, sortColumn, sortOrder, daysToShow);

            var startDate = DateTime.Today;
            var dateList = Enumerable.Range(0, daysToShow).Select(offset => startDate.AddDays(offset)).ToList();

            return Json(new
            {
                result,
                headers = dateList.Select(date => new
                {
                    day = date.ToString("ddd"),
                    date = date.ToString("dd MMM yyyy")
                }).ToList()
            });

            //return Json(result);
        }
        #endregion


        #region GetDepartmentByOrganization
        public async Task<IActionResult> GetDepartmentByOrganization(int? id)
        {
            var result = await _assignDefaultShiftService.GetDepartmentByOrganization(id);
            return Json(result);
        }
        #endregion


        #region GetEmployeeByOrganization
        public async Task<IActionResult> GetEmployeeByOrganization(int? id)
        {
            var result = await _assignDefaultShiftService.GetEmployeeByOrganization(id);
            return Json(result);
        }
        #endregion


        #region GetShiftByOrganization
        public async Task<IActionResult> GetShiftByOrganization(int? id)
        {
            var result = await _assignDefaultShiftService.GetShiftByOrganization(id);
            return Json(result);
        }
        #endregion


        #region GetEmployeeByDepartment
        public async Task<IActionResult> GetEmployeeByDepartment(int? orgId, [FromQuery] List<int> depIds)
        {
            var result = await _assignDefaultShiftService.GetEmployeeByDepartment(orgId, depIds);
            return Json(result);
        }

        #endregion
    }
}
