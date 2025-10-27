using GCTL.Core.Helpers;
using GCTL.Service.CommonService;
using GCTL.Service.Finance.OpeningBalance;
using GCTL.Service.Language;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GCTL.Core.ViewModels.Finance.OpeningBalancesVM;

namespace GCTL_App.Controllers.Finance
{
    [Authorize]
    public class OpeningBalanceController : BaseController
    {
        #region Services
        private readonly ICommonService _commonService;
        private readonly IOpeningBalancesService _openingBalancesService;

        public OpeningBalanceController(ITranslateService translateService, IUserProfileService userProfileService, ICommonService commonService, IOpeningBalancesService openingBalancesService) : base(translateService, userProfileService)
        {
            _commonService = commonService;
            _openingBalancesService = openingBalancesService;
        }
        #endregion


        #region Index
        [Permission("View", "OpeningBalance")]
        public async Task<IActionResult> Index()
        {
            try
            {
                CreateOpeningBalancesVM model = new CreateOpeningBalancesVM();

                SetSmartPageCode(203900);

                ViewBag.MainAccDD = await _commonService.GetMainAccount();

                return View(model);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home");
            }
        }
        #endregion


        #region Create
        [Permission("Create", "OpeningBalance")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(CreateOpeningBalancesVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueCode = await _openingBalancesService.IsCodeUniqueAsync(model.OpeningBalanceCode, model.OpeningBalanceID);
                    if (!uniqueCode)
                    {
                        return Json(new { isSuccess = false, message = $"{model.OpeningBalanceCode} already exists!" });
                    }

                    var result = _openingBalancesService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully." });
                }

                var orderedKeys = new[] { "MainAccountID", "SubAccountID", "TrxAccID", "OpeningBalanceCode", "Amount", "TrxType" };

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
        [Permission("Edit", "OpeningBalance")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Update(UpdateOpeningBalancesVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueCode = await _openingBalancesService.IsCodeUniqueAsync(model.OpeningBalanceCode, model.OpeningBalanceID);
                    if (!uniqueCode)
                    {
                        return Json(new { isSuccess = false, message = $"{model.OpeningBalanceCode} already exists!" });
                    }

                    var result = await _openingBalancesService.UpdateAsync(model);
                    return Json(new
                    {
                        isSuccess = result.Success,
                        message = result.Message,
                        errors = result.Errors, // optional
                        data = result.Data      // optional
                    });
                }

                var orderedKeys = new[] { "MainAccountID", "SubAccountID", "TrxAccID", "OpeningBalanceCode", "Amount", "TrxType" };

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


        #region GetById
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _openingBalancesService.GetByIdAsync(id);
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
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "OpeningBalanceID", string sortOrder = "desc")
        {
            try
            {
                var result = await _openingBalancesService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }
        #endregion


        #region SoftDelete
        [Permission("Delete", "OpeningBalance")]
        [HttpDelete]
        public async Task<IActionResult> SoftDelete(DeleteRequestVM requestVM)
        {
            try
            {
                var result = await _openingBalancesService.SoftDeleteAsync(requestVM);
                return Json(new
                {
                    isSuccess = result.Success,
                    message = result.Message,
                    errors = result.Errors, // optional
                    data = result.Data      // optional
                });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        #endregion


        #region GetSubAccByMainAccId
        public async Task<IActionResult> GetSubAccByMainAccId(int? mainAccId)
        {
            try
            {
                var result = await _commonService.GetSubAccByMainAccId(mainAccId);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }
        #endregion


        #region GetTrxAccByMainAccIdSubAccId
        public async Task<IActionResult> GetTrxAccByMainAccIdSubAccId(int? mainAccId, int? subAccId)
        {
            try
            {
                var result = await _commonService.GetTrxAccByMainAccIdSubAccId(mainAccId, subAccId);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }
        #endregion


        #region GenerateThreeDigitCodeAsync
        [Route("OpeningBalance/GenerateThreeDigitCodeAsync")]
        [HttpGet]
        public async Task<IActionResult> GenerateThreeDigitCodeAsync()
        {
            try
            {
                var result = await _openingBalancesService.GenerateThreeDigitCodeAsync();
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }
        #endregion
    }
}
