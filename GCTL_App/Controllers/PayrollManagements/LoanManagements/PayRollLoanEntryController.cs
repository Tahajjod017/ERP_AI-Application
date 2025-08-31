using GCTL.Core.Repository;
using GCTL.Core.ViewModels.PayrollManagements.LoanManagement;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.PayRollManagements.PayRollLoanManagement;
using GCTL.Service.UserProfile;
using GCTL_App.ViewModels.PayRollManagements.LoanManagent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Client;
using OpenQA.Selenium.BiDi.Modules.Script;


namespace GCTL_App.Controllers.PayrollManagements.LoanManagements
{
    public class PayRollLoanEntryController : BaseController
    {
        private IGenericRepository<LoanInstallmentPeriods> loanInstallment;
        private readonly IPayRollLoanEntryService payRollLoanEntryService;
        public PayRollLoanEntryController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<LoanInstallmentPeriods> loanInstallment, IPayRollLoanEntryService payRollLoanEntryService) : base(translateService, userProfileService)
        {
            this.loanInstallment = loanInstallment;
            this.payRollLoanEntryService = payRollLoanEntryService;
        }

        public IActionResult Index()
        {
            PayrollLoanEntryPageVM model = new PayrollLoanEntryPageVM();
            ViewBag.LoanInstallmentDD = new SelectList(loanInstallment.AllActive(), "LoanInstallmentPeriodID", "PeriodText");
            return View(model);
        }

        #region Save 
        [Route("PayRollLoanEntry/SaveAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveAsync(LoanSaveVM model)
        {

            try
            {
                var data = await payRollLoanEntryService.SaveAsync(model);
                return Json(new {Success=data.Success, Message=data.Message});
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion
    }
}
