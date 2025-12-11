using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.POS.Purchasess
{
    [Authorize]
    public class PurchaseApprovalController : BaseController
    {
        public PurchaseApprovalController(ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
