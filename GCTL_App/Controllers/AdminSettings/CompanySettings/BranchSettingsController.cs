using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Service.AdminSettings.OrganizationSettings.BranchService;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.AdminSettings.CompanySettings
{
    [Authorize]
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

        #region edit
        public async Task<IActionResult> GetById(int id)
        {
            var branch = await _branchSettingService.GetByIdAsync(id);
            if (branch == null)
            {
                return Json(new { isSuccess = false, message = "No record found against this id." });
            }
            return Json(new { isSuccess = true, data = branch });
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Updates(BranchSettingsVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _branchSettingService.IsNameUniqueAsync(model.OrganizationBranchName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This name already exists!" });
                    }
                    await _branchSettingService.UpdateAsync(model);
                    return Json(new { isSuccess = true, message = "Updated Successfully.", lastId = model.OrganizationBranchName });
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
        #region delete 

        [HttpPost]
        public async Task<IActionResult> SoftDelete(DeleteRequestVM requestVM)
        {
            try
            {
                if (requestVM.Ids == null || !requestVM.Ids.Any() || requestVM.Ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No id selected to delete." });
                }
                var result = await _branchSettingService.SoftDeleteAsync(requestVM);
                if (result == null)
                {
                    return Json(new { isSuccess = false, message = "No id found to delete." });
                }

                return Json(new { isSuccess = true, message = "Deleted Successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        #endregion

    }
}
