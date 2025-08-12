using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Service.AdminSettings.GeneralSettings;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.AdminSettings.CompanySettings
{
    [Authorize]
    public class LocalizationSettingsController : BaseController
    {
        private readonly ILocalizationSettingService _localizationSettingService;
        public LocalizationSettingsController(ITranslateService translateService, IUserProfileService userProfileService, ILocalizationSettingService localizationSettingService) : base(translateService, userProfileService)
        {
            _localizationSettingService = localizationSettingService;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(LocalizationViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Optional: You can add any custom validation if required, like checking if Organization already exists or other checks
                    // For instance, check if the Localization already exists based on certain properties (OrganizationID, LanguageID, etc.)
                    //var uniqueCheck = await _localizationSettingService.IsLocalizationUniqueAsync(model.OrganizationID, model.LanguageID, model.TimezoneID);
                    //if (!uniqueCheck)
                    //{
                    //    return Json(new { isSuccess = false, message = "This Localization already exists!" });
                    //}

                    // Add or update the Localization record based on the provided model
                    var result = await _localizationSettingService.AddAsync(model);

                    // Return success response
                    return Json(new { isSuccess = true, message = "Localization saved successfully.", lastId = model.LocalizationID });
                }

                // Get the first error message if model state is invalid
                var errorMessage = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;

                // Return failure response with the error message
                return Json(new { isSuccess = false, message = errorMessage ?? "Something went wrong." });
            }
            catch (Exception ex)
            {
                // Log exception here if needed

                // Return failure response with exception message
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        #endregion

    }
}
