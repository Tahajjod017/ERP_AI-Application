using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveApprovalDecline;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Data.Models;
using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveApprovalDecline;
using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using GCTL_App.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Execution;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using System.Security.Claims;

namespace GCTL_App.Controllers.AttendanceManagement.LeaveManagements
{
   
    public class LeaveRequestController : BaseController
    {
        private readonly IGenericRepository<LeaveTypes> leaveType;
        private readonly IGenericRepository<Statuses> status;
        private ILeaveRequestService  leaveRequestService;
        private ILeaveApprovalService leaveApprovalService;
        public LeaveRequestController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<LeaveTypes> leaveType, IGenericRepository<Statuses> status, ILeaveRequestService leaveRequestService, ILeaveApprovalService leaveApprovalService = null) : base(translateService, userProfileService)
        {
            this.leaveType = leaveType;
            this.status = status;
            this.leaveRequestService = leaveRequestService;
            this.leaveApprovalService = leaveApprovalService;
        }

        public async Task< IActionResult> Index()
        {
            LeaveApplicationsRequestPageVM model = new LeaveApplicationsRequestPageVM
            {
                SetupForm = new LeaveApplicationsRequestVM(),
                SetupFormEdit=new LeaveApplicationEditVM(),
            };


            ViewBag.LeaveTypeDD = new SelectList(leaveType.AllActive(), "LeaveTypeID", "LeaveTypeName");
            ViewBag.StatusDD = new SelectList(status.AllActive(), "StatusID", "StatusName");
            ViewBag.OrganizationDD = new SelectList(await leaveRequestService.GetCompanies(), "Id", "Name");
            ViewBag.DepartmentDD = new SelectList(await leaveRequestService.GetDepartments(), "Id", "Name");
            ViewBag.EmployeeList = await leaveRequestService.GetGroupedEmployees();
            return View(model);
        }
        #region Get All Or Single Employee according to loginID
       
        [Route("LeaveRequest/GetEmployee")]
        [HttpGet]
        public async Task<IActionResult>GetEmployee()
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

        #region  Get LeaveDays
        [HttpGet]
        [Route("LeaveRequest/GetLeaveDays")]
        public async Task<IActionResult> GetLeaveDays(int employeeId, int leaveTypeId)
        {
            var data = await leaveRequestService.GetLeaveTypeTotaldays(employeeId, leaveTypeId);

            if (data == null)
            {
                return NotFound();
            }
            return Json(data);
        }

        #endregion

        #region  Save/Update Data 

        [HttpPost]
        public async Task<IActionResult> SaveLeaveRequest(LeaveApplicationsRequestVM model)
        {
            if (!ModelState.IsValid)
            {

                var errorMessages = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Ok(new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Validation failed.",
                    Errors = errorMessages
                });
            }
           
