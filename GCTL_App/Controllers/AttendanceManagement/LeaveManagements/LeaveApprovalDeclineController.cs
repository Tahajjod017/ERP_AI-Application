using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveApprovalDecline;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Data.Models;
using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveApprovalDecline;
using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using GCTL_App.ViewModels.AttendanceManagement.LeaveManagements.LeaveApproval;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SkiaSharp;
using System.Security.Claims;

namespace GCTL_App.Controllers.AttendanceManagement.LeaveManagements
{
    [Authorize]
    public class LeaveApprovalDeclineController : BaseController
    {
        private readonly ILeaveApprovalService leaveApprovalService;
        private readonly IGenericRepository<LeaveTypes> leaveType;
        private readonly IGenericRepository<Statuses> status;
        private ILeaveRequestService leaveRequestService;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> employee;
        private readonly AppDbContext appDb;
        private readonly IGenericRepository<EmployeeOfficeInfo> empoffi;
        public LeaveApprovalDeclineController(ITranslateService translateService, IUserProfileService userProfileService, ILeaveApprovalService leaveApprovalService, IGenericRepository<LeaveTypes> leaveType, IGenericRepository<Statuses> status, ILeaveRequestService leaveRequestService, IGenericRepository<GCTL.Data.Models.Employees> employee, AppDbContext appDb, IGenericRepository<EmployeeOfficeInfo> empoffi) : base(translateService, userProfileService)
        {
            this.leaveApprovalService = leaveApprovalService;
            this.leaveType = leaveType;
            this.status = status;
            this.leaveRequestService = leaveRequestService;
            this.employee = employee;
            this.appDb = appDb;
            this.empoffi = empoffi;
        }

        public bool IsEligibleForLeave(LeaveTypes leaveType, DateOnly joiningDate, DateOnly today)
        {
            if (leaveType.EffectiveFromMonthYear?.Contains("Years") == true && leaveType.EffectiveFrom.HasValue)
            {
                return today >= joiningDate.AddYears(leaveType.EffectiveFrom.Value);
            }

            if (leaveType.EffectiveFromMonthYear?.Contains("Months") == true && leaveType.EffectiveFrom.HasValue)
            {
                return today >= joiningDate.AddMonths(leaveType.EffectiveFrom.Value);
            }

            if (!string.IsNullOrEmpty(leaveType.EffectiveFromMonthYear))
            {
                if (DateTime.TryParse(leaveType.EffectiveFromMonthYear, out var effDate))
                {
                    return today >= DateOnly.FromDateTime(effDate);
                }
            }

            if (leaveType.EffectiveAfter?.Contains("After Joining Date") == true)
            {
                return today >= joiningDate;
            }

            return true;
        }

        public async Task< IActionResult> Index()
        {
            LeaveApprovalPageVM model = new LeaveApprovalPageVM
            {
                Setup = new LeaveApplicationApprovalModifyVM()
            };
            string url = GetEmployeePictureURL();
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var employeeId = await appDb.Users
                   .Where(u => u.Id == userId)
                   .Select(e => e.EmployeeId)
                   .FirstOrDefaultAsync();

            var employee22 = await empoffi.AllActive().Where(e => e.EmployeeID == employeeId).Select(e => new { e.JoiningDate }).FirstOrDefaultAsync();

            if (employee22 == null)
            {
                return NotFound("Employee not found");
            }

            var joiningDate = employee22.JoiningDate ?? DateOnly.FromDateTime(DateTime.Today);
            var today = DateOnly.FromDateTime(DateTime.Today);


            var leaveTypes = await leaveType.AllActive().Where(x => x.IsActive).ToListAsync();   
            var eligibleLeaveTypes = leaveTypes.Where(x => IsEligibleForLeave(x, joiningDate, today)).ToList();
            ViewBag.LeaveTypeDD = new SelectList(eligibleLeaveTypes, "LeaveTypeID", "LeaveTypeName");

            ViewBag.StatusDD = new SelectList(status.AllActive().Where(x=>x.StatusName=="APPROVED" || x.StatusName== "DECLINED"), "StatusID", "StatusName");
            var employees =await  employee.AllActive().Select(e => new  {  e.EmployeeID, FullName = e.FirstName + " " + e.LastName  }).ToListAsync();
            if (employees.Count == 1)
            {
                ViewBag.EmployeesDD = new SelectList(employees, "EmployeeID", "FullName", employees[0].EmployeeID);
            }
            else
            {
                ViewBag.EmployeesDD = new SelectList(employees, "EmployeeID", "FullName");
            }

            return View(model);
        }

