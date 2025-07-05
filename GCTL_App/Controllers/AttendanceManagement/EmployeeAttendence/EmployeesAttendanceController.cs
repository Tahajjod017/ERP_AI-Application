using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.AttendanceManagement.EmployeeAttendence
{
    public class EmployeesAttendanceController : BaseController
    {
        public EmployeesAttendanceController(ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
