using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmpTransfer;
using GCTL.Data.Models;
using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveApprovalDecline;
using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Service.Employees.EmployeeTraining;
using GCTL.Service.Employees.EmpTransfer;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.AccessControl;
using System.Security.Claims;

namespace GCTL_App.Controllers.Employees.EmployeeTransferManagemnets
{
    public class EmployeeTransferManagementController : BaseController
    {
    
        private ILeaveRequestService leaveRequestService;
        private ILeaveApprovalService leaveApprovalService;
        private readonly IGenericRepository<Organization> organizationService;
        private readonly IGenericRepository<OrganizationBranches> organizationBranchesService;
        private IEmployeeTransferService  employeeTransferService;
        public EmployeeTransferManagementController(ITranslateService translateService, IUserProfileService userProfileService,  ILeaveRequestService leaveRequestService, ILeaveApprovalService leaveApprovalService, IGenericRepository<Organization> organizationService, IGenericRepository<OrganizationBranches> organizationBranchesService, IEmployeeTransferService employeeTransferService) : base(translateService, userProfileService)
        {
          
            this.leaveRequestService = leaveRequestService;
            this.leaveApprovalService = leaveApprovalService;
            this.organizationService = organizationService;
            this.organizationBranchesService = organizationBranchesService;
            this.employeeTransferService = employeeTransferService;
        }

      

        public async Task<IActionResult> Index()
        {

          
           
            ViewBag.OrganizationDD = new SelectList(await leaveRequestService.GetCompanies(), "Id", "Name");
            ViewBag.DepartmentDD = new SelectList(await leaveRequestService.GetDepartments(), "Id", "Name");
            ViewBag.EmployeeList = await leaveRequestService.GetGroupedEmployees();
            ViewBag.OrganizationDD=new SelectList( organizationService.AllActive(), "OrganizationID", "OrganizationName");
            ViewBag.OrganizationBranchDD = new SelectList(organizationBranchesService.AllActive(), "OrganizationBranchID", "OrganizationBranchName");
            return View();
        }

        
        [Route("EmployeeTransferManagement/SaveEmplopyeeTransfer")]
        [HttpPost]
            public async Task<IActionResult> SaveEmplopyeeTransfer(EmployeeTransferAddVM entityVM)
        {
            try
            {
                var data = await employeeTransferService.SaveEmployeeTansferAsync(entityVM);
                return Json(data);
            }
            catch (Exception)
            {

                throw;
            }
           
        }

        #region Get All Data List

        [Route("EmployeeTransferManagement/GetAllTableListAsync")]

        [HttpGet]
        public async Task<IActionResult> GetAllTableListAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", int? organizationId = null,
    List<int> departmentIds = null,
    List<int> employeeIds = null, DateOnly? fromDate = null, DateOnly? toDate = null)
        {
            try
            {
                string url = GetEmployeePictureURL();
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var data = await employeeTransferService.GetAllTableAsync(pageNumber, pageSize, searchTerm, currentSortColumn, currentSortOrder, url, userId, organizationId, departmentIds, employeeIds, fromDate, toDate);
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
