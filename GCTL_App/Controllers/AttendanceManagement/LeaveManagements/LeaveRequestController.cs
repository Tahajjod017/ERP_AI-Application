using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Data.Models;
using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Service.Language;
using GCTL_App.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.AttendanceManagement.LeaveManagements
{
    public class LeaveRequestController : BaseController
    {
        private readonly IGenericRepository<LeaveTypes> leaveType;
        private readonly IGenericRepository<Statuses> status;
        private ILeaveRequestService  leaveRequestService;
        public LeaveRequestController(ITranslateService translateService, IGenericRepository<LeaveTypes> leaveType, IGenericRepository<Statuses> status, ILeaveRequestService leaveRequestService) : base(translateService)
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


            ViewBag.LeaveTypeDD = new SelectList(leaveType.All(), "LeaveTypeID", "LeaveTypeName");
            ViewBag.StatusDD = new SelectList(status.All(), "StatusID", "StatusName");
            //SetSmartPageCode(300);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveLeaveRequest(LeaveApplicationsRequestVM model)
        {
          var data= await leaveRequestService.SaveLeaveRequestAsync(model);
           return Ok(data);
        }
    }
}
