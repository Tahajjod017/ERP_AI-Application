using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.AssignDefaultShift;
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
                        return Json(new { isSuccess = false, message = "Please choose a Company!" });
                    }

                    if (model.ShiftID == null)
                    {
                        return Json(new { isSuccess = false, message = "Please choose a shift!" });
                    }

                    if(model.StartDate == null)
                    {
                        return Json(new { isSuccess = false, message = "Please select start date!" });
                    }

                    if(model.EndDate == null)
                    {
                        return Json(new { isSuccess = false, message = "Please select end date!" });
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
    }
}
