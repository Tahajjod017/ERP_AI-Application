using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.PayrollManagements.PayrollPolicy
{
    public class EmployeesSalaryController : BaseController
    {
        public EmployeesSalaryController(ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
