using GCTL.Service.Language;
using GCTL_App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace GCTL_App.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(ITranslateService translateService) : base(translateService) { }

        public IActionResult Index()
        {
            

            SetSmartPageCode(912000);

           

            return View();
        }


        

        public IActionResult Privacy()
        {
            
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

}