            var data = await leaveRequestService.SaveLeaveRequestAsync(model);
            return Ok(data);


        }
        [Route("LeaveRequestUpdatedRoute/UpdateLeaveRequest")]
        [HttpPost]
        public async Task<IActionResult> UpdateLeaveRequest(LeaveApplicationEditVM model)
        {
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(new { success = false, message = "Invalid data." });
            //}
            var data = await leaveRequestService.UpdateLeaveRequestAsynce(model);
            // your update logic here
            return Ok(data);
        }

        #endregion

        #region Get All Data List

        [Route("LeaveRequestRoute/GetAllTableListAsync")]

        [HttpGet]
        public async Task<IActionResult> GetAllTableListAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", int? leaveTypeID = null, int? statusID = null, int? organizationId = null,
    List<int> departmentIds = null,
    List<int> employeeIds = null, DateOnly? fromDate = null, DateOnly? toDate = null)
        {
            try
            {
                 string url = GetEmployeePictureURL();
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var data=await leaveRequestService.GetAllTableAsync(pageNumber, pageSize, searchTerm, currentSortColumn, currentSortOrder , url,userId,leaveTypeID,statusID,organizationId,departmentIds,employeeIds, fromDate, toDate);
                return Json(data);
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);   
                return BadRequest(ex.Message);
             }
        }
        #endregion

        #region  GetBY Data LeaveRequest
        [Route("LeaveRequestRoute/GetLeaveRequestByIdAsync")]
        [HttpGet]
        public async Task<IActionResult> GetLeaveRequestByIdAsync(int leaveApplicationID)
        {
            try
            {
                if (leaveApplicationID == 0) return BadRequest();
                var data = await leaveRequestService.GetLeaveRequestByIdAsync(leaveApplicationID);
                return Json(data);
            }
            catch (Exception)
            {

                throw;
            }
           
        }
        #endregion

        #region Delete Leave Request
        [Route("LeaveRequestRoute/SofteDeleteLeaveRequest")]
        [HttpPost]
        public async Task<IActionResult> SofteDeleteLeaveRequest(DeleteRequestVM deleteRequestVM)
        {
            try
            {
                var data = await leaveRequestService.SoftDeleteLeaveRequest(deleteRequestVM);
                return Json(data);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Json(new { message = ex.Message });
               
            }
        }
        #endregion


        #region Get Leave policy as count or not
        [Route("LeaveRequest/GetLeavePolicyIsCountAsync")]
        [HttpGet]

        public async Task<IActionResult> GetLeavePolicyIsCountAsync()
        {
            try
            {
                var data = await leaveRequestService.GetLeavePolicyIsCountAsync();
                if (data == null)
                {
                    return null;
                }
                return Json(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            
        }
        [HttpGet]
        [Route("LeaveRequest/SubsequentLeaveCount")]
        public async Task<IActionResult> SubsequentLeaveCount(int employeeId, DateTime fromDate,DateTime toDate)
        {
            try
            {
                var data = await leaveRequestService.SubsequentAsynceWithRestriction(employeeId,fromDate, toDate);
                return Json(data);
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region GetEmployeeByDepartment
        [Route("LeaveRequest/GetEmployeeByDepartment")]
        [HttpGet]
        public async Task<JsonResult> GetEmployeeByDepartment(string departmentIds)
        {
            var deptIds = !string.IsNullOrEmpty(departmentIds)
                ? departmentIds.Split(',').Select(int.Parse).ToList()
                : new List<int>();

            var data = await leaveRequestService.GetEmployeeByDepartment(deptIds);
            return Json(data);
        }
        #endregion


        #region GetDepartmentByCompany
        [Route("LeaveRequest/GetDepartmentByCompany")]
        [HttpGet]
        public async Task<JsonResult> GetDepartmentByCompany(int id)
        {
            var data = await leaveRequestService.GetDepartmentByCompany(id);
            return Json(data);
        }
        #endregion


        #region GetEmployeeByCompany
        [Route("LeaveRequest/GetEmployeeByCompany")]
        [HttpGet]
        public async Task<JsonResult> GetEmployeeByCompany(int id)
        {
            var data = await leaveRequestService.GetEmployeeByCompany(id);
            return Json(data);
        }
        #endregion


        #region Display Leave banlance

        [Route("LeaveRequest/GetLeaveTypeBalancesForEmployeeDisplay")]

        [HttpGet]
        public async Task<IActionResult> GetLeaveTypeBalancesForEmployee(int employeeId)
        {

            try
            {

                
                if (employeeId==0)
                 return BadRequest("EmployeeID not found in claims.");


                var data = await leaveRequestService.GetLeaveTypeBalancesForEmployee(employeeId);
                return Json(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }

        }
        #endregion


        #region Display Leave banlance

        [Route("LeaveRequest/GetByPersonLeaveStepVM")]

        [HttpGet]
        public async Task<IActionResult> GetByPersonLeaveStepVM(int leaveApplicationID)
        {

            try
            {


                if (leaveApplicationID == 0)
                    return BadRequest("LeaveApplicationId not found in claims.");


                var data = await leaveRequestService.GetByPersonLeaveStepVM(leaveApplicationID);
                return Json(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }

        }
        #endregion
    }
}
