using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GCTL_App.Controllers.AttendanceManagement.EmployeeAttendence
{
    public class EmployeesAttendanceController : BaseController
    {

        public EmployeesAttendanceController(ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {
        }

        public async Task<IActionResult> Index()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }
            // user profile
            SetUserProfile();

            return View();
        }
    }
}
