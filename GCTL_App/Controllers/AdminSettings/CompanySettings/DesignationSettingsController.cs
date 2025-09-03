using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup.Designations;
using GCTL.Data.Models;
using GCTL.Service.AdminSettings.OrganizationSettings.DesignationService;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.AdminSettings.CompanySettings
{
    [Authorize]
    public class DesignationSettingsController : BaseController
    {
        private readonly IDesignationSettingService _designationSettingService;
        private readonly IGenericRepository<Organization> _organizationRepository;
        public DesignationSettingsController(ITranslateService translateService, IUserProfileService userProfileService, IDesignationSettingService designationSettingService, IGenericRepository<Organization> organizationRepository) : base(translateService, userProfileService)
        {
            _designationSettingService = designationSettingService;
            _organizationRepository = organizationRepository;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Organizations = await _designationSettingService.GetOrganizationsAsync();
            ViewBag.OrganizationDD = new SelectList(_organizationRepository.AllActive(), "OrganizationID", "OrganizationName");
            return View();
        }
        public async Task<IActionResult> GetDesignations(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "DesignationID", string sortOrder = "desc")
        {
            var result = await _designationSettingService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);
            return Json(result);
        }
        #region
        [HttpPost]
        public async Task<IActionResult> Create(DesignationVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _designationSettingService.IsNameUniqueAsync(model.DesignationName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This name already exists!" });
                    }
                    if (model.DesignationName == null)
                    {
                        return Json(new { isSuccess = false, message = "Designation Name cannot be Empty!" });
                    }
                    await _designationSettingService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.DesignationName });
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

        public async Task<IActionResult> GetById(int id)
        {
            var designation = await _designationSettingService.GetByIdAsync(id);
            if (designation == null)
            {
                return Json(new { isSuccess = false, message = "No record found." });
            }
            return Json(new { isSuccess = true, data = designation });
        }
        #region update
        [HttpPost]
        public async Task<IActionResult> Updates(DesignationVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //var uniqueName = await _designationSettingService.IsNameUniqueAsync(model.DesignationName);

                    //if (!uniqueName)
                    //{
                    //    return Json(new { isSuccess = false, message = "This name already exists!" });
                    //}
                    if (model.OrganizationID == null)
                    {
                        return Json(new { isSuccess = false, message = "Organization Name cannot be Empty!" });
                    }
                    if (model.DesignationName == null)
                    {
                        return Json(new { isSuccess = false, message = "Designation Name cannot be Empty!" });
                    }

                    await _designationSettingService.UpdateAsync(model);
                    return Json(new { isSuccess = true, message = "Updated Successfully." });
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
                var result = await _designationSettingService.SoftDeleteAsync(requestVM);
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
