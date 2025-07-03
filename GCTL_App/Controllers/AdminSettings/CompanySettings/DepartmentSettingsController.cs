using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Service.AdminSettings.OrganizationSettings.DepartmentService;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.Department;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.AdminSettings.CompanySettings
{
    public class DepartmentSettingsController : BaseController
    {
        private readonly IDepartmentSettingService _departmentSettingService;
        public DepartmentSettingsController(ITranslateService translateService, IUserProfileService userProfileService, IDepartmentSettingService departmentSettingService) : base(translateService, userProfileService)
        {
            _departmentSettingService = departmentSettingService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Organizations = await _departmentSettingService.GetOrganizationsAsync();
            ViewBag.EmployeeNameWithCode = await _departmentSettingService.GetEmployeeCodeAsync();
            return View();
        }
        #region Create
        //[Permission("Create", "HolidaySettings")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(DepartmentSettingsVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _departmentSettingService.IsNameUniqueAsync(model.DepartmentName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This name already exists!" });
                    }
                    await _departmentSettingService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.DepartmentName });
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
    }
}
