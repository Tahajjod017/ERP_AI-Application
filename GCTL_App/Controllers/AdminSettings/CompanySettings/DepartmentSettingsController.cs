using GCTL.Core.Helpers;
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

        #region Update
        //[Permission("Update", "HolidaySettings")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Update(DepartmentSettingsVM model)
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
                    await _departmentSettingService.UpdateAsync(model);
                    return Json(new { isSuccess = true, message = "Updated Successfully." });
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
        #region GetById
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var department = await _departmentSettingService.GetByIdAsync(id);
                if (department == null)
                {
                    return Json(new { isSuccess = false, message = "Department not found." });
                }
                return Json(new { isSuccess = true, data = department });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        #endregion
        #region GetAll
        [HttpGet]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "OrganizationDepartmentID", string sortOrder = "desc", int? organizationID = null)
        {
            try
            {
                var result = await _departmentSettingService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder, organizationID);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
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
                var result = await _departmentSettingService.SoftDeleteAsync(requestVM);
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
