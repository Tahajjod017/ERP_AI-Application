using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Data.Models;
using GCTL.Service.AdminSettings.OrganizationSettings.WeekendService;
using GCTL.Service.Language;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;



namespace GCTL_App.Controllers.AdminSettings.CompanySettings
{
    [Authorize]
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
        public async Task<IActionResult> GetOrganizations()
        {
            var organizations = await _weekendSettingService.GetOrganizationsAsync();
            return Json(organizations);
        }
        public async Task<IActionResult> GetWeekendDays()
        {
            // Get the list of weekend days from the enum
            var weekendDays = Enum.GetValues(typeof(DayOfWeek)) 
                                .Cast<DayOfWeek>()
                                .Select(d => new SelectListItem
                                {
                                    Value = ((int)d).ToString(), // Values: 0 = Sunday, 1 = Monday, etc.
                                    Text = d.ToString()
                                })
                                .ToList();
            return Json(weekendDays);
        }

        #region Create
        //[Permission("Create", "WeekendSettings")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Creates(WeekendSettingVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _weekendSettingService.IsNameUniqueAsync(model.OrganizationID ?? 0, model.OrganizationBranchID ?? 0);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This Organization and Branch already exists!" });
                    }
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

        #region
        [HttpGet]
        public async Task<IActionResult> GetWeekendSettingById(int id)
        {
            var model = await _weekendSettingService.GetByIdAsync(id);
            if (model == null)
            {
                return NotFound(); // Return a 404 if the entity is not found
            }

            
           

            // Set the WeekendDays dropdown
            var weekendDays = Enum.GetValues(typeof(DayOfWeek))
                                .Cast<DayOfWeek>()
                                .Select(d => new SelectListItem
                                {
                                    Value = ((int)d).ToString(),
                                    Text = d.ToString()
                                })
                                .ToList();

            // Create a response object that will be returned as JSON
            var response = new
            {
                model,
                WeekendDays = weekendDays
            };

            // Return the response as JSON
            return Json(response);
        }


        #endregion

        #region GetAll
        public async Task<IActionResult> GetAlls(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "HolidayTitle", string sortOrder = "desc", int? organizationID = null)
        {

            var result = await _weekendSettingService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder, organizationID);
            return Json(result);

        }
        #endregion

        #region delete 

        [HttpPost]
        public async Task<IActionResult> SoftDelete(DeleteRequestVM requestVM)
        {
            try
            {
                if (requestVM.Ids == null || !requestVM.Ids.Any() || requestVM.Ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No id selected to delete." });
                }
                var result = await _weekendSettingService.SoftDeleteAsync(requestVM);
                if (result == null)
                {
                    return Json(new { isSuccess = false, message = "No id found to delete." });
                }

                return Json(new { isSuccess = true, message = "Deleted Successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        #endregion




    }
}
