using GCTL.Core.Repository;
using GCTL.Core.ViewModels.PayrollManagements.LoanManagement;
using GCTL.Data.Models;
using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Service.CommonService;
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
        private readonly ICommonService _commonService;
       
        public PayRollLoanEntryController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<LoanInstallmentPeriods> loanInstallment, IPayRollLoanEntryService payRollLoanEntryService, ICommonService commonService) : base(translateService, userProfileService)
        {
            this.loanInstallment = loanInstallment;
            this.payRollLoanEntryService = payRollLoanEntryService;
            _commonService = commonService;
           
        }

        public async Task<IActionResult> Index()
        {
            PayrollLoanEntryPageVM model = new PayrollLoanEntryPageVM();
            ViewBag.OrganizationDD = new SelectList(await _commonService.GetOrganizations(), "Id", "Name");
            ViewBag.DepartmentDD = await _commonService.GetDepartments();
            ViewBag.EmployeeList = await _commonService.GetEmpGroupedByDep();
            ViewBag.LoanInstallmentDD = new SelectList(loanInstallment.AllActive(), "PeriodValue", "PeriodText");
          
            return View(model);
        }

        #region Save 
        [Route("PayRollLoanEntry/SaveAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveAsync(LoanSaveVM model)
        {
            if (!ModelState.IsValid)
            {
                // Return all errors
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                               .Select(e => e.ErrorMessage)
                                               .ToList();
                return Json(new { Success = false, Message = "Validation Failed.", Errors = errors });
            }

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


        #region GetEmployeesByOrgBraDepId
        public async Task<IActionResult> GetEmployeesByOrgBraDepId(int? orgId, [FromQuery] List<int>? branchIds, [FromQuery] List<int>? depIds, string? search, int? page = 1, int? pageSize = 10)
        {
            var result = await _commonService.GetEmployeesByOrgBraDepId(orgId, branchIds, depIds, search, page, pageSize);
            return Json(result);
        }
        #endregion
    }
}
