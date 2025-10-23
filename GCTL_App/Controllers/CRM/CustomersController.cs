using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.CRM
{
    public class CustomersController : BaseController
    {
        public CustomersController(ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult IndexModal()
        {
            return PartialView("_indexModal");
        }
    }
}
