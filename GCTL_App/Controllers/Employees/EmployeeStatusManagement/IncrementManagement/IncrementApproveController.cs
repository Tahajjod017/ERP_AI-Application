using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.Employees.EmployeeStatusManagement.IncrementManagement
{
    public class IncrementApproveController : BaseController
    {
        public IncrementApproveController(ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {
        }

        public IActionResult Index()
        {
            SetSmartPageCode(121900);

            return View();
        }
    }
}
