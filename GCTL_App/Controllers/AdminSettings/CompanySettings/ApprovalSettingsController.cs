using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.AdminSettings.CompanySettings
{
    public class ApprovalSettingsController : BaseController
    {
        public ApprovalSettingsController(ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {
        }

        public IActionResult Index()
        {
            //ViewBag.Organizations = await  _holidaySettingService.GetOrganizationsAsync();
            return View();
        }
    }
}
