using GCTL.Core.ViewModels.Finance.AddJournalVM;
using GCTL.Core.ViewModels.Finance.JournalDetailsVM;
using GCTL.Service.CommonService;
using GCTL.Service.Finance.AddJournal;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.Finance
{
    public class AddJournalController : BaseController
    {
        private readonly ICommonService _commonService;
        private readonly IAddJournalService _addJournalService;

        public AddJournalController(ITranslateService translateService, IUserProfileService userProfileService, ICommonService commonService, IAddJournalService addJournalService) : base(translateService, userProfileService)
        {
            _commonService = commonService;
            _addJournalService = addJournalService;
        }


        #region Index
        public async Task<IActionResult> Index()
        {
            try
            {
                SetSmartPageCode(203900);

                CreateAddJournalVM model = new CreateAddJournalVM()
                {
                    CreateJournalDetailsVMs = new List<CreateJournalDetailsVM?>()
                    {
                        new CreateJournalDetailsVM(),
                        new CreateJournalDetailsVM()
                    }
                };

                ViewBag.JournalTypeDD = new SelectList(await _commonService.GetJournalType(), "Id", "Name");
                ViewBag.ScenarioTypeDD = new SelectList(await _commonService.GetScenarioType(), "Id", "Name");
                ViewBag.FinancialYearDD = new SelectList(await _commonService.GetFinancialYears(), "Id", "Name");
                ViewBag.MainAccDD = await _commonService.GetMainAccount();

                return View(model);
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Home");
            }
        }
        #endregion


        #region Create
        public async Task<IActionResult> Create(CreateAddJournalVM model)
        {
            try
            {
                if(model.CreateJournalDetailsVMs == null || !model.CreateJournalDetailsVMs.Any())
                {
                    ModelState.AddModelError("CreateJournalDetailsVMs", "Journal Details is Required!");
                }

                if (ModelState.IsValid)
                {
                    var uniqueCode = await _addJournalService.IsCodeUniqueAsync(model.JournalCode, model.JournalID);
                    if (!uniqueCode)
                    {
                        return Json(new { isSuccess = false, message = $"{model.JournalCode} already exists!" });
                    }

                    var result = await _addJournalService.AddAsync(model);
                    return Json(new
                    {
                        isSuccess = result.Success,
                        message = result.Message,
                        errors = result.Errors, // optional
                        data = result.Data      // optional
                    });
                }

                var orderedKeys = new[] { "JournalTypeID", "JournalCode", "PostingRuleID", "FinancialYearID", "JournalDate", "CreateJournalDetailsVMs", "Debit", "Credit" };

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


        #region GetMainAccByScenarioTypeId
        public async Task<IActionResult> GetMainAccByScenarioTypeId(int? scenarioTypeId)
        {
            try
            {
                var result = await _commonService.GetMainAccByScenarioTypeId(scenarioTypeId);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }
        #endregion


        #region GetSbuAccByScenarioTypeId
        public async Task<IActionResult> GetSbuAccByScenarioTypeId(int? scenarioTypeId)
        {
            try
            {
                var result = await _commonService.GetSbuAccByScenarioTypeId(scenarioTypeId);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }
        #endregion


        #region GetTrxAccByScenarioTypeId
        public async Task<IActionResult> GetTrxAccByScenarioTypeId(int? scenarioTypeId)
        {
            try
            {
                var result = await _commonService.GetTrxAccByScenarioTypeId(scenarioTypeId);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }
        #endregion


        #region GenerateThreeDigitCodeAsync
        [Route("AddJournal/GenerateThreeDigitCodeAsync")]
        [HttpGet]
        public async Task<IActionResult> GenerateThreeDigitCodeAsync()
        {
            try
            {
                var result = await _addJournalService.GenerateThreeDigitCodeAsync();
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
