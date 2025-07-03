using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Core.ViewModels.MasterSetup.BloodGroup;
using GCTL.Service.AdminSettings.OrganizationSettings.HolidayService;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.AdminSettings.CompanySettings
{
    public class HolidaySettingsController : BaseController
    {
        private readonly IHolidaySettingService _holidaySettingService;
        public HolidaySettingsController(ITranslateService translateService, IUserProfileService userProfileService, IHolidaySettingService holidaySettingService) : base(translateService, userProfileService)
        {
            _holidaySettingService = holidaySettingService;
        }

        public async Task<IActionResult> Index()
        {
           
            ViewBag.Organizations = await  _holidaySettingService.GetOrganizationsAsync();
            ViewBag.HolidayStatuses =await  _holidaySettingService.GetHolidayStatusesAsync();

            return View();
        }
        #region Create
        //[Permission("Create", "HolidaySettings")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(HolidayViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _holidaySettingService.IsNameUniqueAsync(model.HolidayTitle);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This name already exists!" });
                    }
                    await _holidaySettingService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.HolidayID });
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
           
                var result = await _holidaySettingService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder, organizationID);
                return Json(result);
           
        }
        #endregion
    }
}
