using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveApprovalDecline;
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
        public EmpTransferApprovarController(ITranslateService translateService, IUserProfileService userProfileService, ILeaveRequestService leaveRequestService, ILeaveApprovalService leaveApprovalService, IGenericRepository<Organization> organizationService, IGenericRepository<OrganizationBranches> organizationBranchesService, IEmployeeTransferService employeeTransferService, IGenericRepository<Designations> designationsService, IGenericRepository<Departments> departmentsService, IGenericRepository<Statuses> status) : base(translateService, userProfileService)
        {

            this.leaveRequestService = leaveRequestService;
            this.leaveApprovalService = leaveApprovalService;
            this.organizationService = organizationService;
            this.organizationBranchesService = organizationBranchesService;
            this.employeeTransferService = employeeTransferService;
            this.designationsService = designationsService;
            this.departmentsService = departmentsService;
            this.status = status;
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
            return View();
        }

    }
}