        #region Get All Data List above table Data

        [Route("LeaveApprovalDeclineRoute/GetAllTableListAsync")]

        [HttpGet]
        public async Task<IActionResult> GetAllTableListAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", int? leaveTypeID = null, int? statusID = null, DateOnly? fromDate = null, DateOnly? toDate = null)
        {
            try
            {
                string url = GetEmployeePictureURL();
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var data = await leaveApprovalService.GetAllTableAsync(pageNumber, pageSize, searchTerm, currentSortColumn, currentSortOrder, url, userId, leaveTypeID, statusID, fromDate, toDate);
                return Json(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(ex.Message);
            }
        }
        #endregion 

        #region Get All Data List  below Data

        [Route("LeaveApprovalDeclineRoute/GetAllTableBelowAsync")]

        [HttpGet]
        public async Task<IActionResult> GetAllTableBelowAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", int? leaveTypeID = null, int? statusID = null, DateOnly? fromDate=null, DateOnly ? toDate=null)
        {
            try
            {
                string url = GetEmployeePictureURL();
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var data = await leaveApprovalService.GetAllTableBelowAsync(pageNumber, pageSize, searchTerm, currentSortColumn, currentSortOrder, url, userId, leaveTypeID, statusID, fromDate, toDate);
                return Json(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(ex.Message);
            }
        }
        #endregion


       

        #region  Update Leave request in Approval Side
        [Route("LeaveApprovalDeclineRoute/UpdateRequestAsync")]
        [HttpPost]
        public async Task<IActionResult> UpdateRequestAsync([FromBody]LeaveApplicationApprovalModifyVM entityVM)
        {
            try
            {
                int? employeeId = await GetCurrentEmployeeIdAsync();
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
                }
                string url = $"{Request.Scheme}://{Request.Host.Value}";
                var data = await leaveApprovalService.UpdateLeaveRequestAsynce(entityVM,url);
                return Json(data);
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Get By Data leave Request
        [Route("LeaveApprovalDeclineRoute/GetLeaveRequestByIdAsync")]

        [HttpGet]
        public async Task<IActionResult> GetLeaveRequestByIdAsync(int leaveApplicationID)
        {
            try
            {
                if (leaveApplicationID == 0) return BadRequest();
                var data = await leaveApprovalService.GetLeaveRequestByIdAsync(leaveApplicationID);
                return Json(data);
            }
            catch (Exception)
            {

                throw;
            }

        }

        #endregion

        #region Check Leave Subsequent Days 
        [HttpGet]
        [Route("LeaveApprovalDeclineRoute/SubsequentLeaveCount")]
        public async Task<IActionResult> SubsequentLeaveCount(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var data = await leaveRequestService.SubsequentAsynce(fromDate, toDate);
                return Json(data);
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

        #region Display Leave banlance

        [Route("LeaveApprovalDeclineRoute/GetLeaveTypeBalancesForEmployeeDisplay")]

        [HttpGet]
        public async Task<IActionResult> GetLeaveTypeBalancesForEmployee()
        {

            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                    return BadRequest("EmployeeID not found in claims.");

               
                var data = await leaveApprovalService.GetLeaveTypeBalancesForEmployee(userId);
                return Json(data);
            }
            catch (Exception ex)
            {
                // Optional: log the exception before re-throwing
                throw;
            }

        }
        #endregion
    }
}
