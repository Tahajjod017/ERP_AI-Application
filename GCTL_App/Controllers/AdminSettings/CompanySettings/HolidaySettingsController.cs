using GCTL.Core.DataTables;
using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Core.ViewModels.MasterSetup.BloodGroup;
using GCTL.Data.Models;
using GCTL.Service.AdminSettings.OrganizationSettings.HolidayService;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.AdminSettings.CompanySettings
{
    [Authorize]
    public class HolidaySettingsController : BaseController
    {
        private readonly IHolidaySettingService _holidaySettingService;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IGenericRepository<Statuses> _statusRepository;
        public HolidaySettingsController(ITranslateService translateService, IUserProfileService userProfileService, IHolidaySettingService holidaySettingService, IGenericRepository<Organization> organizationRepository, IGenericRepository<Statuses> statusRepository) : base(translateService, userProfileService)
        {
            _holidaySettingService = holidaySettingService;
            _organizationRepository = organizationRepository;
            _statusRepository = statusRepository;
        }

        public async Task<IActionResult> Index()
        {

            ViewBag.OrganizationDD = new SelectList(_organizationRepository.AllActive(), "OrganizationID", "OrganizationName");
            ViewBag.StatusDD = new SelectList(_statusRepository.AllActive().Where(x=>x.StatusType== "Active/Inactive"), "StatusID", "StatusName");
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
                // Custom ordered validation message 
                var orderedKeys = new[] { "OrganizationID", "HolidayTitle", "StartDate", "EndDate", "TotalDays", "StatusID" };

                foreach (var key in orderedKeys)
                {
                    if (ModelState.TryGetValue(key, out var entry) && entry.Errors.Any())
                    {
                        return Json(new { isSuccess = false, field = key, message = entry.Errors.First().ErrorMessage });
                    }
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

        #region getbyId
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _holidaySettingService.GetByIdAsync(id);
                if (result == null)
                {
                    return Json(new { isSuccess = false, message = "No record found." });
                }
                return Json(new { isSuccess = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        #endregion

        #region updates
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Updates(HolidayViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //var uniqueName = await _holidaySettingService.IsNameUniqueAsync(model.HolidayTitle, model.HolidayID);
                    //if (!uniqueName)
                    //{
                    //    return Json(new { isSuccess = false, message = "This name already exists!" });
                    //}
                    var result = await _holidaySettingService.UpdateAsync(model);
                    if (result == null)
                    {
                        return Json(new { isSuccess = false, message = "No record found to update." });
                    }
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

        #region GetAll
        public async Task<IActionResult> GetAlls(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "HolidayTitle", string sortOrder = "desc", int? organizationID = null)
        {
           
                var result = await _holidaySettingService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder, organizationID);
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
                var result = await _holidaySettingService.SoftDeleteAsync(requestVM);
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
