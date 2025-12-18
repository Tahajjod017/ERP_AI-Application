using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.FieldServices
{
    public class AdvancedApprovalController : BaseController
    {
        public AdvancedApprovalController(ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
