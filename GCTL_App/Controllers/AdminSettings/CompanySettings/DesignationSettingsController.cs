using GCTL.Service.AdminSettings.OrganizationSettings.DesignationService;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.AdminSettings.CompanySettings
{
    public class DesignationSettingsController : BaseController
    {
        private readonly IDesignationSettingService _designationSettingService;
        public DesignationSettingsController(ITranslateService translateService, IUserProfileService userProfileService, IDesignationSettingService designationSettingService) : base(translateService, userProfileService)
        {
            _designationSettingService = designationSettingService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Organizations = await _designationSettingService.GetOrganizationsAsync();
            return View();
        }
    }
}
