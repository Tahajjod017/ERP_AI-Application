using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.Finance.AddSubAccountVM;
using GCTL.Service.CommonService;
using GCTL.Service.Finance.AddSubAccount;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.Finance
{
    public class AddSubAccountController : BaseController
    {
        #region Services
        private readonly IAddSubAccountService _addSubAccountService;
        private readonly ICommonService _commonService;


        public AddSubAccountController(ITranslateService translateService, IUserProfileService userProfileService, IAddSubAccountService addSubAccountService, ICommonService commonService) : base(translateService, userProfileService)
        {
            _addSubAccountService = addSubAccountService;
            _commonService = commonService;
        }
        #endregion


        #region Index
        //[Permission("View", "AddSubAccount")]
        public async Task<IActionResult> Index()
        {
            try
            {
                //AddSubAccountPageVM model = new AddSubAccountPageVM();
                CreateAddSubAccountVM model = new CreateAddSubAccountVM();

                SetSmartPageCode(203700);

                ViewBag.AccountClassDD = await _commonService.GetAccountClass();
                ViewBag.AccountGroupDD = await _commonService.GetAccountGroup();
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
        //[Permission("Create", "AddSubAccount")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(CreateAddSubAccountVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _addSubAccountService.IsNameUniqueAsync(model.SubAccountName, model.SubAccountID);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = $"{model.SubAccountName} already exists!" });
                    }

                    var uniqueCode = await _addSubAccountService.IsCodeUniqueAsync(model.SubAccountCode, model.SubAccountID);
                    if (!uniqueCode)
                    {
                        return Json(new { isSuccess = false, message = $"{model.SubAccountCode} already exists!" });
                    }

                    await _addSubAccountService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully." });
                }

                var orderedKeys = new[] { "ClassID", "GroupID", "MainAccountID", "SubAccountName", "SubAccountCode" };

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
        //[Permission("Edit", "AddSubAccount")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Update(UpdateAddSubAccountVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _addSubAccountService.IsNameUniqueAsync(model.SubAccountName, model.SubAccountID);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = $"{model.SubAccountName} already exists!" });
                    }

                    var uniqueCode = await _addSubAccountService.IsCodeUniqueAsync(model.SubAccountCode, model.SubAccountID);
                    if (!uniqueCode)
                    {
                        return Json(new { isSuccess = false, message = $"{model.SubAccountCode} already exists!" });
                    }

                    await _addSubAccountService.UpdateAsync(model);
                    return Json(new { isSuccess = true, message = "Updated Successfully." });
                }

                var orderedKeys = new[] { "ClassID", "GroupID", "MainAccountID", "BaseAccountCode", "SubAccountName" };
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
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "SubAccountID", string sortOrder = "desc")
        {
            var result = await _addSubAccountService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion


        #region GetById
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _addSubAccountService.GetByIdAsync(id);
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
        //[Permission("Delete", "AddSubAccount")]
        [HttpDelete]
        public async Task<IActionResult> Delete(DeleteRequestVM requestVM)
        {
            try
            {
                if (requestVM.Ids == null || !requestVM.Ids.Any() || requestVM.Ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No id selected to delete." });
                }
                var result = await _addSubAccountService.SoftDeleteAsync(requestVM);
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

                bool isUnique = await _addSubAccountService.IsNameUniqueAsync(name, id);
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

                bool isUnique = await _addSubAccountService.IsCodeUniqueAsync(code);
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


        #region GetAccountGroupByClassId
        public async Task<IActionResult> GetAccountGroupByClassId(int classId)
        {
            var result = await _commonService.GetAccountGroupByClassId(classId);
            return Json(result);
        }
        #endregion


        #region GetMainAccByClassIdGroupId
        public async Task<IActionResult> GetMainAccByClassIdGroupId(int classId, int? groupId)
        {
            var result = await _commonService.GetMainAccByClassIdGroupId(classId, groupId);
            return Json(result);
        }
        #endregion


        #region GenerateNextCodeAsync
        [Route("AddSubAccount/GenerateNextCodeAsync")]
        [HttpGet]
        public async Task<IActionResult> GenerateNextCodeAsync(int mainAccId)
        {
            var result = await _addSubAccountService.GenerateNextCodeAsync(mainAccId);
            return Json(result);
        }
        #endregion
    }
}
