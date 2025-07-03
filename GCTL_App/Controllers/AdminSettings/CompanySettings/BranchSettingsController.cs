using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Service.AdminSettings.OrganizationSettings.BranchService;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.AdminSettings.CompanySettings
{
    public class BranchSettingsController : BaseController
    {
        private readonly IBranchSettingService _branchSettingService;
        public BranchSettingsController(ITranslateService translateService, IUserProfileService userProfileService, IBranchSettingService branchSettingService) : base(translateService, userProfileService)
        {
            _branchSettingService = branchSettingService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.CountriesDropDown = await _branchSettingService.GetCountriesAsync();
            ViewBag.OrganizationsDropDown = await _branchSettingService.GetOrganizationsAsync();
            return View();
        }

        #region create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(BranchSettingsVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                   


                    var uniqueName = await _branchSettingService.IsNameUniqueAsync(model.OrganizationBranchName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This approvalType already exists!" });
                    }
                    await _branchSettingService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.OrganizationBranchName });
                }
                var errorMessage = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;

                return Json(new { isSuccess = false, message = errorMessage ?? "Something went wrong." });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }

        #endregion

        #region Table

        public async Task<IActionResult> GetAllData(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "OrganizationID", string sortOrder = "desc", int? organizationID = null)
        {
            var result = await _branchSettingService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder, organizationID);
            return Json(result);
        }
        #endregion
    }
}
