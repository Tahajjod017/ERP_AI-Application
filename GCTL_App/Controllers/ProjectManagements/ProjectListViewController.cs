using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.ProjectManagements
{
    public class ProjectListViewController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
