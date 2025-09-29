using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.Finance.ThirdTabVM;
using GCTL.Service.CommonService;
using GCTL.Service.Finance.ThirdTab;
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
    public class ThirdTabController : BaseController
    {
        #region Services
        private readonly IThirdTabService _thirdTabService;
        private readonly ICommonService _commonService;


        public ThirdTabController(ITranslateService translateService, IUserProfileService userProfileService, IThirdTabService baseAccountService, ICommonService commonService) : base(translateService, userProfileService)
        {
            _thirdTabService = baseAccountService;
            _commonService = commonService;
        }
        #endregion


        #region Index
        //[Permission("View", "ThirdTab")]
        public async Task<IActionResult> Index()
        {
            try
            {
                ThirdTabPageVM model = new ThirdTabPageVM();

                SetSmartPageCode(203800);

                var baseAccounts = await _commonService.GetAccountClass();
                if (baseAccounts.Count == 1)
                {
                    model.Create.ClassID = (int)baseAccounts[0].Id;
                }
                ViewBag.ClassDD = new SelectList(baseAccounts, "Id", "Name", model.Create.ClassID);

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
        //[Permission("Create", "ThirdTab")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(CreateThirdTabVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _thirdTabService.IsNameUniqueAsync(model.GroupName, model.ClassID);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = $"{model.GroupName} already exists!" });
                    }
                    await _thirdTabService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully." });
                }

                var orderedKeys = new[] { "GroupName", "BaseAccountCode" };

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
        //[Permission("Edit", "ThirdTab")]
        [ValidateAntiForgeryToken]
        [HttpPut]
        public async Task<IActionResult> Update(UpdateThirdTabVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _thirdTabService.IsNameUniqueAsync(model.GroupName, model.ClassID, model.GroupID);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = $"{model.GroupName} already exists!" });
                    }
                    await _thirdTabService.UpdateAsync(model);
                    return Json(new { isSuccess = true, message = "Updated Successfully." });
                }

                var orderedKeys = new[] { "BaseAccountCode", "GroupName" };
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
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "GroupID", string sortOrder = "desc")
        {
            var result = await _thirdTabService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion


        #region GetById
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _thirdTabService.GetByIdAsync(id);
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
        [Permission("Delete", "ThirdTab")]
        [HttpDelete]
        public async Task<IActionResult> Delete(DeleteRequestVM requestVM)
        {
            try
            {
                if (requestVM.Ids == null || !requestVM.Ids.Any() || requestVM.Ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No id selected to delete." });
                }
                var result = await _thirdTabService.SoftDeleteAsync(requestVM);
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

                bool isUnique = await _thirdTabService.IsNameUniqueAsync(name, id);
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
    }
}
