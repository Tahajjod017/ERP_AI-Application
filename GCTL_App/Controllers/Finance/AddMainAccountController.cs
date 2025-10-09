using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.Finance.AddMainAccountVM;
using GCTL.Service.CommonService;
using GCTL.Service.Finance.AddMainAccount;
using GCTL.Service.Language;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.Finance
{
    [Authorize]
    public class AddMainAccountController : BaseController
    {
        #region Services
        private readonly IAddMainAccountService _addMainAccountService;
        private readonly ICommonService _commonService;


        public AddMainAccountController(ITranslateService translateService, IUserProfileService userProfileService, IAddMainAccountService addMainAccountService, ICommonService commonService) : base(translateService, userProfileService)
        {
            _addMainAccountService = addMainAccountService;
            _commonService = commonService;
        }
        #endregion


        #region Index
        [Permission("View", "AddMainAccount")]
        public async Task<IActionResult> Index()
        {
            try
            {
                //AddMainAccountPageVM model = new AddMainAccountPageVM();
                CreateAddMainAccountVM model = new CreateAddMainAccountVM();

                SetSmartPageCode(203700);

                ViewBag.BodyTabs = await _addMainAccountService.GetBodyTabsAsync();
                ViewBag.ClassDD = await _commonService.GetAccountClass();

                //if (accountClass.Count == 1)
                //{
                //    model.Create.ClassID = (int)accountClass[0].Id;
                //}
                //ViewBag.AccountClassDD = new SelectList(accountClass, "Id", "Name", model.Create.ClassID);

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
        [Permission("Create", "AddMainAccount")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(CreateAddMainAccountVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _addMainAccountService.IsNameUniqueAsync(model.MainAccountName, model.MainAccountID);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = $"{model.MainAccountName} already exists!" });
                    }

                    var uniqueCode = await _addMainAccountService.IsCodeUniqueAsync(model.MainAccountCode, model.MainAccountID);
                    if (!uniqueCode)
                    {
                        return Json(new { isSuccess = false, message = $"{model.MainAccountCode} already exists!" });
                    }

                    await _addMainAccountService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", classId = model.ClassID });
                }

                var orderedKeys = new[] { "ClassID", "MainAccountName", "MainAccountCode" };

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
        [Permission("Edit", "AddMainAccount")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Update(UpdateAddMainAccountVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _addMainAccountService.IsNameUniqueAsync(model.MainAccountName, model.MainAccountID);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = $"{model.MainAccountName} already exists!" });
                    }

                    var uniqueCode = await _addMainAccountService.IsCodeUniqueAsync(model.MainAccountCode, model.MainAccountID);
                    if (!uniqueCode)
                    {
                        return Json(new { isSuccess = false, message = $"{model.MainAccountCode} already exists!" });
                    }

                    var result = await _addMainAccountService.UpdateAsync(model);
                    return Json(new
                    {
                        isSuccess = result.Success,
                        message = result.Message,
                        errors = result.Errors, // optional
                        data = result.Data      // optional
                    });
                }

                var orderedKeys = new[] { "ClassID", "BaseAccountCode", "MainAccountName" };
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
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "MainAccountID", string sortOrder = "desc", int? classId = null)
        {
            var result = await _addMainAccountService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder, classId);

            return Json(result);
        }
        #endregion


        #region GetById
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _addMainAccountService.GetByIdAsync(id);
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
        //[Permission("Delete", "AddMainAccount")]
        [HttpDelete]
        public async Task<IActionResult> Delete(DeleteRequestVM requestVM)
        {
            try
            {
                if (requestVM.Ids == null || !requestVM.Ids.Any() || requestVM.Ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No id selected to delete." });
                }
                var result = await _addMainAccountService.SoftDeleteAsync(requestVM);
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

                bool isUnique = await _addMainAccountService.IsNameUniqueAsync(name, id);
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

                bool isUnique = await _addMainAccountService.IsCodeUniqueAsync(code);
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
