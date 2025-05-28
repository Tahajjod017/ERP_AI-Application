using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.AttendanceManagement.ScheduleManagement
{
    public class AddRosterController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
