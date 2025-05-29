
using GCTL.Service.Employees.EmployeeEducational;
using GCTL.Service.Language;

using GCTL.Service.Language;
using GCTL.Service.UserProfile;

using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeEducationController : BaseController
    {

        private readonly IEmployeeEducationalService _employeeEducationalService;
        public EmployeeEducationController(ITranslateService translateService, IEmployeeEducationalService employeeEducationalService) : base(translateService)

        public EmployeeEducationController(ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)

        {
            _employeeEducationalService = employeeEducationalService;
        }

        public IActionResult Index(int id)
        {
            SetSmartPageCode(114000);
            return View();
        }
    }
}
