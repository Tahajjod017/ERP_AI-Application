using GCTL.Core.Repository;
using GCTL.Core.ViewModels.PayrollManagements.LoanManagement;
using GCTL.Data.Models;
using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveApprovalDecline;
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
    public class PayRollLoanViewController : BaseController
    {
        private readonly ICommonService _commonService;
        private readonly IPayRollLoanEntryService payRollLoanEntryService;
        private readonly IGenericRepository<LoanInstallmentPeriods> loanInstallment;
        public PayRollLoanViewController(ITranslateService translateService, IUserProfileService userProfileService, ICommonService commonService, IPayRollLoanEntryService payRollLoanEntryService, IGenericRepository<LoanInstallmentPeriods> loanInstallment) : base(translateService, userProfileService)
        {
            _commonService = commonService;
            this.payRollLoanEntryService = payRollLoanEntryService;
            this.loanInstallment = loanInstallment;
        }

        public async Task<IActionResult> Index()
        {
            PayRollLoanViewPageVM model = new PayRollLoanViewPageVM();
            model.Save = new PayRollLoanViewDeclineApprovedVM();
            ViewBag.OrganizationDD = new SelectList(await _commonService.GetOrganizations(), "Id", "Name");
            ViewBag.DepartmentDD = await _commonService.GetDepartments();
            ViewBag.EmployeeList = await _commonService.GetEmpGroupedByDep();
            ViewBag.EmployeeDD = await payRollLoanEntryService.SelectAsync();
            ViewBag.LoanInstallmentDD = new SelectList(loanInstallment.AllActive(), "LoanInstallmentPeriodID", "PeriodText");
            return View(model);
        }

        #region GetEmployeesByOrgBraDepId
        public async Task<IActionResult> GetEmployeesByOrgBraDepId(int? orgId, [FromQuery] List<int>? branchIds, [FromQuery] List<int>? depIds)
        {
            var result = await _commonService.GetEmployeesByOrgBraDepId(orgId, branchIds, depIds);
            return Json(result);
        }
        #endregion
        #region Get All Data List

        [Route("PayRollLoanView/GetAllTableListAsync")]

        [HttpGet]
        public async Task<IActionResult> GetAllTableListAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "",  int? organizationId = null,
    List<int> departmentIds = null,
    List<int> employeeIds = null)
        {
            try
            {
                string url = GetEmployeePictureURL();
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var data = await payRollLoanEntryService.GetAllTableAsync(pageNumber, pageSize, searchTerm, currentSortColumn, currentSortOrder, url, userId, organizationId, departmentIds, employeeIds);
                return Json(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(ex.Message);
            }
        }
        #endregion
        #region Get ByData for Approved or Decline
        [Route("PayRollLoanView/GetByIdApprovedOrDecline")]
        [HttpGet]
        public async Task<IActionResult> GetByIdApprovedOrDecline(int id)
        {
            try
            {
                var data = await payRollLoanEntryService.GetByIdApprovedOrDecline(id);
                return Json(new { Success = data.Success, Data=data.Data});
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion
        #region Update   Approved or Decline

        [Route("PayRollLoanView/UpdateFromAppDecAsync")]
        [HttpPost]
        public async Task<IActionResult> UpdateFromAppDecAsync([FromBody]PayRollLoanViewDeclineApprovedVM formdata)
        {
            if (!ModelState.IsValid)
            {
                // Return all errors
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { Success = false, Message = "Validation Failed.", Errors = errors });
            }

            try
            {
                var data = await payRollLoanEntryService.UpdateFromAppDecAsync(formdata);
                return Json(new { Success = data.Success, Message = data.Message });
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

    }
}
