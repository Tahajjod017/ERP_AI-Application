using GCTL.Core.ViewModels.MasterSetup.Genders;
using GCTL.Core.ViewModels;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.Gender;
using GCTL.Service.RolePermissions;
using Microsoft.AspNetCore.Mvc;
using GCTL_App.ViewModels.MasterSetup.Genders;

namespace GCTL_App.Controllers.MasterSetup
{
    public class GendersController : Controller
    {
        #region Services & Repositories
        private readonly IUserInfoService _userInfoService;
        private readonly IGenderService _genderService;
        private readonly ITranslateService _translationService;

        public GendersController(IGenderService genderService, IUserInfoService userInfoService, ITranslateService translationService)
        {
            _genderService = genderService;
            _userInfoService = userInfoService;
            _translationService = translationService;
        }
        #endregion


        #region Index
        [Permission("View", "Genders")]
        public IActionResult Index()
        {
            var languageCode = HttpContext.Items["Language"] as string ?? "en";
            int PageCode = 319000; // Unique page code for gender translations

            // Adding translations for all labels
            ViewBag.Title = _translationService.GetTranslationInd("Add Gender", (PageCode++).ToString(), languageCode);
            ViewBag.Save = _translationService.GetTranslationInd("Save", (PageCode++).ToString(), languageCode);
            ViewBag.Reset = _translationService.GetTranslationInd("Reset", (PageCode++).ToString(), languageCode);
            ViewBag.GenderName = _translationService.GetTranslationInd("Gender Name", (PageCode++).ToString(), languageCode);
            ViewBag.AddGender = _translationService.GetTranslationInd("Add Gender", (PageCode++).ToString(), languageCode);
            ViewBag.InformationOfGender = _translationService.GetTranslationInd("Information of Gender", (PageCode++).ToString(), languageCode);
            ViewBag.Showing = _translationService.GetTranslationInd("Showing", (PageCode++).ToString(), languageCode);
            ViewBag.SearchHere = _translationService.GetTranslationInd("Search here", (PageCode++).ToString(), languageCode);
            ViewBag.Delete = _translationService.GetTranslationInd("Delete", (PageCode++).ToString(), languageCode);
            ViewBag.ID = _translationService.GetTranslationInd("ID", (PageCode++).ToString(), languageCode);
            ViewBag.Action = _translationService.GetTranslationInd("Action", (PageCode++).ToString(), languageCode);

            GenderPageVM model = new GenderPageVM();
            return View(model);
        }
        #endregion


        #region GetById
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _genderService.GetByIdAsync(id);
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
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "GenderName", string sortOrder = "asc")
        {
            var result = await _genderService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion


        #region Update
        [Permission("Edit", "Genders")]
        [HttpPost]
        public async Task<IActionResult> Update(GenderVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _genderService.UpdateAsync(model);
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


        #region Create
        [Permission("Create", "Genders")]
        [HttpPost]
        public async Task<IActionResult> Create(GenderVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _genderService.IsNameUniqueAsync(model.GenderName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This name already exists!" });
                    }
                    await _genderService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.GenderID });
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
                bool isUnique = await _genderService.IsNameUniqueAsync(name);
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
        [Permission("Delete", "Genders")]
        [HttpPost]
        public async Task<IActionResult> SoftDelete(BaseViewModel model, List<int> ids)
        {
            try
            {
                if (ids == null || !ids.Any() || ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No id selected to delete." });
                }
                var result = await _genderService.SoftDeleteAsync(model, ids);
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
