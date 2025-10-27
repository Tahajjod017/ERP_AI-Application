using GCTL.Core.ViewModels.Finance.AddJournalVM;
using GCTL.Core.ViewModels.Finance.JournalDetailsVM;
using GCTL.Service.CommonService;
using GCTL.Service.Finance.AddJournal;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.Finance
{
    [Authorize]
    public class AddJournalController : BaseController
    {
        #region Services
        private readonly ICommonService _commonService;
        private readonly IAddJournalService _addJournalService;

        public AddJournalController(ITranslateService translateService, IUserProfileService userProfileService, ICommonService commonService, IAddJournalService addJournalService) : base(translateService, userProfileService)
        {
            _commonService = commonService;
            _addJournalService = addJournalService;
        }
        #endregion


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
                //ViewBag.MainAccDD = await _commonService.GetMainAccount();

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

                var orderedKeys = new[] { "JournalTypeID", "JournalCode", "PostingRuleID", "FinancialYearID", "JournalDate", "CreateJournalDetailsVMs", "TrxType", "Amount" };

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
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "JournalID", string sortOrder = "desc")
        {
            try
            {
                var result = await _addJournalService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }
        #endregion


        #region GetJournalDetailsByIdAsync
        [HttpGet("AddJournal/GetJournalDetailsByIdAsync")]
        public async Task<IActionResult> GetJournalDetailsByIdAsync(int id)
        {
            try
            {
                var result = await _addJournalService.GetJournalDetailsByIdAsync(id);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }
        #endregion


        #region GetDataByPostingRuleID
        public async Task<IActionResult> GetDataByPostingRuleID(int scenarioTypeId)
        {
            try
            {
                var result = await _addJournalService.GetDataByPostingRuleID(scenarioTypeId);
                return Json(new { isSuccess = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }
        #endregion


        #region GetMainAccount
        public async Task<IActionResult> GetMainAccount()
        {
            try
            {
                var result = await _commonService.GetMainAccount();
                return Json(new { isSuccess = true, data = result });
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
        [Route("AddJournal/GenerateThreeDigitCodeAsync")]
        [HttpGet]
        public async Task<IActionResult> GenerateThreeDigitCodeAsync()
        {
            try
            {
                var result = await _addJournalService.GenerateSixDigitCodeAsync();
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
