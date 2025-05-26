using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.MasterSetup.ProvisionPeriodTtimeTypes;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.ProvisionPeriodTimeType;
using GCTL.Service.RolePermissions;
using GCTL_App.ViewModels.MasterSetup.ProvisionPeriodTtimeTypes;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.MasterSetup
{
    public class ProvisionPeriodTimeController : BaseController
    {
        #region Services & Repositories
        private readonly IProvisionPeriodTtimeTypesService _provisionPeriodTtimeTypesService;

        public ProvisionPeriodTimeController(IProvisionPeriodTtimeTypesService provisionPeriodTtimeTypesService, ITranslateService translateService) : base(translateService)
        {
            _provisionPeriodTtimeTypesService = provisionPeriodTtimeTypesService;
        }
        #endregion


        #region Index
        //[Permission("View", " ProvisionPeriodTime")]
        public IActionResult Index()
        {
            ProvisionPeriodTtimeTypesPageVM model = new ProvisionPeriodTtimeTypesPageVM();
            SetSmartPageCode(202100);
            return View(model);
        }
        #endregion


        #region Create
        //[Permission("Create", "ProvisionPeriodTime")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(ProvisionPeriodTtimeTypesVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // userInfoService.SetUserInfo(model, User, HttpContext);
                    var uniqueName = await _provisionPeriodTtimeTypesService.IsNameUniqueAsync(model.ProvisionPeriodTtimeTypeName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This name already exists!" });
                    }

                    await _provisionPeriodTtimeTypesService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.ProvisionPeriodTtimeTypeID });
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
        //[Permission("Edit", "ProvisionPeriodTime")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Update(ProvisionPeriodTtimeTypesVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // userInfoService.SetUserInfo(model, User, HttpContext);
                    await _provisionPeriodTtimeTypesService.UpdateAsync(model);
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
                bool isUnique = await _provisionPeriodTtimeTypesService.IsNameUniqueAsync(name);
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


        #region GetById
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _provisionPeriodTtimeTypesService.GetByIdAsync(id);
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
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "ProvisionPeriodTtimeTypeID", string sortOrder = "desc")
        {
            var result = await _provisionPeriodTtimeTypesService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion


        #region Delete
        [HttpPost]
        public async Task<IActionResult> Delete(DeleteRequestVM requestVM)
        {
            try
            {
                if (requestVM.Ids == null || !requestVM.Ids.Any() || requestVM.Ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No bank selected to delete." });
                }

                var result = await _provisionPeriodTtimeTypesService.SoftDeleteAsync(requestVM);
                if (result == null)
                {
                    return Json(new { isSuccess = false, message = "No banks found to delete." });
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
