using GCTL.Service.Language;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeAllowanceController : BaseController
    {
        public EmployeeAllowanceController(ITranslateService translateService) : base(translateService)
        {
        }

        public IActionResult Index()
        {
            SetPageCode(119000);
            return View();
        }
    }
}
