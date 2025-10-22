using GCTL.Core.ViewModels.Finance.PostingRulesVM;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.Finance
{
    public class PostingRulesController : BaseController
    {
        public PostingRulesController(ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                CreatePostingRulesVM model = new CreatePostingRulesVM();
                SetSmartPageCode(203800);
                return View(model);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
