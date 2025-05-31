
using GCTL.Service.Employees.EmployeeFamily;
using GCTL.Service.Language;

using GCTL.Service.Language;
using GCTL.Service.UserProfile;

using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeFamilyController : BaseController
    {

        private readonly IEmployeeFamilyService _employeeFamilyService;


        public EmployeeFamilyController(ITranslateService translateService, IUserProfileService userProfileService, IEmployeeFamilyService employeeFamilyService) : base(translateService, userProfileService)

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
