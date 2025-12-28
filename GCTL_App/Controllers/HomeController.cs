using System.Diagnostics;
using GCTL.Core.DataTables;
using GCTL.Core.Repository;
using GCTL.Data.Models;
using GCTL.Service.AdminSettings.GeneralSettings;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using GCTL_App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NodaTime;
using NodaTime.Text;

namespace GCTL_App.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        private readonly ILocalizationContext _loc;
        private readonly IGenericRepository<Organization> _orgaRepository;

        public HomeController(ITranslateService translateService, IUserProfileService userProfileService, ILocalizationContext loc, IGenericRepository<Organization> orgaRepository) : base(translateService, userProfileService)
        {
            _loc = loc;
            _orgaRepository = orgaRepository;
        }

        public async Task<IActionResult> Index()
        {
            var orga = await GetCurrentOrganizationIdAsync();

            ViewBag.OrgaName = _orgaRepository.AllActive().Where(e => e.OrganizationID == orga).Select(e => e.OrganizationName).FirstOrDefault();

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
