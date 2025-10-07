using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveApprovalDecline;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveApprovalDecline;
using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using GCTL_App.Controllers.AttendanceManagement.AttentendceReports.DailyReport;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers.OpenUrl
{
    public class LeaveAcceptDenyController : BaseController
    {
        private readonly ILeaveApprovalService leaveApprovalService;
        private readonly AppDbContext appDb;
        private readonly IUserInfoService userInfoService;
        public LeaveAcceptDenyController(ITranslateService translateService, IUserProfileService userProfileService, ILeaveApprovalService leaveApprovalService, AppDbContext appDb, IUserInfoService userInfoService) : base(translateService, userProfileService)
        {
            this.leaveApprovalService = leaveApprovalService;
            this.appDb = appDb;
            this.userInfoService = userInfoService;
        }

        public IActionResult Index()
        {
            return View();
        }
        #region accept/declien from  email 


        private ContentResult BootstrapMessage(string message, string alertType, string iconClass)
        {
            string html = $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <meta charset='utf-8'>
                            <title>Message</title>
                            <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css' rel='stylesheet'>
                            <link href='https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.1/font/bootstrap-icons.css' rel='stylesheet'>
                        </head>
                        <body class='bg-light'>
                            <div class='d-flex justify-content-center align-items-center vh-100'>
                                <div class='text-center'>
                                    <div class='display-1 mb-3 text-{alertType}'>
                                        <i class='bi {iconClass}'></i>
                                    </div>
                                    <div class='alert alert-{alertType} fw-bold fs-5' role='alert'>
                                        {message}
                                    </div>
                                </div>
                            </div>
                        </body>
                        </html>";
            return Content(html, "text/html");
        }

        [HttpGet("LeaveApprovalDeclineRoute/Action")]
        public async Task<IActionResult> LeaveApprovalActionAsync(int leaveId, int approverId, bool isApproved, string secrectCode)
        {
            try
            {
                var data = await leaveApprovalService.GetLeaveRequestByIdAsync(leaveId);
                var employeeId = await appDb.Users
               .Where(u => u.EmployeeId == approverId).Select(e => e.EmployeeId).FirstOrDefaultAsync();
                if (data == null)
                    return BootstrapMessage("Leave request not found.", "danger", "bi-x-circle-fill");

                if (string.IsNullOrEmpty(data.SecrectCode) || data.SecrectCode != secrectCode)
                    return BootstrapMessage("You are not an authorized person to approve this leave.", "danger", "bi-lock-fill");

                if (data.SecrectCodeDateTime.HasValue && data.SecrectCodeDateTime.Value.AddHours(24) < DateTime.UtcNow)
                    return BootstrapMessage("This approval link has expired. You can no longer approve or decline this leave.", "warning", "bi-clock-history");

                if (employeeId == 0)
                    return BootstrapMessage("You are not an authorized person.", "danger", "bi-lock-fill");

                if (leaveId == 0)
                    return BootstrapMessage("Invalid Leave ID.", "danger", "bi-exclamation-triangle-fill");

                if (!data.ApprovalPersonID.HasValue || data.ApprovalPersonID != approverId)
                    return BootstrapMessage("Approver not found.", "danger", "bi-x-circle-fill");

                var entityVM = new LeaveApplicationApprovalModifyVM
                {
                    LeaveApplicationID = leaveId,
                    UpdatedBy = approverId,
                    Approved = isApproved,
                    CreatedBy = approverId,
                    DeletedBy = approverId,
                    EmployeeIDEdit = data.EmployeeIDEdit,
                    LeaveTypeIDEdit = data.LeaveTypeIDEdit,
                    IsFullDayEdit = data.IsFullDayEdit,
                    ReasonEdit = data.ReasonEdit,
                    FromDateEdit = data.FromDateEdit,
                    ToDateEdit = data.ToDateEdit,
                    TotalAppliedDays = (int)data.Period,
                    ApprovalNote = isApproved ? "Approved via email link" : "Declined via email link"
                };

                string url = $"{Request.Scheme}://{Request.Host.Value}";
                var result = await leaveApprovalService.UpdateLeaveRequestAsynce(entityVM, url);

                if (result.Success)
                {
                    // Success messages
                    return BootstrapMessage(
                        isApproved ? "Leave request approved successfully." : "Leave request declined successfully.",
                        isApproved ? "success" : "danger",
                        isApproved ? "bi-check-circle-fill" : "bi-x-circle-fill"
                    );
                }
                else
                {
                    // Operation failed
                    return BootstrapMessage($"Operation failed: {result.Message}", "danger", "bi-x-circle-fill");
                }
            }
            catch (Exception ex)
            {
                await userInfoService.ActionLogExceptionAsync("Leave accept/deny via email", ex, leaveId, ActionName.Error);
                throw;
            }
           

            
          
        }

        #endregion
    }
}
