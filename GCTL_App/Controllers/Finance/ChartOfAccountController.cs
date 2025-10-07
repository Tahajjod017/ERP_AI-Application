using GCTL.Service.Language;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.Finance
{
    [Authorize]
    public class ChartOfAccountController : BaseController
    {
        public ChartOfAccountController(ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {

        }


        [Permission("View", "ChartOfAccount")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
