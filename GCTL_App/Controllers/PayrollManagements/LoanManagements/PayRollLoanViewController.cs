using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveApprovalDecline;
using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Service.CommonService;
using GCTL.Service.Language;
using GCTL.Service.PayRollManagements.PayRollLoanManagement;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace GCTL_App.Controllers.PayrollManagements.LoanManagements
{
    public class PayRollLoanViewController : BaseController
    {
        private readonly ICommonService _commonService;
        private readonly IPayRollLoanEntryService payRollLoanEntryService;
        public PayRollLoanViewController(ITranslateService translateService, IUserProfileService userProfileService, ICommonService commonService, IPayRollLoanEntryService payRollLoanEntryService) : base(translateService, userProfileService)
        {
            _commonService = commonService;
            this.payRollLoanEntryService = payRollLoanEntryService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.OrganizationDD = new SelectList(await _commonService.GetOrganizations(), "Id", "Name");
            ViewBag.DepartmentDD = await _commonService.GetDepartments();
            ViewBag.EmployeeList = await _commonService.GetEmpGroupedByDep();
            return View();
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
    }
}
