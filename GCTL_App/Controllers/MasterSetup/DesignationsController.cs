using GCTL.Core.ViewModels.MasterSetup.Designations;
using GCTL.Core.ViewModels;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.Designation;
using GCTL.Service.RolePermissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GCTL_App.ViewModels.MasterSetup.Designations;

namespace GCTL_App.Controllers.MasterSetup
{
    public class DesignationsController : BaseController
    {
        #region Services & Repositories
        private readonly IDesignationService _designationService;
        private readonly ITranslateService _translationService;


        public DesignationsController(IDesignationService designationService, ITranslateService translationService, ITranslateService translateService) : base(translateService)
        {
            _designationService = designationService;
            _translationService = translationService;
        }
        #endregion


        #region Index
        //[Permission("View", "Designations")]
        public IActionResult Index()
        {
            DesignationPageVM model = new DesignationPageVM();
            SetSmartPageCode(206000);
            ViewBag.DepartmentDD = new SelectList(_designationService.GetDepartments(), "Id", "Name");
            return View(model);
        }
        #endregion


        #region GetById
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _designationService.GetByIdAsync(id);
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
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "DesignationID", string sortOrder = "desc")
        {
            var result = await _designationService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion


        #region Create
        //[Permission("Create", "Designations")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(DesignationVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _designationService.IsNameUniqueAsync(model.DesignationName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This name already exists!" });
                    }
                    await _designationService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.DesignationID });
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
        //[Permission("Edit", "Designations")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Update(DesignationVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _designationService.UpdateAsync(model);
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
                bool isUnique = await _designationService.IsNameUniqueAsync(name);
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
        [Permission("Delete", "Designations")]
        [HttpPost]
        public async Task<IActionResult> SoftDelete(BaseViewModel model, List<int> ids)
        {
            try
            {
                if (ids == null || !ids.Any() || ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No id selected to delete." });
                }
                var result = await _designationService.SoftDeleteAsync(model, ids);
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
