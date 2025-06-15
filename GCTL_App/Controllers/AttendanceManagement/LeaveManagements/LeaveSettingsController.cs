using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveSettings;
using GCTL.Data.Models;
using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveSettings;
using GCTL.Service.Language;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Client;

namespace GCTL_App.Controllers.AttendanceManagement.LeaveManagements
{
    public class LeaveSettingsController : BaseController
    {
        private readonly IGenericRepository<Organization> organization;
        private readonly ILeaveSettingsService leaveSettingsService;

        public LeaveSettingsController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<Organization> organization, ILeaveSettingsService leaveSettingsService) : base(translateService, userProfileService)
        {
            this.organization = organization;
            this.leaveSettingsService = leaveSettingsService;
        }

        public IActionResult Index()
        {
            ViewBag.organizationDD = new SelectList(organization.AllActive(), "OrganizationID", "OrganizationName");
            return View();
        }
        #region  Add and Update Data Leave 
        public async Task<IActionResult> AddNewLeave(AddNewLeaveSave entityVM)
        {
            if (!ModelState.IsValid)
            {

                var errorMessages = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Ok(new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Validation failed.",
                    Errors = errorMessages
                });
            }
            var data = await leaveSettingsService.SaveAddNewLeaveAsync(entityVM);
            return Json(data);
        }

        public async Task<IActionResult> UpdateLeave([FromBody] UpdateLeaveVM entityVM)
        {
            if (!ModelState.IsValid)
            {

                var errorMessages = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Ok(new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Validation failed.",
                    Errors = errorMessages
                });
            }
            var data = await leaveSettingsService.UpdateLeaveAsynce(entityVM);
            return Json(data);
        }

        #endregion

        #region  Detele Data Leave 
        //[Permission("Delete", "BloodGroups")]
        [Route("LeaveDeleteRoute/LeaveDelete")]
        [HttpPost]
        public async Task<IActionResult> LeaveDelete(DeleteRequestVM requestVM)
        {
            try
            {
                if (requestVM.Ids == null || !requestVM.Ids.Any() || requestVM.Ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No id selected to delete." });
                }
                var result = await leaveSettingsService.SoftDeleteAsync(requestVM);
                if (result == null)
                {
                    return Json(new { isSuccess = false, message = "No id found to delete." });
                }

                return Json(new { isSuccess = true, message = "Deleted Successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        #endregion

        #region Get All and By Id Data
        [Route("LeaveSettingsRoute/GetLeaveTypesDataByID")]
        public async Task<IActionResult> GetLeaveTypesDataByID(int leaveTypeID)
        {
            if (leaveTypeID == 0)
            {
                return Json(new { message = "Data is Invalid" });
            }
            var data = await leaveSettingsService.GetLeaveTypesByIdAsync(leaveTypeID);
            return Json(data);
        }

        [Route("LeaveSettingsRoute/GetAllLeaveTypesAsync")]
        public async Task<IActionResult> GetAllLeaveTypesAsync()
        {

            var data = await leaveSettingsService.GetAllLeaveTypesAsync();
            return Json(data);
        }
        #endregion]

        #region LeavePolicyConfig
        public async Task<IActionResult> LeavePolicyConfig(AddLeavePolicyConfigarationVM entityVM)
        {
            if (!ModelState.IsValid)
            {

                var errorMessages = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Ok(new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Validation failed.",
                    Errors = errorMessages
                });
            }
            var data = await leaveSettingsService.AddLeavepolicyAsync(entityVM);
            return Json(data);
        }
        #endregion
    }
}
