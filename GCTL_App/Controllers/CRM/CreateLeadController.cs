using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.CRM
{
    public class CreateLeadController : BaseController
    {
        public CreateLeadController(ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {
        }

        public IActionResult index()
        {
            SetSmartPageCode(600100);
            return View();
        }
    }
}
