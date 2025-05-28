using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.AttendanceManagement.ScheduleManagement
{
    public class RosterController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
