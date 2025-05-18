using GCTL.Service.Language;
using GCTL_App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace GCTL_App.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        

        public HomeController(ITranslateService translateService, ILogger<HomeController> logger)
            : base(translateService)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var languageCode = HttpContext.Items["Language"] as string ?? "en";
            int pageCode = 612000;

            var ip = GetLocalIP();
           
            ViewBag.welcome = _translateService.GetTranslationInd("Welcome to GCTL Dashboard", (pageCode++).ToString(), languageCode);
           // ViewBag.welcomeNote = _translateService.GetTranslationInd("Your data, your insights, your control. Let’s build something amazing today", (pageCode++).ToString(), languageCode);
            ViewBag.welcomeNote = _translateService.GetTranslationInd("Empowering businesses with smart, scalable, and seamless ERP solutions.", (pageCode++).ToString(), languageCode);
           
            

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
