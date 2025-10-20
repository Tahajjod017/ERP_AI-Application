using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.CRM
{
    public class CustomersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult IndexModal()
        {
            return View();
        }
    }
}
