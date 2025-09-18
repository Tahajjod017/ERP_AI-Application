using GCTL.Core.Repository;
using GCTL.Data.Models;
using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveHistoryBalances;
using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace GCTL_App.Controllers.AttendanceManagement.LeaveManagements
{
    public class LeaveHistoryController : BaseController
    {
        private readonly IGenericRepository<LeaveTypes> leaveType;
        private readonly IGenericRepository<Statuses> status;
        private ILeaveRequestService leaveRequestService;
        private readonly ILeaveHistoryBalancesService leaveHistoryBalancesService;

        public LeaveHistoryController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<LeaveTypes> leaveType = null, IGenericRepository<Statuses> status = null, ILeaveRequestService leaveRequestService = null, ILeaveHistoryBalancesService leaveHistoryBalancesService = null) : base(translateService, userProfileService)
        {
            this.leaveType = leaveType;
            this.status = status;
            this.leaveRequestService = leaveRequestService;
            this.leaveHistoryBalancesService = leaveHistoryBalancesService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.LeaveTypeDD = new SelectList(leaveType.AllActive().Where(x=>x.IsActive==true), "LeaveTypeID", "LeaveTypeName");
            ViewBag.StatusDD = new SelectList(status.AllActive().Where(x => x.StatusName == "DECLINED" || x.StatusName == "APPROVED"), "StatusID", "StatusName");
            var companies = await leaveRequestService.GetCompanies();
            ViewBag.OrganizationDD = companies.Count == 1? new SelectList(companies, "Id", "Name", companies[0].Id) : new SelectList(companies, "Id", "Name");
            var departments = await leaveRequestService.GetDepartments();
            ViewBag.DepartmentDD = departments.Count == 1 ? new SelectList(departments, "Id", "Name", departments[0].Id) : new SelectList(departments, "Id", "Name");
            ViewBag.EmployeeList = await leaveRequestService.GetGroupedEmployees();

            return View();
        }

        #region Get All Data List

        [Route("LeaveHistoryRoute/GetAllTableHistoryAsync")]

        [HttpGet]
        public async Task<IActionResult> GetAllTableListAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", int? leaveTypeID = null, int? statusID = null, int? organizationId = null,
    List<int> departmentIds = null,
    List<int> employeeIds = null, DateOnly? fromDate = null, DateOnly? toDate = null)
        {
            try
            {
                string url = GetEmployeePictureURL();
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var data = await leaveHistoryBalancesService.GetAllTableHistoryAsync(pageNumber, pageSize, searchTerm, currentSortColumn, currentSortOrder, url, userId, leaveTypeID, statusID, organizationId, departmentIds, employeeIds, fromDate, toDate);
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

