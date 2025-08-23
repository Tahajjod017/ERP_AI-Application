using GCTL.Core.Repository;
using GCTL.Data.Models;
using GCTL.Service.AdminSettings.SystemSettings.ISmsSettingService;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.AdminSettings
{
    public class SMSsettingController : BaseController
    {
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly ISmsSettingsService _smsSettingsService;
        public SMSsettingController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<Organization> organizationRepository, ISmsSettingsService smsSettingsService) : base(translateService, userProfileService)
        {
            _organizationRepository = organizationRepository;
            _smsSettingsService = smsSettingsService;
        }

        public IActionResult Index()
        {
            ViewBag.OrganizationDD = new SelectList(_organizationRepository.AllActive(), "OrganizationID", "OrganizationName");
            return View();
        }
    }
}
