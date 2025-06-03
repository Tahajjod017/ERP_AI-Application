using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.AdminSetup
{
    public class LayoutManagementController : BaseController
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public LayoutManagementController(ITranslateService translateService, 
            IUserProfileService userProfileService, 
            IWebHostEnvironment webHostEnvironment) : base(translateService, userProfileService)
        {
            _webHostEnvironment = webHostEnvironment;
        }


        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("file", "Please select a file.");
                return View();
            }

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
            {
                ModelState.AddModelError("file", "Please select a valid image file.");
                return View();
            }

            if (file.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError("file", "File size cannot exceed 5MB.");
                return View();
            }

            var webRootPath = _webHostEnvironment.WebRootPath;
            var logoFolderPath = Path.Combine(webRootPath, "media", "logo");
            var logoFilePath = Path.Combine(logoFolderPath, "logo.png");

            if (!Directory.Exists(logoFolderPath))
            {
                Directory.CreateDirectory(logoFolderPath);
            }

            using (var stream = new FileStream(logoFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return RedirectToAction("Index");
        }
    }
}
