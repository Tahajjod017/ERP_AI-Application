using GCTL.Core.ViewModels.MasterSetup.EmploymentNature;
using GCTL.Core.ViewModels;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.EmploymentNatures;
using GCTL.Service.RolePermissions;
using Microsoft.AspNetCore.Mvc;
using GCTL_App.ViewModels.MasterSetup.EmploymentNature;
using GCTL.Core.Helpers;

namespace GCTL_App.Controllers.MasterSetup
{
    public class EmploymentNaturesController : BaseController
    {
        #region Services & Repositories
        private readonly IEmploymentNatureService _employmentNatureService;
        private readonly ITranslateService _translationService;


        public EmploymentNaturesController(IEmploymentNatureService employmentNatureService, ITranslateService translationService, ITranslateService translateService) : base(translateService)
        {
            _employmentNatureService = employmentNatureService;
            _translationService = translationService;
        }
        #endregion


        #region Index
        //[Permission("View", "EmploymentNatures")]
        public IActionResult Index()
        {
            EmploymentNaturePageVM model = new EmploymentNaturePageVM();
            SetSmartPageCode(201100);
            return View(model);
        }
        #endregion


        #region GetById
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _employmentNatureService.GetByIdAsync(id);
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
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "EmploymentNatureName", string sortOrder = "asc")
        {
            var result = await _employmentNatureService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion


        #region Create
        //[Permission("Create", "EmploymentNatures")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(EmploymentNatureVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _employmentNatureService.IsNameUniqueAsync(model.EmploymentNatureName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This name already exists!" });
                    }
                    await _employmentNatureService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.EmploymentNatureID });
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
        //[Permission("Edit", "EmploymentNatures")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Update(EmploymentNatureVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _employmentNatureService.UpdateAsync(model);
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
                bool isUnique = await _employmentNatureService.IsNameUniqueAsync(name);
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
        [Permission("Delete", "EmploymentNatures")]
        [HttpPost]
        public async Task<IActionResult> SoftDelete(DeleteRequestVM requestVM)
        {
            try
            {
                if (requestVM.Ids == null || !requestVM.Ids.Any() || requestVM.Ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No id selected to delete." });
                }
                var result = await _employmentNatureService.SoftDeleteAsync(requestVM);
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
