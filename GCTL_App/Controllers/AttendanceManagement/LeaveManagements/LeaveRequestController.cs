using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Data.Models;
using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using GCTL_App.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GCTL_App.Controllers.AttendanceManagement.LeaveManagements
{
   
    public class LeaveRequestController : BaseController
    {
        private readonly IGenericRepository<LeaveTypes> leaveType;
        private readonly IGenericRepository<Statuses> status;
        private ILeaveRequestService  leaveRequestService;
     
        public LeaveRequestController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<LeaveTypes> leaveType, IGenericRepository<Statuses> status, ILeaveRequestService leaveRequestService ) : base(translateService, userProfileService)
        {
            this.leaveType = leaveType;
            this.status = status;
            this.leaveRequestService = leaveRequestService;
           
        }

        public IActionResult Index()
        {
            LeaveApplicationsRequestPageVM model = new LeaveApplicationsRequestPageVM
            {
                SetupForm = new LeaveApplicationsRequestVM()
            };


            ViewBag.LeaveTypeDD = new SelectList(leaveType.AllActive(), "LeaveTypeID", "LeaveTypeName");
            ViewBag.StatusDD = new SelectList(status.AllActive(), "StatusID", "StatusName");
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

        [HttpGet]
        [Route("LeaveRequest/GetLeaveDays")]
        public async Task<IActionResult> GetLeaveDays(int leaveTypeId)
        {
            var data = await leaveRequestService.GetLeaveTypeTotaldays(leaveTypeId);
                
                if(data==null)
            {
                return NotFound();
            }
            return Json(data);
        }
        #region  Save Data 

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
        #endregion

        #region Get All Data List

        [Route("LeaveRequestRoute/GetAllTableListAsync")]

        [HttpGet]
        public async Task<IActionResult> GetAllTableListAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", int? leaveTypeID = null, int? statusID = null)
        {
            try
            {
                 string url = GetEmployeePictureURL();
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var data=await leaveRequestService.GetAllTableAsync(pageNumber, pageSize, searchTerm, currentSortColumn, currentSortOrder , url,userId,leaveTypeID,statusID);
                return Json(data);
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);   
                return BadRequest(ex.Message);
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
    }
}
