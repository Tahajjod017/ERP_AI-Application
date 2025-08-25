using GCTL.Core.DataTables;
using GCTL.Service.AdminSettings.GeneralSettings;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using GCTL_App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NodaTime.Text;
using NodaTime;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;

namespace GCTL_App.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        private readonly ILocalizationContext _loc;
        public HomeController(ITranslateService translateService, IUserProfileService userProfileService, ILocalizationContext loc) : base(translateService, userProfileService)
        {
            _loc = loc;
        }

        public IActionResult Index()
        {
           

            SetSmartPageCode(912000);

            return View();
        }


        

        public IActionResult Privacy()
        {
            
            return View();
        }
        public IActionResult AccessDenied()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

}
