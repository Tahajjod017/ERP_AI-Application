using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.AttendanceManagement.LeaveManagements
{
    public class LeaveApprovalDeclineController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
