using GCTL.Core.ViewModels.MasterSetup.Grade;
using GCTL.Core.ViewModels;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.Grades;
using GCTL.Service.RolePermissions;
using Microsoft.AspNetCore.Mvc;
using GCTL_App.ViewModels.MasterSetup.Grade;

namespace GCTL_App.Controllers.MasterSetup
{
    public class GradesController : Controller
    {
        #region Services & Repositories
        private readonly IUserInfoService _userInfoService;
        private readonly IGradeService _gradeService;
        private readonly ITranslateService _translationService;


        public GradesController(IGradeService gradeService, IUserInfoService userInfoService, ITranslateService translationService)
        {
            _gradeService = gradeService;
            _userInfoService = userInfoService;
            _translationService = translationService;
        }
        #endregion


        #region Index
        [Permission("View", "Grades")]
        public IActionResult Index()
        {
            var languageCode = HttpContext.Items["Language"] as string ?? "en";
            int PageCode = 319100; // Unique page code for grade translations

            // Adding translations for all labels
            ViewBag.Title = _translationService.GetTranslationInd("Add Grade", (PageCode++).ToString(), languageCode);
            ViewBag.Save = _translationService.GetTranslationInd("Save", (PageCode++).ToString(), languageCode);
            ViewBag.Reset = _translationService.GetTranslationInd("Reset", (PageCode++).ToString(), languageCode);
            ViewBag.GradeName = _translationService.GetTranslationInd("Grade Name", (PageCode++).ToString(), languageCode);
            ViewBag.AddGrade = _translationService.GetTranslationInd("Add Grade", (PageCode++).ToString(), languageCode);
            ViewBag.InformationOfGrades = _translationService.GetTranslationInd("Information of Grades", (PageCode++).ToString(), languageCode);
            ViewBag.Showing = _translationService.GetTranslationInd("Showing", (PageCode++).ToString(), languageCode);
            ViewBag.SearchHere = _translationService.GetTranslationInd("Search here", (PageCode++).ToString(), languageCode);
            ViewBag.Delete = _translationService.GetTranslationInd("Delete", (PageCode++).ToString(), languageCode);
            ViewBag.ID = _translationService.GetTranslationInd("ID", (PageCode++).ToString(), languageCode);
            ViewBag.Action = _translationService.GetTranslationInd("Action", (PageCode++).ToString(), languageCode);

            GradePageVM model = new GradePageVM();
            return View(model);
        }
        #endregion


        #region GetById
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _gradeService.GetByIdAsync(id);
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
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "GradeName", string sortOrder = "asc")
        {
            var result = await _gradeService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion


        #region Update
        [Permission("Edit", "Grades")]
        public async Task<IActionResult> Update(GradeVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _gradeService.UpdateAsync(model);
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
        [Permission("Create", "Grades")]
        [HttpPost]
        public async Task<IActionResult> Create(GradeVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _gradeService.IsNameUniqueAsync(model.GradeName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This name already exists!" });
                    }
                    await _gradeService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.GradeID });
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
        public async Task<IActionResult> CheckNameUnique(string bankName)
        {
            try
            {
                bool isUnique = await _gradeService.IsNameUniqueAsync(bankName);
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
        [Permission("Delete", "Grades")]
        [HttpPost]
        public async Task<IActionResult> SoftDelete(BaseViewModel model, List<int> ids)
        {
            try
            {
                if (ids == null || !ids.Any() || ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No id selected to delete." });
                }
                var result = await _gradeService.SoftDeleteAsync(model, ids);
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
