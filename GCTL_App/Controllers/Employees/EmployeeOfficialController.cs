using GCTL.Service.Language;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeOfficialController : BaseController
    {
        public EmployeeOfficialController(ITranslateService translateService) : base(translateService)
        {
        }

        public IActionResult Index()
        {
            SetSmartPageCode(112000);
            return View();
        }
    }
}
