using System.Net.NetworkInformation;
using System.Security.Claims;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium.DevTools.V134.Debugger;

namespace GCTL_App.Controllers
{
    public abstract class BaseController : Controller
    {

        protected readonly IUserProfileService _userProfileService;
        protected readonly ITranslateService _translateService;
        private int _smartPageCode = 0;

        protected BaseController(ITranslateService translateService, IUserProfileService userProfileService)
        {
            _translateService = translateService;
            _userProfileService = userProfileService;
        }

        protected void SetSmartPageCode(int code)
        {
            _smartPageCode = code;
            ViewData["SmartPageCode"] = _smartPageCode;
            ViewData["BaseControllerInstance"] = this;
        }

        protected string SmartLocalizeText(string defaultText)
        {
            string lang = HttpContext.Items["Language"] as string ?? "en";
            return _translateService.GetTranslationInd(defaultText, (_smartPageCode++).ToString(), lang);
        }
        protected void SetUserProfile()
        {
            string fullName = "Guest User";
            string profilePicturePath = "/img/team/72x72/default.webp";

            if (User?.Identity?.IsAuthenticated == true && _userProfileService!=null)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    (fullName, profilePicturePath) = _userProfileService.GetUserProfileAsync(userId);
                }
            }

            ViewData["FullName"] = fullName;
            ViewData["ProfilePicturePath"] = profilePicturePath;
            ViewData["IsCustomPicture"] = !string.IsNullOrEmpty(profilePicturePath)
                             && !profilePicturePath.EndsWith("default.webp", StringComparison.OrdinalIgnoreCase)
                             && !profilePicturePath.EndsWith("No_image_available.svg.png", StringComparison.OrdinalIgnoreCase);

        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetUserProfile();
            base.OnActionExecuting(context);
        }

    }


}
