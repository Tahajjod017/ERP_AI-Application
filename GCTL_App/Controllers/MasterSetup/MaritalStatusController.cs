using GCTL.Core.ViewModels.MasterSetup.MaritalStatus;
using GCTL.Core.ViewModels;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.MaritalStatuses;
using GCTL.Service.RolePermissions;
using Microsoft.AspNetCore.Mvc;
using GCTL_App.ViewModels.MasterSetup.MaritalStatus;
using GCTL.Core.Helpers;
using GCTL.Service.UserProfile;

namespace GCTL_App.Controllers.MasterSetup
{
    public class MaritalStatusController : BaseController
    {
        #region Services & Repositories
        private readonly IMaritalStatusService _maritalStatusService;
        private readonly ITranslateService _translationService;


        public MaritalStatusController(IMaritalStatusService maritalStatusService, ITranslateService translationService, ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {
            _maritalStatusService = maritalStatusService;
            _translationService = translationService;
        }
        #endregion


        #region Index
        //[Permission("View", "MaritalStatus")]
        public IActionResult Index()
        {
            MaritalStatusPageVM model = new MaritalStatusPageVM();
            SetSmartPageCode(201500);
            return View(model);
        }
        #endregion


        #region GetById
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _maritalStatusService.GetByIdAsync(id);
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
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "MaritalStatusName", string sortOrder = "asc")
        {
            var result = await _maritalStatusService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion


        #region Create
        //[Permission("Create", "MaritalStatus")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(MaritalStatusVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _maritalStatusService.IsNameUniqueAsync(model.MaritalStatusName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This name already exists!" });
                    }
                    await _maritalStatusService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.MaritalStatusID });
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
        //[Permission("Edit", "MaritalStatus")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Update(MaritalStatusVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _maritalStatusService.UpdateAsync(model);
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
                bool isUnique = await _maritalStatusService.IsNameUniqueAsync(name);
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
        [Permission("Delete", "MaritalStatus")]
        [HttpPost]
        public async Task<IActionResult> SoftDelete(DeleteRequestVM requestVM)
        {
            try
            {
                if (requestVM.Ids == null || !requestVM.Ids.Any() || requestVM.Ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No id selected to delete." });
                }
                var result = await _maritalStatusService.SoftDeleteAsync(requestVM);
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
