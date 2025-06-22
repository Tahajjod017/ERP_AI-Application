using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Data.Models;
using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveApprovalDecline;
using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace GCTL_App.Controllers.AttendanceManagement.LeaveManagements
{
    public class LeaveApprovalDeclineController : BaseController
    {
        private readonly ILeaveApprovalService leaveApprovalService;
        private readonly IGenericRepository<LeaveTypes> leaveType;
        private readonly IGenericRepository<Statuses> status;
        private ILeaveRequestService leaveRequestService;
        public LeaveApprovalDeclineController(ITranslateService translateService, IUserProfileService userProfileService, ILeaveApprovalService leaveApprovalService, IGenericRepository<LeaveTypes> leaveType , IGenericRepository<Statuses> status , ILeaveRequestService leaveRequestService) : base(translateService, userProfileService)
        {
            this.leaveApprovalService = leaveApprovalService;
            this.leaveType = leaveType;
            this.status = status;
            this.leaveRequestService = leaveRequestService;
        }



        public IActionResult Index()
        {
            LeaveApplicationEditVM model=new LeaveApplicationEditVM();
            ViewBag.LeaveTypeDD = new SelectList(leaveType.AllActive(), "LeaveTypeID", "LeaveTypeName");
            ViewBag.StatusDD = new SelectList(status.AllActive(), "StatusID", "StatusName");
            return View(model);
        }

        #region Get All Data List

        [Route("LeaveApprovalDeclineRoute/GetAllTableListAsync")]

        [HttpGet]
        public async Task<IActionResult> GetAllTableListAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", int? leaveTypeID = null, int? statusID = null)
        {
            try
            {
                string url = GetEmployeePictureURL();
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var data = await leaveApprovalService.GetAllTableAsync(pageNumber, pageSize, searchTerm, currentSortColumn, currentSortOrder, url, userId, leaveTypeID, statusID);
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
        //[Route("LeaveApprovalDecline/UpdateRequestAsync")]
        [HttpPost]
        public async Task<IActionResult> UpdateRequestAsync(LeaveApplicationEditVM entityVM)
        {
            try
            {
                if (entityVM == null) return BadRequest();
                var data = await leaveRequestService.UpdateLeaveRequestAsynce(entityVM);
                return Json(data);
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

    }
}
