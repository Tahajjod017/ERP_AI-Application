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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private int _smartPageCode = 0;

        
        protected string imgSrc => GetBaseUrl() + "/media/employee/images/";
        protected string imgSrcThumb => GetBaseUrl() + "/media/employee/images/thumbs/";
       


        protected BaseController(ITranslateService translateService, IUserProfileService userProfileService)
        {
            _translateService = translateService;
            _userProfileService = userProfileService;
            _httpContextAccessor = new HttpContextAccessor(); // Instantiated internally
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

            var url = _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host + "/media/employee/images/";
            


            ViewData["FullName"] = fullName;
            ViewData["ProfilePicturePath"] = url  + profilePicturePath;
            ViewData["IsCustomPicture"] = !string.IsNullOrEmpty(profilePicturePath)
                             && !profilePicturePath.EndsWith("default.webp", StringComparison.OrdinalIgnoreCase)
                             && !profilePicturePath.EndsWith("No_image_available.svg.png", StringComparison.OrdinalIgnoreCase);

        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetUserProfile();
            base.OnActionExecuting(context);
        }

        public IActionResult GetLocalHost()
        {
            var request = _httpContextAccessor.HttpContext.Request;
            string url = $"{request.Scheme}://{request.Host}";

            return Ok(new { LocalHostUrl = url });
        }
        
        public string GetLocalHostStr()
        {
            var request = _httpContextAccessor.HttpContext.Request;
            string url = $"{request.Scheme}://{request.Host}";

            return url;
        }

        public string GetEmployeePictureURL(bool thumb = false)
        {
            if (thumb)
            {
                return _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host + "/media/employee/images/thumbs/";

            }

            return _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host + "/media/employee/images/";
        }

        // Helper method to get base URL
        private string GetBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null) return string.Empty;

            return $"{request.Scheme}://{request.Host}";
        }
        // New method to get the current EmployeeID
        protected async Task<int?> GetCurrentEmployeeIdAsync()
        {
            int? currentEmployeeId = null;

            if (User?.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    // Fetch current employee ID using IUserProfileService
                    currentEmployeeId = await _userProfileService.GetCurrentEmployeeIdAsync(userId);
                }
            }

            return currentEmployeeId;
        }
        protected async Task<int?> GetCurrentOrganizationIdAsync()
        {
            int? currentOrganizationId = null;
            if (User?.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    // Fetch current organization ID using IUserProfileService
                    currentOrganizationId = await _userProfileService.GetCurrentOrganizationIdAsync(userId);
                }
            }
            return currentOrganizationId;
        }

    }


}
