using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.MasterSetup.YearlyEndBonusType;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.YearlyEndBonusType;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using GCTL_App.ViewModels.MasterSetup.YearlyEndBonusType;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.MasterSetup
{
    public class YearlyEndBonusTypeController : BaseController
    {
        #region Services & Repositories
        private readonly IYearlyEndBonusTypeService _yearlyEndBonusTypeService;

        public YearlyEndBonusTypeController(IYearlyEndBonusTypeService yearlyEndBonusTypeService, ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {
            _yearlyEndBonusTypeService = yearlyEndBonusTypeService;
        }
        #endregion


        #region Index
        //[Permission("View", " YearlyEndBonusType")]
        public IActionResult Index()
        {
            YearlyEndBonusTypePageVM model = new YearlyEndBonusTypePageVM();
            SetSmartPageCode(202700);
            return View(model);
        }
        #endregion


        #region Create
        //[Permission("Create", "YearlyEndBonusType")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(YearlyEndBonusTypeVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // userInfoService.SetUserInfo(model, User, HttpContext);
                    var uniqueName = await _yearlyEndBonusTypeService.IsNameUniqueAsync(model.YearlyEndBonusTypeName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This name already exists!" });
                    }

                    await _yearlyEndBonusTypeService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.YearlyEndBonusTypeID });
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
        //[Permission("Edit", "YearlyEndBonusType")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Update(YearlyEndBonusTypeVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // userInfoService.SetUserInfo(model, User, HttpContext);
                    await _yearlyEndBonusTypeService.UpdateAsync(model);
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
                bool isUnique = await _yearlyEndBonusTypeService.IsNameUniqueAsync(name);
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
                var result = await _yearlyEndBonusTypeService.GetByIdAsync(id);
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
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "YearlyEndBonusTypeID", string sortOrder = "desc")
        {
            var result = await _yearlyEndBonusTypeService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

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

                var result = await _yearlyEndBonusTypeService.SoftDeleteAsync(requestVM);
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
