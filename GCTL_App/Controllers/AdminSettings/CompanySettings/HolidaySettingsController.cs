using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.AdminSettings.CompanySettings
{
    public class HolidaySettingsController : BaseController
    {
        public HolidaySettingsController(ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
