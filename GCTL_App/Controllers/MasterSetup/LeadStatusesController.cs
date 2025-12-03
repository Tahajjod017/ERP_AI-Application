using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.MasterSetup.Genders;
using GCTL.Core.ViewModels.MasterSetup.LeadStatuses;
using GCTL.Core.ViewModels.MasterSetup.ServiceType;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.Gender;
using GCTL.Service.MasterSetup.LeadStatuses;
using GCTL.Service.MasterSetup.ServiceType;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.MasterSetup
{
    public class LeadStatusesController : BaseController
    {
        #region Services & Repositories
        private readonly ILeadStatusService _leadStatusService;

        public LeadStatusesController(ITranslateService translateService, IUserProfileService userProfileService, ILeadStatusService leadStatusService) : base(translateService, userProfileService)
        {
            _leadStatusService = leadStatusService;
        }

        #endregion

        #region Index
        [Permission("View", "LeadStatuses")]
        public IActionResult Index()
        {
            SetSmartPageCode(600100);
            var vm = new GCTL_App.ViewModels.MasterSetup.LeadStatuses.LeadStatusPageVM();
            return View(vm);
        }
        #endregion

        #region create
        [Permission("Create", "LeadStatuses")]
        [HttpPost]
        public async Task<IActionResult> Create(LeadStatusVM model)
        {
            SetSmartPageCode(600200);
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _leadStatusService.IsNameUniqueAsync(await GetCurrentOrganizationIdAsync() ?? 0, model.LeadStatusName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This name already exists!" });
                    }
                    await _leadStatusService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.LeadStatusID });
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
        [Permission("Edit", "LeadStatuses")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Update(LeadStatusVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var isUpdated = await _leadStatusService.UpdateAsync(model);
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
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "LeadStatusName", string sortOrder = "asc")
        {
            SetSmartPageCode(600300);
            var result = await _leadStatusService.GetAllAsync(await GetCurrentOrganizationIdAsync() ?? 0, pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion

        #region GetById
        public async Task<IActionResult> GetById(int id)
        {
            SetSmartPageCode(600400);
            try
            {
                var result = await _leadStatusService.GetByIdAsync(await GetCurrentOrganizationIdAsync() ?? 0, id);
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
        [Permission("Delete", "LeadStatuses")]
        [HttpPost]
        public async Task<IActionResult> SoftDelete(DeleteRequestVM requestVM)
        {
            try
            {
                if (requestVM.Ids == null || !requestVM.Ids.Any() || requestVM.Ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No id selected to delete." });
                }
                var result = await _leadStatusService.SoftDeleteAsync(requestVM);
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
