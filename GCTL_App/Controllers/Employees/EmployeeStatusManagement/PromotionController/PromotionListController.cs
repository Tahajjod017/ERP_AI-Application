using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.Employees.EmployeeStatusManagement.PromotionController
{
    public class PromotionListController : BaseController
    {
        public PromotionListController(ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {
        }

        public IActionResult Index()
        {
            SetSmartPageCode(112100);

            return View();
        }
    }
}
