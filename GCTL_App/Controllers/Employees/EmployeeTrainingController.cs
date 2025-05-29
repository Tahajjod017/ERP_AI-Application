using GCTL.Service.Employees.EmployeeTraining;
using GCTL.Service.Language;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeTrainingController : BaseController
    {
        private readonly IEmployeeTrainingService _employeeTrainingService;
        public EmployeeTrainingController(ITranslateService translateService, IEmployeeTrainingService employeeTrainingService) : base(translateService)
        {
            _employeeTrainingService = employeeTrainingService;
        }

        public IActionResult Index()
        {
            SetSmartPageCode(115000);
            return View();
        }
    }
}
