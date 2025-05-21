using GCTL.Service.Language;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeBenifitController : BaseController
    {
        public EmployeeBenifitController(ITranslateService translateService) : base(translateService)
        {
        }

        public IActionResult Index()
        {
            SetPageCode(118000);
            return View();
        }
    }
}
