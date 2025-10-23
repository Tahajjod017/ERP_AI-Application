using GCTL.Core.ViewModels.Finance.PostingRuleDetailsVM;
using GCTL.Core.ViewModels.Finance.PostingRulesVM;
using GCTL.Core.ViewModels.Finance.TransactionAccountVM;
using GCTL.Data.Models;
using GCTL.Service.CommonService;
using GCTL.Service.Finance.PostingRule;
using GCTL.Service.Language;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.Finance
{
    [Authorize]
    public class PostingRulesController : BaseController
    {
        #region Services
        private readonly ICommonService _commonService;
        private readonly IPostingRulesService _postingRulesService;

        public PostingRulesController(ITranslateService translateService, IUserProfileService userProfileService, ICommonService commonService, IPostingRulesService postingRulesService) : base(translateService, userProfileService)
        {
            _commonService = commonService;
            _postingRulesService = postingRulesService;
        }
        #endregion


        #region Index
        [Permission("View", "PostingRules")]
        public async Task<IActionResult> Index()
        {
            try
            {
                CreatePostingRulesVM model = new CreatePostingRulesVM()
                {
                    PostingRuleDetailsVMs = new List<CreatePostingRuleDetailsVM>
                    {
                        new CreatePostingRuleDetailsVM { DebitCredit = "Debit" },
                        new CreatePostingRuleDetailsVM { DebitCredit = "Credit" }
                    }
                };
                SetSmartPageCode(203800);

                ViewBag.BodyTabs = await _postingRulesService.GetBodyTabsAsync();
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
        //[Permission("Create", "TransactionAccount")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(CreatePostingRulesVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _postingRulesService.IsNameUniqueAsync(model.ScenarioName, model.PostingRuleID);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = $"{model.ScenarioName} already exists!" });
                    }

                    var uniqueCode = await _postingRulesService.IsCodeUniqueAsync(model.ScenarioCode, model.PostingRuleID);
                    if (!uniqueCode)
                    {
                        return Json(new { isSuccess = false, message = $"{model.ScenarioCode} already exists!" });
                    }

                    await _postingRulesService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully." });
                }

                var orderedKeys = new[] { "ScenarioName", "ScenarioCode", "SubAccountID", "TrxAccName", "TrxAccCode" };

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


        #region CheckNameUnique
        [HttpPost]
        public async Task<IActionResult> CheckNameUnique(string name, int id)
        {
            try
            {
                if (name == null || name == "")
                    return Json(true);

                bool isUnique = await _postingRulesService.IsNameUniqueAsync(name, id);
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
        [Route("PostingRules/GenerateThreeDigitCodeAsync")]
        [HttpGet]
        public async Task<IActionResult> GenerateThreeDigitCodeAsync()
        {
            try
            {
                var result = await _postingRulesService.GenerateThreeDigitCodeAsync();
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
