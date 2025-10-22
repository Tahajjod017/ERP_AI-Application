using GCTL.Service.CommonService;
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
        private readonly ICommonService _commonService;


        public ChartOfAccountController(ITranslateService translateService, IUserProfileService userProfileService, ICommonService commonService) : base(translateService, userProfileService)
        {
            _commonService = commonService;
        }


        [Permission("View", "ChartOfAccount")]
        public async Task<IActionResult> Index()
        {
            try
            {
                ViewBag.BodyTabs = await _commonService.GetFinanceBodyTabsAsync();
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
