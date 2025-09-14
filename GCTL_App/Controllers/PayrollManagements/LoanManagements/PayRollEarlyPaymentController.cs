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
using System.Security.Claims;

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
            //ViewBag.OrganizationDD = new SelectList(await _commonService.GetOrganizations(), "Id", "Name");
            var result = await _commonService.GetOrganizations(search: "", page: 1, pageSize: 50);
            var organizations = result.Items;
            if (organizations.Count == 1)
            {
                model.Save.OrganizationID = organizations[0].Id;
            }
            ViewBag.OrganizationDD = new SelectList(organizations, "Id", "Name");
            ViewBag.DepartmentDD = await _commonService.GetDepartments();
            ViewBag.EmployeeList = await _commonService.GetEmpGroupedByDep();
            ViewBag.EmployeeDD = await payRollLoanEntryService.SelectAsync();
            var employees = await payRollEarlyPaymentService.SelectAsync();
            ViewBag.EmployeeDDEditDD = new SelectList(employees, "Id", "Name");

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
        #region Update Data

        [Route("PayRollEarlyPayment/UpdatePayRollEarlyPaymentAsync")]
        public async Task<IActionResult> UpdatePayRollEarlyPaymentAsync([FromBody]UpdateEarlyPayamentVM model)
        {
            try
            {
                var data = await payRollEarlyPaymentService.UpdatePayRollEarlyPaymentAsync(model);
                return Json(new { Success = data.Success, Message = data.Message });
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        #endregion


        #region  Table Data


        #endregion
        [Route("PayRollEarlyPayment/GetAllTableAsync")]

        [HttpGet]
        public async Task<IActionResult> GetAllTableAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", int? organizationId = null,
       List<int> departmentIds = null,
       List<int> employeeIds = null)
        {
            try
            {
                string url = GetEmployeePictureURL();
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var data = await payRollEarlyPaymentService.GetAllTableAsync(pageNumber, pageSize, searchTerm, currentSortColumn, currentSortOrder, url, userId, organizationId, departmentIds, employeeIds);
                return Json(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        #region  Get LaonDeatils
        [Route("PayRollEarlyPayment/GetLaonDetailsAsync")]
        [HttpGet]
        public async Task<IActionResult> GetLaonDetailsAsync(int id)
        {
            try
            {
                var data = await payRollEarlyPaymentService.GetLaonDetailsAsync(id);
                return Json(new { Success = data.Success, Data = data.Data });
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion
        #region GetDepartmentByOrganization
        [HttpGet]
        public async Task<IActionResult> GetDepartmentByOrganization(int? id)
        {
            var result = await payRollEarlyPaymentService.GetDepartmentsByOrgId(id);
            return Json(result);
        }
        #endregion
        #region GetEmployeesByOrgBraDepId
        public async Task<IActionResult> GetEmployeesByOrgBraDepId(int? orgId, [FromQuery] List<int>? branchIds, [FromQuery] List<int>? depIds)
        {
            var result = await payRollEarlyPaymentService.GetEmployeesByOrgBraDepId(orgId, branchIds, depIds);
            return Json(result);
        }
        #endregion
    }
}
