using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Data.Models;
using GCTL.Service.AdminSettings.SystemSettings.Emailsettingservice;
using GCTL.Service.AdminSettings.SystemSettings.EmailSettingService;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers.AdminSettings
{
    public class EmailSettingController : BaseController
    {
        private readonly AppDbContext context;
        private readonly IEmailSettingService _emailSettingService;
        private readonly IGenericRepository<Organization> _organizationRepository;
        public EmailSettingController(ITranslateService translateService, IUserProfileService userProfileService, AppDbContext context, IEmailSettingService emailSettingService, IGenericRepository<Organization> organizationRepository) : base(translateService, userProfileService)
        {
            this.context = context;
            _emailSettingService = emailSettingService;
            _organizationRepository = organizationRepository;
        }

        public IActionResult Index()
        {
            ViewBag.Organizations = context.Organization
                                .Select(x => new
                                {
                                    Id = x.OrganizationID,
                                    Name = x.OrganizationName
                                }).ToList();
            ViewBag.OrganizationDD = new SelectList(_organizationRepository.AllActive(), "OrganizationID", "OrganizationName");

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Save(EmailSettingsViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (model.OrganizationID == null)
                    {
                        return Json(new { isSuccess = false, message = "Organization Name cannot be Empty!" });
                    }
                    if (model.ServerName == null)
                    {
                        return Json(new { isSuccess = false, message = "ServerName Name cannot be Empty!" });
                    }

                    var uniqueName = await _emailSettingService.IsNameUniqueAsync(model.ServerName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This ServerName already exists!" });
                    }
                   

                    await _emailSettingService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully." });
                }
                var errorMessage = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;

                return Json(new { isSuccess = false, message = errorMessage ?? "Something went wrong." });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> GetById(int id)
        {
            var branch = await _emailSettingService.GetByIdAsync(id);
            if (branch == null)
            {
                return Json(new { isSuccess = false, message = "No record found against this id." });
            }
            return Json(new { isSuccess = true, data = branch });
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Updates(EmailSettingsViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (model.OrganizationID == null)
                    {
                        return Json(new { isSuccess = false, message = "Organization Name cannot be Empty!" });
                    }
                    if (model.ServerName == null)
                    {
                        return Json(new { isSuccess = false, message = "ServerName cannot be Empty!" });
                    }

                    var uniqueName = await _emailSettingService.IsNameUniqueAsync(model.ServerName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This name already exists!" });
                    }
                    await _emailSettingService.UpdateAsync(model);
                    return Json(new { isSuccess = true, message = "Updated Successfully.", lastId = model.ServerName });
                }
                var errorMessage = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
                return Json(new { isSuccess = false, message = errorMessage ?? "Something went wrong." });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }

        #region GetAll
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "EmailSettingID", string sortOrder = "desc")
        {
            var result = await _emailSettingService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

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
                var result = await _emailSettingService.SoftDeleteAsync(requestVM);
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
