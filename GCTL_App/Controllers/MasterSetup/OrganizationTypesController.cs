using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.MasterSetup.OrganizationTypes;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.CompanyTypes;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using GCTL_App.ViewModels.MasterSetup.OrganizationTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.MasterSetup
{
    public class OrganizationTypesController : BaseController
    {
        #region Services & Repositories
        private readonly IOrganizationTypeService _organizationTypeService;
        private readonly ITranslateService _translationService;

        public OrganizationTypesController(IOrganizationTypeService organizationTypeService, ITranslateService translationService, ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {
            _organizationTypeService = organizationTypeService;
            _translationService = translationService;
        }
        #endregion


        #region Index
        [Permission("View", "OrganizationTypes")]
        public IActionResult Index()
        {
            OrganizationTypePageVM model = new OrganizationTypePageVM();
            SetSmartPageCode(201200);
            var organizationTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "Organization", Text = "Organization" },
                new SelectListItem { Value = "Branch", Text = "Branch" }
            };

            // Step 2: Send it to the View using ViewBag (or ViewData)
            ViewBag.OrganizationTypes = organizationTypes;
            return View(model);
        }
        #endregion

        #region GetById
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _organizationTypeService.GetByIdAsync(await GetCurrentOrganizationIdAsync() ?? 0, id);
                if (result == null)
                {
                    return Json(new { isSuccess = false, message = "No data found!" });
                }

                return Json(new { isSuccess = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        #endregion

        #region GetAll
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "CreatedAt", string sortOrder = "asc")
        {
            var result = await _organizationTypeService.GetAllAsync(await GetCurrentOrganizationIdAsync() ?? 0, pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion

        #region Create
        [Permission("Create", "OrganizationTypes")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(OrganizationTypeVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _organizationTypeService.IsNameUniqueAsync(await GetCurrentOrganizationIdAsync() ?? 0, model.TypeName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This name already exists!" });
                    }
                    await _organizationTypeService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.Id });
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

        #region Update
        [Permission("Edit", "OrganizationTypes")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Update(OrganizationTypeVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _organizationTypeService.UpdateAsync(model);
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

        #region CheckNameUnique
        [HttpPost]
        public async Task<IActionResult> CheckNameUnique(string name)
        {
            try
            {
                bool isUnique = await _organizationTypeService.IsNameUniqueAsync(await GetCurrentOrganizationIdAsync() ?? 0, name);
                if (!isUnique)
                {
                    return Json("This name already exists.");
                }
                return Json(true);
            }
            catch (Exception ex)
            {
                return Json("Error occurred: " + ex.Message);
            }
        }
        #endregion

        #region SoftDelete
        [Permission("Delete", "OrganizationTypes")]
        [HttpPost]
        public async Task<IActionResult> SoftDelete(DeleteRequestVM requestVM)
        {
            try
            {
                if (requestVM.Ids == null || !requestVM.Ids.Any() || requestVM.Ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No id selected to delete." });
                }
                var result = await _organizationTypeService.SoftDeleteAsync(requestVM);
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
