using GCTL.Service.Language;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeAdditionalController : BaseController
    {
        public EmployeeAdditionalController(ITranslateService translateService) : base(translateService)
        {
        }

        public IActionResult Index(int id)
        {
            SetSmartPageCode(113000);
            return View();
        }
    }
}
