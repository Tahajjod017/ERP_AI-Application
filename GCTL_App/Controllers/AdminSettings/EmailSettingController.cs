using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers.AdminSettings
{
    public class EmailSettingController : BaseController
    {
        private readonly AppDbContext context;
        public EmailSettingController(ITranslateService translateService, IUserProfileService userProfileService, AppDbContext context) : base(translateService, userProfileService)
        {
            this.context = context;
        }

        public IActionResult Index()
        {
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
                    CreatedBy = 1, 
                    LIP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    LMAC = "", 
                };

                context.EmailSettings.Add(entity);
                context.SaveChanges();

                return Json(new { success = true, message = "Settings saved successfully." });
            }

            return Json(new { success = false, message = "Invalid input. Please try again." });
        }
    }
}
