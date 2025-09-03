using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AdminSettingsVM;
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

        [HttpPost]
        public async Task<IActionResult> Save(SmsSettingsVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (model.OrganizationID == null)
                    {
                        return Json(new { isSuccess = false, message = "Organization Name cannot be Empty!" });
                    }
                    if (model.ServerName == null)
                    {
                        return Json(new { isSuccess = false, message = "ServerName Name cannot be Empty!" });
                    }

                    var uniqueName = await _smsSettingsService.IsNameUniqueAsync(model.ServerName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This ServerName already exists!" });
                    }


                    await _smsSettingsService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully." });
                }
                var errorMessage = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;

                return Json(new { isSuccess = false, message = errorMessage ?? "Something went wrong." });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
    }
}
