using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.MasterSetup.Genders;
using GCTL.Core.ViewModels.MasterSetup.LeadSource;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.LeadSource;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.MasterSetup
{
    public class LeadSourcesController : BaseController
    {
        #region Services & Repositories
        private readonly ILeadSourceService _leadSourceService;

        public LeadSourcesController(ITranslateService translateService, IUserProfileService userProfileService, ILeadSourceService leadStatusService) : base(translateService, userProfileService)
        {
            _leadSourceService = leadStatusService;
        }

        #endregion
        public IActionResult Index()
        {
            SetSmartPageCode(600100);
            var vm = new GCTL_App.ViewModels.MasterSetup.LeadSource.LeadSourcePageVM();
            return View(vm);
        }

        #region create
        [HttpPost]
        public async Task<IActionResult> Create(LeadSourceVM model)
        {
            SetSmartPageCode(600200);
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _leadSourceService.IsNameUniqueAsync(model.LeadSourceName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This name already exists!" });
                    }
                    await _leadSourceService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.LeadSourceID });
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
        //[Permission("Edit", "Genders")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Update(LeadSourceVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var isUpdated = await _leadSourceService.UpdateAsync(model);
                    if (isUpdated)
                    {
                        return Json(new { isSuccess = true, message = "Updated Successfully." });
                    }
                    else
                    {
                        return Json(new { isSuccess = true, message = "Updated failed. Record not inserted" });
                    }
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


        #region GetAll
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "LeadSourceName", string sortOrder = "asc")
        {
            SetSmartPageCode(600300);
            var result = await _leadSourceService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion

        #region GetById
        public async Task<IActionResult> GetById(int id)
        {
            SetSmartPageCode(600400);
            try
            {
                var result = await _leadSourceService.GetByIdAsync(id);
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

        #region SoftDelete
        //[Permission("Delete", "Genders")]
        [HttpPost]
        public async Task<IActionResult> SoftDelete(DeleteRequestVM requestVM)
        {
            try
            {
                if (requestVM.Ids == null || !requestVM.Ids.Any() || requestVM.Ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No id selected to delete." });
                }
                var result = await _leadSourceService.SoftDeleteAsync(requestVM);
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
