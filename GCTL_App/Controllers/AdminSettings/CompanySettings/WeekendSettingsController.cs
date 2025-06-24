using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Service.AdminSettings.OrganizationSettings.WeekendService;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;



namespace GCTL_App.Controllers.AdminSettings.CompanySettings
{
    public class WeekendSettingsController : BaseController
    {
        private readonly IWeekendSettingService _weekendSettingService;
        public WeekendSettingsController(ITranslateService translateService, IUserProfileService userProfileService, IWeekendSettingService weekendSettingService) : base(translateService, userProfileService)
        {
            _weekendSettingService = weekendSettingService;
        }

        public async Task<IActionResult> Index()
        {
            //var organizations = await _weekendSettingService.GetOrganizationsAsync();
            ViewBag.Organizations = await _weekendSettingService.GetOrganizationsAsync(); ;
            ViewBag.Branches = new List<SelectListItem>(); // initially empty

            //ViewBag.WeekendDays = new List<SelectListItem>
            //                    {
            //                        new SelectListItem { Value = "0", Text = "Monday" },
            //                        new SelectListItem { Value = "1", Text = "Tuesday" },
            //                        new SelectListItem { Value = "2", Text = "Wednesday" },
            //                        new SelectListItem { Value = "3", Text = "Thursday" },
            //                        new SelectListItem { Value = "4", Text = "Friday" },
            //                        new SelectListItem { Value = "5", Text = "Saturday" },
            //                        new SelectListItem { Value = "6", Text = "Sunday" }
            //                    };
            ViewBag.WeekendDays = Enum.GetValues(typeof(DayOfWeek))
                                .Cast<DayOfWeek>()
                                .Select(d => new SelectListItem
                                {
                                    Value = ((int)d).ToString(), // Values: 0 = Sunday, 1 = Monday, etc.
                                    Text = d.ToString()
                                })
                                .ToList();

            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetBranches(int organizationId)
        {
            var branches = await _weekendSettingService.GetBranchesByOrganizationIdAsync(organizationId);
            return Json(branches);
        }

        #region Create
        //[Permission("Create", "HolidaySettings")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(WeekendSettingVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                   
                    await _weekendSettingService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.WeekendSettingID });
                }
                var errorMessage = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;

                return Json(new { isSuccess = false, message = errorMessage ?? "Something went wrong." });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        #endregion

        #region GetAll
        public async Task<IActionResult> GetAlls(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "HolidayTitle", string sortOrder = "desc", int? organizationID = null)
        {

            var result = await _weekendSettingService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder, organizationID);
            return Json(result);

        }
        #endregion



    }
}
