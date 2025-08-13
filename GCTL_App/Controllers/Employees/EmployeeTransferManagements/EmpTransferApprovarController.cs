using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveApprovalDecline;
using GCTL.Core.ViewModels.Employee.EmpTransfer;
using GCTL.Data.Models;
using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveApprovalDecline;
using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Service.Employees.EmpTransfer;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using GCTL_App.ViewModels.AttendanceManagement.LeaveManagements.LeaveApproval;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GCTL_App.Controllers.Employees.EmployeeTransferManagements
{
    public class EmpTransferApprovarController : BaseController
    {
        private ILeaveRequestService leaveRequestService;
        private ILeaveApprovalService leaveApprovalService;
        private readonly IGenericRepository<Organization> organizationService;
        private readonly IGenericRepository<OrganizationBranches> organizationBranchesService;
        private IEmployeeTransferService employeeTransferService;
        private readonly IGenericRepository<Designations> designationsService;
        private readonly IGenericRepository<Departments> departmentsService;
        private readonly IGenericRepository<Statuses> status;
        private IEmpTransferApprovedOrDeclineService empTransferApprovedOrDeclineService;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> employee;
        public EmpTransferApprovarController(ITranslateService translateService, IUserProfileService userProfileService, ILeaveRequestService leaveRequestService, ILeaveApprovalService leaveApprovalService, IGenericRepository<Organization> organizationService, IGenericRepository<OrganizationBranches> organizationBranchesService, IEmployeeTransferService employeeTransferService, IGenericRepository<Designations> designationsService, IGenericRepository<Departments> departmentsService, IGenericRepository<Statuses> status, IEmpTransferApprovedOrDeclineService empTransferApprovedOrDeclineService, IGenericRepository<GCTL.Data.Models.Employees> employee = null) : base(translateService, userProfileService)
        {

            this.leaveRequestService = leaveRequestService;
            this.leaveApprovalService = leaveApprovalService;
            this.organizationService = organizationService;
            this.organizationBranchesService = organizationBranchesService;
            this.employeeTransferService = employeeTransferService;
            this.designationsService = designationsService;
            this.departmentsService = departmentsService;
            this.status = status;
            this.empTransferApprovedOrDeclineService = empTransferApprovedOrDeclineService;
            this.employee = employee;
        }



        public async Task<IActionResult> Index()
        {



            ViewBag.OrganizationDD = new SelectList(await leaveRequestService.GetCompanies(), "Id", "Name");
            ViewBag.DepartmentDD = new SelectList(await leaveRequestService.GetDepartments(), "Id", "Name");
            ViewBag.EmployeeList = await leaveRequestService.GetGroupedEmployees();
            ViewBag.OrganizationDD = new SelectList(organizationService.AllActive(), "OrganizationID", "OrganizationName");
            ViewBag.OrganizationBranchDD = new SelectList(organizationBranchesService.AllActive(), "OrganizationBranchID", "OrganizationBranchName");
            ViewBag.DesignationDD = new SelectList(designationsService.AllActive(), "DesignationID", "DesignationName");
            ViewBag.DepartmentDD = new SelectList(departmentsService.AllActive(), "DepartmentID", "DepartmentName");
            ViewBag.StatusDD = new SelectList(status.AllActive().Where(x => x.StatusName == "APPROVED" || x.StatusName == "DECLINE"), "StatusID", "StatusName");
            var allData = await employee.AllActive().Select(x => new  { EmployeeID = x.EmployeeID,  FullName = x.FirstName + " " + x.LastName }).ToListAsync();

            ViewBag.EmployeeDD = new SelectList(allData, "EmployeeID", "FullName");

            return View();
        }
        [Route("EmpTransferApprovar/UpdateEmployeeTransferAsync")]
        [HttpPost]
        public async Task<IActionResult> UpdateEmployeeTransferAsync([FromBody] EmployeeTransferApproveOrDecEditVM entityVM)
        {
            try
            {
                var data = await empTransferApprovedOrDeclineService.UpdateEmployeeTransferAsync(entityVM);
                return Json(data);
            }
            catch (Exception)
            {

                throw;
            }

        }
        #region GetBy EmployeeTransferID
        [Route("EmpTransferApprovar/GetEmployeeTransferByIdAsync")]
        [HttpGet]
        public async Task<IActionResult> GetEmployeeTransferByIdAsync(int employeeTransferID)
        {

            var data = await empTransferApprovedOrDeclineService.GetEmployeeTransferByIdAsync(employeeTransferID);
            return Json(data);
        }

        #endregion
        #region Get All Data List

        [Route("EmpTransferApprovar/GetAllTableListAsync")]

        [HttpGet]
        public async Task<IActionResult> GetAllTableListAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", int? organizationId = null,
    List<int> departmentIds = null,
    List<int> employeeIds = null, DateOnly? fromDate = null, DateOnly? toDate = null)
        {
            try
            {
                string url = GetEmployeePictureURL();
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var data = await empTransferApprovedOrDeclineService.GetAllTableAsync(pageNumber, pageSize, searchTerm, currentSortColumn, currentSortOrder, url, userId, organizationId, departmentIds, employeeIds, fromDate, toDate);
                return Json(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [Route("EmpTransferApprovar/GetAllTableListAsyncBelow")]

        [HttpGet]
        public async Task<IActionResult> GetAllTableListAsyncBelow(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", int? organizationId = null,
List<int> departmentIds = null,
List<int> employeeIds = null, DateOnly? fromDate = null, DateOnly? toDate = null)
        {
            try
            {
                string url = GetEmployeePictureURL();
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var data = await empTransferApprovedOrDeclineService.GetAllTableListAsyncBelow(pageNumber, pageSize, searchTerm, currentSortColumn, currentSortOrder, url, userId, organizationId, departmentIds, employeeIds, fromDate, toDate);
                return Json(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(ex.Message);
            }
        }
        #endregion
        #region Get All Or Single Employee according to loginID

        [Route("EmpTransferApprovar/GetEmployee")]
        [HttpGet]
        public async Task<IActionResult> GetEmployee()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return BadRequest();

            }
            var data = await leaveRequestService.GetAllEmployee(userId);
            return Json(data);
        }
        #endregion
    }
}
