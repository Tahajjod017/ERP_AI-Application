using GCTL.Core.ViewModels.MasterSetup.Grade;
using GCTL.Core.ViewModels;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.Grades;
using GCTL.Service.RolePermissions;
using Microsoft.AspNetCore.Mvc;
using GCTL_App.ViewModels.MasterSetup.Grade;
using GCTL.Core.Helpers;
using GCTL.Service.UserProfile;

namespace GCTL_App.Controllers.MasterSetup
{
    public class GradesController : BaseController
    {
        #region Services & Repositories
        private readonly IGradeService _gradeService;
        private readonly ITranslateService _translationService;


        public GradesController(IGradeService gradeService, ITranslateService translationService, ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {
            _gradeService = gradeService;
            _translationService = translationService;
        }
        #endregion


        #region Index
        //[Permission("View", "Grades")]
        public IActionResult Index()
        {
            GradePageVM model = new GradePageVM();
            SetSmartPageCode(201300);
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


        #region Create
        //[Permission("Create", "Grades")]
        [ValidateAntiForgeryToken]
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


        #region Update
        //[Permission("Edit", "Grades")]
        [ValidateAntiForgeryToken]
        [HttpPost]
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
        public async Task<IActionResult> SoftDelete(DeleteRequestVM requestVM)
        {
            try
            {
                if (requestVM.Ids == null || !requestVM.Ids.Any() || requestVM.Ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No id selected to delete." });
                }
                var result = await _gradeService.SoftDeleteAsync(requestVM);
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
