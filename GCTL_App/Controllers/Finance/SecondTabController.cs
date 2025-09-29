using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.Finance.SecondTabVM;
using GCTL.Service.CommonService;
using GCTL.Service.Finance.SecondTab;
using GCTL.Service.Language;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using GCTL_App.ViewModels.Finance.MasterSetup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.Finance
{
    //[Authorize]
    public class SecondTabController : BaseController
    {
        #region Services
        private readonly ISecondTabService _secondTabService;
        private readonly ICommonService _commonService;


        public SecondTabController(ITranslateService translateService, IUserProfileService userProfileService, ISecondTabService baseAccountService, ICommonService commonService) : base(translateService, userProfileService)
        {
            _secondTabService = baseAccountService;
            _commonService = commonService;
        }
        #endregion


        #region Index
        //[Permission("View", "SecondTab")]
        public async Task<IActionResult> Index()
        {
            try
            {
                SecondTabPageVM model = new SecondTabPageVM();

                SetSmartPageCode(203700);

                var baseAccounts = await _commonService.GetBaseAccounts();
                if (baseAccounts.Count == 1)
                {
                    model.Create.BaseAccountID = (int)baseAccounts[0].Id;
                }
                ViewBag.BaseAccountDD = new SelectList(baseAccounts, "Id", "Name", model.Create.BaseAccountID);

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
        //[Permission("Create", "SecondTab")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(CreateSecondTabVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _secondTabService.IsNameUniqueAsync(model.ClassName, (int)model.BaseAccountID);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = $"{model.ClassName} already exists!" });
                    }

                    var uniqueCode = await _secondTabService.IsCodeUniqueAsync(model.ClassCode, model.ClassID);
                    if (!uniqueCode)
                    {
                        return Json(new { isSuccess = false, message = $"{model.ClassCode} already exists!" });
                    }

                    await _secondTabService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully." });
                }

                var orderedKeys = new[] { "BaseAccountID", "ClassName", "ClassCode" };

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
        //[Permission("Edit", "SecondTab")]
        [ValidateAntiForgeryToken]
        [HttpPut]
        public async Task<IActionResult> Update(UpdateSecondTabVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _secondTabService.IsNameUniqueAsync(model.ClassName, model.BaseAccountID, model.ClassID);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = $"{model.ClassName} already exists!" });
                    }

                    var uniqueCode = await _secondTabService.IsCodeUniqueAsync(model.ClassCode, model.ClassID);
                    if (!uniqueCode)
                    {
                        return Json(new { isSuccess = false, message = $"{model.ClassCode} already exists!" });
                    }

                    await _secondTabService.UpdateAsync(model);
                    return Json(new { isSuccess = true, message = "Updated Successfully." });
                }

                var orderedKeys = new[] { "BaseAccountID", "BaseAccountCode", "ClassName" };
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
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "ClassID", string sortOrder = "desc")
        {
            var result = await _secondTabService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion


        #region GetById
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _secondTabService.GetByIdAsync(id);
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
        //[Permission("Delete", "SecondTab")]
        [HttpDelete]
        public async Task<IActionResult> Delete(DeleteRequestVM requestVM)
        {
            try
            {
                if (requestVM.Ids == null || !requestVM.Ids.Any() || requestVM.Ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No id selected to delete." });
                }
                var result = await _secondTabService.SoftDeleteAsync(requestVM);
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
        public async Task<IActionResult> CheckNameUnique(string name, int id)
        {
            try
            {
                if (name == null || name == "")
                    return Json(true);

                bool isUnique = await _secondTabService.IsNameUniqueAsync(name, id);
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

                bool isUnique = await _secondTabService.IsCodeUniqueAsync(code);
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
