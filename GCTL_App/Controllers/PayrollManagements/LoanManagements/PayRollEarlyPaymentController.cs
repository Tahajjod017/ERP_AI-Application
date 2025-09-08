using GCTL.Core.Repository;
using GCTL.Core.ViewModels.PayrollManagements.PayRollEarlyLoanpayment;
using GCTL.Data.Models;
using GCTL.Service.CommonService;
using GCTL.Service.Language;
using GCTL.Service.PayRollManagements.PayRollLoanManagement;
using GCTL.Service.UserProfile;
using GCTL_App.ViewModels.PayRollManagements.LoanManagent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.PayrollManagements.LoanManagements
{
    public class PayRollEarlyPaymentController : BaseController
    {
        private IGenericRepository<LoanInstallmentPeriods> loanInstallment;
        private readonly IPayRollLoanEntryService payRollLoanEntryService;
        private readonly ICommonService _commonService;
        private readonly IPayRollEarlyPaymentService payRollEarlyPaymentService;
        public PayRollEarlyPaymentController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<LoanInstallmentPeriods> loanInstallment, IPayRollLoanEntryService payRollLoanEntryService , ICommonService commonService, IPayRollEarlyPaymentService payRollEarlyPaymentService) : base(translateService, userProfileService)
        {
            this.loanInstallment = loanInstallment;
            this.payRollLoanEntryService = payRollLoanEntryService;
            _commonService = commonService;
            this.payRollEarlyPaymentService = payRollEarlyPaymentService;
        }

        public async Task<IActionResult> Index()
        {
            PayRollEarlyPaymentPage model = new PayRollEarlyPaymentPage();
            ViewBag.OrganizationDD = new SelectList(await _commonService.GetOrganizations(), "Id", "Name");
            ViewBag.DepartmentDD = await _commonService.GetDepartments();
            ViewBag.EmployeeList = await _commonService.GetEmpGroupedByDep();
            ViewBag.EmployeeDD = await payRollLoanEntryService.SelectAsync();
            ViewBag.LoanInstallmentDD = new SelectList(loanInstallment.AllActive(), "PeriodValue", "PeriodText");
            return View(model);
        }

        #region Get LoanId by 
        [Route("PayRollEarlyPayment/GetPayRollEarlyPaymentAsync")]
        [HttpGet]
        public async Task<IActionResult> GetPayRollEarlyPaymentAsync(int id)
        {
            try
            {
                var data=await payRollEarlyPaymentService.GetPayRollEarlyPaymentAsync(id);
               return Json(new { Success = data.Success , Data=data.Data});
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion
        #region Save Early payment Loan
        [Route("PayRollEarlyPayment/SaveAsynce")]
        public async Task<IActionResult> SaveAsynce(SaveEarlyPaymentVM model)
        {
            try
            {
                var data = await payRollEarlyPaymentService.SavePayRollEarlyPaymentAsync(model);
                return Json(new { Success = data.Success, Message = data.Message});
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        #endregion
    }
}
