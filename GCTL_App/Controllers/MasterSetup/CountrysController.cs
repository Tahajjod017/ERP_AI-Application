using GCTL.Core.ViewModels.MasterSetup.Country;
using GCTL.Core.ViewModels;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.Countries;
using GCTL.Service.RolePermissions;
using Microsoft.AspNetCore.Mvc;
using GCTL_App.ViewModels.MasterSetup.Country;

namespace GCTL_App.Controllers.MasterSetup
{
    public class CountrysController : BaseController
    {
        #region Services & Repositories
        private readonly ICountryService _countryService;
        private readonly ITranslateService _translationService;


        public CountrysController(ICountryService countryService, ITranslateService translationService, ITranslateService translateService) : base(translateService)
        {
            _countryService = countryService;
            _translationService = translationService;
        }
        #endregion


        #region Index
        //[Permission("View", "Countrys")]
        public IActionResult Index()
        {
            CountryPageVM model = new CountryPageVM();

            SetSmartPageCode(202000);

            return View(model);
        }
        #endregion


        #region GetById
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _countryService.GetByIdAsync(id);
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
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "CountryID", string sortOrder = "desc")
        {
            var result = await _countryService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion


        #region Create
        //[Permission("Create", "Countrys")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(CountryVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _countryService.IsNameUniqueAsync(model.CountryName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This name already exists!" });
                    }
                    await _countryService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.CountryID });
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
        //[Permission("Edit", "Countrys")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Update(CountryVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _countryService.UpdateAsync(model);
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
                bool isUnique = await _countryService.IsNameUniqueAsync(name);
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
        [Permission("Delete", "Countrys")]
        [HttpPost]
        public async Task<IActionResult> SoftDelete(BaseViewModel model, List<int> ids)
        {
            try
            {
                if (ids == null || !ids.Any() || ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No id selected to delete." });
                }
                var result = await _countryService.SoftDeleteAsync(model, ids);
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
