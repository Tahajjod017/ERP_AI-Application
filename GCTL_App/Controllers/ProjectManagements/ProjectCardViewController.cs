using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.ProjectManagements
{
    public class ProjectCardViewController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
