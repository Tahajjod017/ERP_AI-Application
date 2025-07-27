using GCTL.Core.Enums;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using System.Web.Mvc;

namespace GCTL_App.Controllers.AttendanceManagement.AttentendceReports.MonthlyReport
{
    public class MonthlyReportForAllController : BaseController
    {
        public MonthlyReportForAllController(ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> GetOrganizationId()
        {
           
            return Json(new { });
        }
        public async Task<IActionResult> GetMonthDropdown()
        {


            return Json(new {});
        }
        //public async Task<SelectListItem> GetMonthAsync()
        //{
        //    // Get all months from the enum
        //    var months = Enum.GetValues(typeof(Month))
        //                     .Cast<Month>()
        //                     .Select(m => new SelectListItem
        //                     {
        //                         Text = m.ToString(), // Month name (e.g., January)
        //                         Value = ((int)m).ToString() // Month number (e.g., 1 for January)
        //                     })
        //                     .ToList();

        //    return Json();
        //}

    }
}
