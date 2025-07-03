using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.HRMsettings
{
    public class ProbationPeriodSettingController : BaseController
    {
        public ProbationPeriodSettingController(ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {

        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
