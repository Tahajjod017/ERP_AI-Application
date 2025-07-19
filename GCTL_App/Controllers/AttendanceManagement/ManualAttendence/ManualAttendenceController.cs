using GCTL.Service.AttendanceManagement.ManualAttendence;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.AttendanceManagement.ManualAttendence
{
    public class ManualAttendenceController : BaseController
    {
        private readonly IManualAttendenceService _manualAttendenceService;
        public ManualAttendenceController(ITranslateService translateService, IUserProfileService userProfileService, IManualAttendenceService manualAttendenceService) : base(translateService, userProfileService)
        {
            _manualAttendenceService = manualAttendenceService;
        }

        public IActionResult Index()
        {
            SetSmartPageCode(113000);

            return View();
        }
    }
}
