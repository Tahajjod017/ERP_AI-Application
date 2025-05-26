using GCTL.Core.ViewModels.MasterSetup.Religions;
using GCTL.Core.ViewModels;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.Religion;
using GCTL.Service.RolePermissions;
using Microsoft.AspNetCore.Mvc;
using GCTL_App.ViewModels.MasterSetup.Religions;
using GCTL.Core.Helpers;

namespace GCTL_App.Controllers.MasterSetup
{
    public class ReligionsController : BaseController
    {
        #region Services & Repositories
        private readonly IReligionService _religionService;
        private readonly ITranslateService _translationService;


        public ReligionsController(IReligionService religionService, ITranslateService translationService, ITranslateService translateService) : base(translateService)
        {
            _religionService = religionService;
            _translationService = translationService;
        }
        #endregion


        #region Index
        //[Permission("View", "Religions")]
        public IActionResult Index()
        {
            ReligionPageVM model = new ReligionPageVM();
            SetSmartPageCode(202200);
            return View(model);
        }
        #endregion


        #region GetById
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _religionService.GetByIdAsync(id);
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
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "ReligionID", string sortOrder = "desc")
        {
            var result = await _religionService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion


        #region Create
        //[Permission("Create", "Religions")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(ReligionVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _religionService.IsNameUniqueAsync(model.ReligionName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This name already exists!" });
                    }
                    await _religionService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.ReligionID });
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
        //[Permission("Edit", "Religions")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Update(ReligionVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _religionService.UpdateAsync(model);
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
                bool isUnique = await _religionService.IsNameUniqueAsync(name);
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
        [Permission("Delete", "Religions")]
        [HttpPost]
        public async Task<IActionResult> SoftDelete(DeleteRequestVM requestVM)
        {
            try
            {
                if (requestVM.Ids == null || !requestVM.Ids.Any() || requestVM.Ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No id selected to delete." });
                }
                var result = await _religionService.SoftDeleteAsync(requestVM);
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
