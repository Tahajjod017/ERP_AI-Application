using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveSettings;
using GCTL.Data.Models;
using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveSettings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Client;

namespace GCTL_App.Controllers.AttendanceManagement.LeaveManagements
{
    public class LeaveSettingsController : Controller
    {
        private readonly IGenericRepository<Organization> organization;
        private readonly ILeaveSettingsService leaveSettingsService;
        public LeaveSettingsController(IGenericRepository<Organization> organization, ILeaveSettingsService leaveSettingsService)
        {
            this.organization = organization;
            this.leaveSettingsService = leaveSettingsService;
        }

        public IActionResult Index()
        {
            ViewBag.organizationDD = new SelectList(organization.AllActive(), "OrganizationID", "OrganizationName");
            return View();
        }
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
    }
}
