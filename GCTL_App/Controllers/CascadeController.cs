using GCTL.Service.CommonService;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers
{
    public class CascadeController : BaseController
    {
        private readonly ICommonService _commonService;

        public CascadeController(ITranslateService translateService, IUserProfileService userProfileService, ICommonService commonService) : base(translateService, userProfileService)
        {
            _commonService = commonService;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
