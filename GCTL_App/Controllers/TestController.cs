using GCTL.Core.ViewModels;
using GCTL.Service.AttendanceManagement;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers
{
    public class TestController : BaseController
    {
        private readonly ILogger<TestController> _logger;
        private readonly IEmailService _emailService;

        public TestController(ITranslateService translateService, IUserProfileService userProfileService, ILogger<TestController> logger, IEmailService emailService) : base(translateService, userProfileService)
        {
            _logger = logger;
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendTestEmail()
        {
            var currentUser = await GetCurrentEmployeeIdAsync();

            var model = new EmailVM
            {
                To = "rhsrakib030@gmail.com", // change this
                Subject = "Test Email",
                Body = "<h3>Hello from ASP.NET Core!</h3><p>This is a test email.</p>"
            };

            try
            {
                await _emailService.SendEmailAsync(model, currentUser);  
                TempData["Message"] = "✅ Email sent successfully!";
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"❌ Failed to send email: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}
