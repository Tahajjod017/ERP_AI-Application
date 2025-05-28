using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.ProjectManagements
{
    public class ProjectBoardViewController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
