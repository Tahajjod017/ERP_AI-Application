using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeListController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
