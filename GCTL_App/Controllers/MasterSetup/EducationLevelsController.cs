using GCTL.Core.ViewModels.MasterSetup.EducationLevels;
using GCTL.Core.ViewModels;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.EducationLevel;
using GCTL.Service.RolePermissions;
using Microsoft.AspNetCore.Mvc;
using GCTL_App.ViewModels.MasterSetup.EducationLevels;

namespace GCTL_App.Controllers.MasterSetup
{
    public class EducationLevelsController : BaseController
    {
        #region Services & Repositories
        private readonly IEducationLevelsService _educationLevelsService;
        private readonly ITranslateService _translationService;


        public EducationLevelsController(IEducationLevelsService educationLevelsService, ITranslateService translationService, ITranslateService translateService) : base(translateService)
        {
            _educationLevelsService = educationLevelsService;
            _translationService = translationService;
        }
        #endregion


        #region Index
        //[Permission("View", "EducationLevels")]
        public IActionResult Index()
        {
            EducationLevelPageVM model = new EducationLevelPageVM();
            SetSmartPageCode(208000);
            return View(model);
        }
        #endregion


        #region GetById
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _educationLevelsService.GetByIdAsync(id);
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
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "EducationLevelName", string sortOrder = "asc")
        {
            var result = await _educationLevelsService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion


        #region Create
        //[Permission("Create", "EducationLevels")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(EducationLevelVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _educationLevelsService.IsNameUniqueAsync(model.EducationLevelName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This name already exists!" });
                    }
                    await _educationLevelsService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.EducationLevelID });
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
        //[Permission("Edit", "EducationLevels")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Update(EducationLevelVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _educationLevelsService.UpdateAsync(model);
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
                bool isUnique = await _educationLevelsService.IsNameUniqueAsync(name);
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
        [Permission("Delete", "EducationLevels")]
        [HttpPost]
        public async Task<IActionResult> SoftDelete(BaseViewModel model, List<int> ids)
        {
            try
            {
                if (ids == null || !ids.Any() || ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No id selected to delete." });
                }
                var result = await _educationLevelsService.SoftDeleteAsync(model, ids);
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
