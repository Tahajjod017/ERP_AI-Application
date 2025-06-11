using GCTL.Service.Employees.EmployeeDetails;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeDetailsController : BaseController
    {
        private readonly IEmployeeDetailsService _employeeDetailsService;
        public EmployeeDetailsController(ITranslateService translateService, IUserProfileService userProfileService, IEmployeeDetailsService employeeDetailsService) : base(translateService, userProfileService)
        {
            _employeeDetailsService = employeeDetailsService;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
