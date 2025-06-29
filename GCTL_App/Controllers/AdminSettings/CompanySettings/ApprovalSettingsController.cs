using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Service.AdminSettings.OrganizationSettings.ApprovalService;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.AdminSettings.CompanySettings
{
    public class ApprovalSettingsController : BaseController
    {
        private readonly IApprovalSettingService _approvalSettingService;
        public ApprovalSettingsController(ITranslateService translateService, IUserProfileService userProfileService, IApprovalSettingService approvalSettingService) : base(translateService, userProfileService)
        {
            _approvalSettingService = approvalSettingService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Organizations = await _approvalSettingService.GetOrganizationsAsync();
            ViewBag.ApprovalTypes = await _approvalSettingService.GetApprovalTypesAsync();
            return View();
        }

        #region
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(ApprovalSettingsVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _approvalSettingService.IsNameUniqueAsync(model.ApprovalTypeName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This name already exists!" });
                    }
                    await _approvalSettingService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.ApprovalTypeName });
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

        #region Table
        public async Task<IActionResult> GetAllData(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "HolidayID", string sortOrder = "desc", int? organizationID = null)
        {
            var result = await _approvalSettingService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder, organizationID);
            return Json(result);
        }
        #endregion
        public async Task<IActionResult> GetApprovalSettings(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "HolidayID", string sortOrder = "desc", int? organizationID = null)
        {
            var result = await _approvalSettingService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder, organizationID);
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployee()
        {
            var employees = await _approvalSettingService.GetEmployeeAsync();
            return Json(employees);
        }

  
        public async Task<IActionResult> GetDesignation()
        {
            var designations = await _approvalSettingService.GetDesignationAsync();
            return Json(designations);
        }


    }
}
