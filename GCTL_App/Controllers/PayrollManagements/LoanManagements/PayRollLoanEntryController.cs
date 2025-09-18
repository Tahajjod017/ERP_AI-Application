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
using System.Security.Claims;



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
            //ViewBag.OrganizationDD = new SelectList(await _commonService.GetOrganizations(), "Id", "Name");
            var result = await _commonService.GetOrganizations(search: "", page: 1, pageSize: 50);
            var organizations = result.Items;
            if (organizations.Count == 1)
            {
                model.Save.OrganizationID = organizations[0].Id;
            }
            ViewBag.OrganizationDD = new SelectList(organizations, "Id", "Name");
            if (organizations.Count == 1)
            {
                ViewBag.OrganizationDDTable = new SelectList(organizations, "Id", "Name", organizations[0].Id);
            }
            else
            {
                ViewBag.OrganizationDDTable = new SelectList(organizations, "Id", "Name");
            }
            ViewBag.DepartmentDD = await _commonService.GetDepartments();
            ViewBag.EmployeeList = await _commonService.GetEmpGroupedByDep();
            ViewBag.EmployeeDD = await payRollLoanEntryService.SelectAsync();
            ViewBag.LoanInstallmentDD = new SelectList(loanInstallment.AllActive(), "PeriodValue", "PeriodText");
          
            return View(model);
        }


        #region Get All Data List

        [Route("PayRollLoanView/LoanEntryList")]

        [HttpGet]
        public async Task<IActionResult> LoanEntryList(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", int? organizationId = null,
    List<int> departmentIds = null,
    List<int> employeeIds = null)
        {
            try
            {
                string url = GetEmployeePictureURL();
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var data = await payRollLoanEntryService.LoanEntryList(pageNumber, pageSize, searchTerm, currentSortColumn, currentSortOrder, url, userId, organizationId, departmentIds, employeeIds);
                return Json(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        #endregion

        #region  Loan Step
        [Route("PayRollLoanView/PayRollLoanStep")]

        [HttpGet]
        public async Task<IActionResult> PayRollLoanStep(int id)
        {

            try
            {


                if (id == 0)
                    return BadRequest("loan not found in claims.");


                var data = await payRollLoanEntryService.PayRollLoanStep(id);
                return Json(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }

        }
        #endregion


        #region Save 
        [Route("PayRollLoanEntry/SaveAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveAsync(LoanSaveVM model)
        {
            if (!ModelState.IsValid)
            {
                // Return all errors
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
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

        #region Update
        [Route("PayRollLoanEntry/UpdateAsync")]
        [HttpPost]
        public async Task<IActionResult> UpdateAsync([FromBody]LoanUpdateVM model)
        {

            try
            {
                var data = await payRollLoanEntryService.UpdateAsync(model);
                return Json(new { Success=data.Success, Message=data.Message});
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

        #region Get ByData for Approved or Decline
        [Route("PayRollLoanEntry/GetByAsync")]
        [HttpGet]
        public async Task<IActionResult> GetByAsync(int id)
        {
            try
            {
                var data = await payRollLoanEntryService.GetByAsync(id);
                return Json(new { Success = data.Success, Data = data.Data });
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
