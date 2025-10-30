using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.MasterSetup.LeadSource;
using GCTL.Core.ViewModels.MasterSetup.Priority;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.LeadSource;
using GCTL.Service.MasterSetup.Priority;
using GCTL.Service.UserProfile;
using GCTL_App.ViewModels.MasterSetup.Priority;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.MasterSetup
{
    public class PrioritiesController : BaseController
    {
        #region Services & Repositories
        private readonly IPriorityService _priorityService;

        public PrioritiesController(ITranslateService translateService, IUserProfileService userProfileService, IPriorityService priorityService) : base(translateService, userProfileService)
        {
            _priorityService = priorityService;
        }

        #endregion
        public IActionResult Index()
        {
            SetSmartPageCode(600500);
            var vm = new PriorityPageVM();
            return View(vm);
        }

        #region create
        [HttpPost]
        public async Task<IActionResult> Create(PriorityVM model)
        {
            SetSmartPageCode(600200);
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _priorityService.IsNameUniqueAsync(await GetCurrentOrganizationIdAsync() ?? 0, model.PriorityName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This name already exists!" });
                    }
                    await _priorityService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.PriorityID });
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
        public async Task<IActionResult> Update(PriorityVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var isUpdated = await _priorityService.UpdateAsync(model);
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
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "PriorityName", string sortOrder = "asc")
        {
            SetSmartPageCode(600300);
            var result = await _priorityService.GetAllAsync(await GetCurrentOrganizationIdAsync() ?? 0, pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion

        #region GetById
        public async Task<IActionResult> GetById(int id)
        {
            SetSmartPageCode(600400);
            try
            {
                var result = await _priorityService.GetByIdAsync(await GetCurrentOrganizationIdAsync() ?? 0, id);
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
                var result = await _priorityService.SoftDeleteAsync(requestVM);
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
