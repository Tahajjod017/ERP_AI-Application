using GCTL.Service.Language;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeePersonalController : BaseController
    {
        public EmployeePersonalController(ITranslateService translateService) : base(translateService)
        {
        }

        public IActionResult Index()
        {
            SetPageCode(111000);
            return View();
        }
    }
}
