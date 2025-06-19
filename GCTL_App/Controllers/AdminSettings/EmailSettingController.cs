using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Data.Models;
using GCTL.Service.AdminSettings.SystemSettings.Emailsettingservice;
using GCTL.Service.AdminSettings.SystemSettings.EmailSettingService;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers.AdminSettings
{
    public class EmailSettingController : BaseController
    {
        private readonly AppDbContext context;
        private readonly IEmailSettingService _emailSettingService;
        public EmailSettingController(ITranslateService translateService, IUserProfileService userProfileService, AppDbContext context, IEmailSettingService emailSettingService) : base(translateService, userProfileService)
        {
            this.context = context;
            _emailSettingService = emailSettingService;
        }

        public IActionResult Index()
        {
            ViewBag.Organizations = context.Organization
                                .Select(x => new
                                {
                                    Id = x.OrganizationID,
                                    Name = x.OrganizationName
                                }).ToList();

            return View();
        }
        [HttpPost]
        public IActionResult Save(EmailSettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var entity = new EmailSettings
                {
                    OrganizationID = model.OrganizationID,
                    ServerName = model.ServerName,
                    PortNumber = model.PortNumber,
                    IsSSLRequired = model.IsSSLRequired,
                    IsSMTPAuthenticationRequired = model.IsSMTPAuthenticationRequired,
                    PriorityIndex = model.PriorityIndex,
                    UserName = model.UserName,
                    Password = model.Password,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.Now,
                    CreatedBy = 14, 
                    LIP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    LMAC = "", 
                    
                };

                context.EmailSettings.Add(entity);
                context.SaveChanges();

                return Json(new { success = true, message = "Settings saved successfully." });
            }

            return Json(new { success = false, message = "Invalid input. Please try again." });
        }

        #region GetAll
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "EmailSettingID", string sortOrder = "desc")
        {
            var result = await _emailSettingService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion
    }
}
