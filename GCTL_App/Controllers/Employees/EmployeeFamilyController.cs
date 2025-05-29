using GCTL.Service.Employees.EmployeeFamily;
using GCTL.Service.Language;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeFamilyController : BaseController
    {
        private readonly IEmployeeFamilyService _employeeFamilyService;
        public EmployeeFamilyController(ITranslateService translateService, IEmployeeFamilyService employeeFamilyService) : base(translateService)
        {
            _employeeFamilyService = employeeFamilyService;
        }

        public IActionResult Index()
        {
            SetSmartPageCode(116000);
            return View();
        }
    }
}
