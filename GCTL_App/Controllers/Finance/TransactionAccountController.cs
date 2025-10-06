using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.Finance.TransactionAccountVM;
using GCTL.Service.CommonService;
using GCTL.Service.Finance.TransactionAccount;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.Finance
{
    public class TransactionAccountController : BaseController
    {
        #region Services
        private readonly ITransactionAccountService _transactionAccountService;
        private readonly ICommonService _commonService;


        public TransactionAccountController(ITranslateService translateService, IUserProfileService userProfileService, ITransactionAccountService transactionAccountService, ICommonService commonService) : base(translateService, userProfileService)
        {
            _transactionAccountService = transactionAccountService;
            _commonService = commonService;
        }
        #endregion


        #region Index
        //[Permission("View", "TransactionAccount")]
        public async Task<IActionResult> Index()
        {
            try
            {
                //TransactionAccountPageVM model = new TransactionAccountPageVM();
                CreateTransactionAccountVM model = new CreateTransactionAccountVM();

                SetSmartPageCode(203700);

                ViewBag.BodyTabs = await _transactionAccountService.GetBodyTabsAsync();
                ViewBag.AccountClassDD = await _commonService.GetAccountClass();
                //ViewBag.AccountGroupDD = await _commonService.GetAccountGroup();
                ViewBag.MainAccDD = await _commonService.GetMainAccount();
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
        //[Permission("Create", "TransactionAccount")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(CreateTransactionAccountVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _transactionAccountService.IsNameUniqueAsync(model.TrxAccName, model.TrxAccID);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = $"{model.TrxAccName} already exists!" });
                    }

                    var uniqueCode = await _transactionAccountService.IsCodeUniqueAsync(model.TrxAccCode, model.TrxAccID);
                    if (!uniqueCode)
                    {
                        return Json(new { isSuccess = false, message = $"{model.TrxAccCode} already exists!" });
                    }

                    await _transactionAccountService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully." });
                }

                var orderedKeys = new[] { "ClassID", "GroupID", "MainAccountID", "SubAccountID", "TrxAccName", "TrxAccCode" };

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
        //[Permission("Edit", "TransactionAccount")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Update(UpdateTransactionAccountVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _transactionAccountService.IsNameUniqueAsync(model.TrxAccName, model.TrxAccID);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = $"{model.TrxAccName} already exists!" });
                    }

                    var uniqueCode = await _transactionAccountService.IsCodeUniqueAsync(model.TrxAccCode, model.TrxAccID);
                    if (!uniqueCode)
                    {
                        return Json(new { isSuccess = false, message = $"{model.TrxAccCode} already exists!" });
                    }

                    var result = await _transactionAccountService.UpdateAsync(model);
                    return Json(new
                    {
                        isSuccess = result.Success,
                        message = result.Message,
                        errors = result.Errors, // optional
                        data = result.Data      // optional
                    });
                }

                var orderedKeys = new[] { "ClassID", "GroupID", "MainAccountID", "SubAccountID", "BaseAccountCode", "TrxAccName" };
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
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "TrxAccID", string sortOrder = "desc", int? subAccId = null)
        {
            var result = await _transactionAccountService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder, subAccId);

            return Json(result);
        }
        #endregion


        #region GetById
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _transactionAccountService.GetByIdAsync(id);
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
        //[Permission("Delete", "TransactionAccount")]
        [HttpDelete]
        public async Task<IActionResult> Delete(DeleteRequestVM requestVM)
        {
            try
            {
                if (requestVM.Ids == null || !requestVM.Ids.Any() || requestVM.Ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No id selected to delete." });
                }
                var result = await _transactionAccountService.SoftDeleteAsync(requestVM);
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

                bool isUnique = await _transactionAccountService.IsNameUniqueAsync(name, id);
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

                bool isUnique = await _transactionAccountService.IsCodeUniqueAsync(code);
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
        //public async Task<IActionResult> GetAccountGroupByClassId(int classId)
        //{
        //    var result = await _commonService.GetAccountGroupByClassId(classId);
        //    return Json(result);
        //}
        #endregion


        #region GetSubAccByClassIdGroupIdMainAccId
        public async Task<IActionResult> GetSubAccByClassIdGroupIdMainAccId(int classId, int? groupId, int? mainAccId)
        {
            var result = await _commonService.GetSubAccByClassIdGroupIdMainAccId(classId, groupId, mainAccId);
            return Json(result);
        }
        #endregion


        #region GenerateNextCodeAsync
        [Route("TransactionAccount/GenerateNextCodeAsync")]
        [HttpGet]
        public async Task<IActionResult> GenerateNextCodeAsync(int mainAccId)
        {
            var result = await _transactionAccountService.GenerateNextCodeAsync(mainAccId);
            return Json(result);
        }
        #endregion
    }
}
