using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.MasterSetup.ServiceType;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.ServiceType;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.MasterSetup
{
    public class ServicesController : BaseController
    {
        #region Services & Repositories
        private readonly IServiceTypeService _serviceTypeService;

        public ServicesController(ITranslateService translateService, IUserProfileService userProfileService, IServiceTypeService serviceTypeService) : base(translateService, userProfileService)
        {
            _serviceTypeService = serviceTypeService;
        }

        #endregion

        #region Index
        [Permission("View", "Services")]
        public IActionResult Index()
        {
            SetSmartPageCode(600100);
            var vm = new GCTL_App.ViewModels.MasterSetup.ServiceType.ServicePageVM();
            return View(vm);
        }
        #endregion

        #region create
        [Permission("Create", "Services")]
        [HttpPost]
        public async Task<IActionResult> Create(ServiceVM model)
        {
            SetSmartPageCode(600200);
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _serviceTypeService.IsNameUniqueAsync(await GetCurrentOrganizationIdAsync() ?? 0, model.ServiceName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This name already exists!" });
                    }
                    await _serviceTypeService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.ServiceID });
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
        [Permission("Edit", "Services")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Update(ServiceVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var isUpdated = await _serviceTypeService.UpdateAsync(model);
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
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "ServiceName", string sortOrder = "asc")
        {
            SetSmartPageCode(600300);
            var result = await _serviceTypeService.GetAllAsync(await GetCurrentOrganizationIdAsync() ?? 0, pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion

        #region GetById
        public async Task<IActionResult> GetById(int id)
        {
            SetSmartPageCode(600400);
            try
            {
                var result = await _serviceTypeService.GetByIdAsync(await GetCurrentOrganizationIdAsync() ?? 0, id);
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
        [Permission("Delete", "Services")]
        [HttpPost]
        public async Task<IActionResult> SoftDelete(DeleteRequestVM requestVM)
        {
            try
            {
                if (requestVM.Ids == null || !requestVM.Ids.Any() || requestVM.Ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No id selected to delete." });
                }
                var result = await _serviceTypeService.SoftDeleteAsync(requestVM);
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
