using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.Finance.BaseAccountVM;
using GCTL.Service.CommonService;
using GCTL.Service.Finance.BaseAccount;
using GCTL.Service.Language;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using GCTL_App.ViewModels.Finance.MasterSetup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.Finance
{
    [Authorize]
    public class MasterSetupController : BaseController
    {
        #region Services
        private readonly IBaseAccountService _baseAccountService;
        private readonly ICommonService _commonService;


        public MasterSetupController(ITranslateService translateService, IUserProfileService userProfileService, IBaseAccountService baseAccountService, ICommonService commonService) : base(translateService, userProfileService)
        {
            _baseAccountService = baseAccountService;
            _commonService = commonService;
        }
        #endregion


        #region Index
        [Permission("View", "MasterSetup")]
        public async Task<IActionResult> Index()
        {
            try
            {
                BaseAccountPageVM model = new BaseAccountPageVM();

                SetSmartPageCode(203600);

                return View(model);
            }
            catch (Exception ex)
            {
                //return Json(new { isSuccess = false, message = ex.Message });
                return RedirectToAction("Index", "Home");
            }
        }
        #endregion


        #region Create
        [Permission("Create", "MasterSetup")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(CreateBaseAccountVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _baseAccountService.IsNameUniqueAsync(model.BaseAccountName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = $"{model.BaseAccountName} already exists!" });
                    }

                    var uniqueCode = await _baseAccountService.IsCodeUniqueAsync(model.BaseAccountCode, model.BaseAccountID);
                    if (!uniqueCode)
                    {
                        return Json(new { isSuccess = false, message = $"{model.BaseAccountCode} already exists!" });
                    }

                    await _baseAccountService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully." });
                }

                var orderedKeys = new[] { "BaseAccountName", "BaseAccountCode" };

                foreach (var key in orderedKeys)
                {
                    if (ModelState.TryGetValue(key, out var entry) && entry.Errors.Any())
                    {
                        return Json(new { isSuccess = false, field = key, message = entry.Errors.First().ErrorMessage });
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


        #region Update
        [Permission("Edit", "MasterSetup")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Update(UpdateBaseAccountVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _baseAccountService.IsNameUniqueAsync(model.BaseAccountName, model.BaseAccountID);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = $"{model.BaseAccountName} already exists!" });
                    }

                    var uniqueCode = await _baseAccountService.IsCodeUniqueAsync(model.BaseAccountCode, model.BaseAccountID);
                    if (!uniqueCode)
                    {
                        return Json(new { isSuccess = false, message = $"{model.BaseAccountCode} already exists!" });
                    }

                    await _baseAccountService.UpdateAsync(model);
                    return Json(new { isSuccess = true, message = "Updated Successfully." });
                }

                var orderedKeys = new[] { "BaseAccountCode", "BaseAccountName" };
                foreach (var key in orderedKeys)
                {
                    if (ModelState.TryGetValue(key, out var entry) && entry.Errors.Any())
                    {
                        return Json(new { isSuccess = false, field = key, message = entry.Errors.First().ErrorMessage });
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
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "BaseAccountID", string sortOrder = "desc")
        {
            var result = await _baseAccountService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion


        #region GetById
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _baseAccountService.GetByIdAsync(id);
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
        [Permission("Delete", "MasterSetup")]
        [HttpDelete]
        public async Task<IActionResult> Delete(DeleteRequestVM requestVM)
        {
            try
            {
                if (requestVM.Ids == null || !requestVM.Ids.Any() || requestVM.Ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No id selected to delete." });
                }
                var result = await _baseAccountService.SoftDeleteAsync(requestVM);
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


        #region CheckNameUnique
        [HttpPost]
        public async Task<IActionResult> CheckNameUnique(string name)
        {
            try
            {
                if (name == null || name == "") 
                    return Json(true);

                bool isUnique = await _baseAccountService.IsNameUniqueAsync(name);
                if (!isUnique)
                {
                    return Json(new { isSuccess = false, message = $"{name} already exists." });
                }
                return Json(true);
            }
            catch (Exception ex)
            {
                return Json("Error occurred: " + ex.Message);
            }
        }
        #endregion


        #region CheckCodeUnique
        [HttpPost]
        public async Task<IActionResult> CheckCodeUnique(string code)
        {
            try
            {
                if (code == null || code == "")
                    return Json(true);

                bool isUnique = await _baseAccountService.IsCodeUniqueAsync(code);
                if (!isUnique)
                {
                    return Json(new { isSuccess = false, message = $"{code} already exists." });
                }
                return Json(true);
            }
            catch (Exception ex)
            {
                return Json("Error occurred: " + ex.Message);
            }
        }
        #endregion
    }
}
