using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.Employees.EmployeeStatusManagement.PromotionController
{
    public class PromotionController : BaseController
    {
        public PromotionController(ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {
        }

        public IActionResult Index()
        {
            SetSmartPageCode(111100);
            return View();
        }
    }
}
