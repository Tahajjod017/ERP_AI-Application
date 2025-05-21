using GCTL.Service.Language;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeTrainingController : BaseController
    {
        public EmployeeTrainingController(ITranslateService translateService) : base(translateService)
        {
        }

        public IActionResult Index()
        {
            SetPageCode(115000);
            return View();
        }
    }
}
