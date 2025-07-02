using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Service.AdminSettings.OrganizationSettings.CompanyService;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace GCTL_App.Controllers.AdminSettings.CompanySettings
{
    public class CompanySettingsController : BaseController
    {
        private readonly ICompanySettingService _companySettingService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public CompanySettingsController(ITranslateService translateService, IUserProfileService userProfileService, ICompanySettingService companySettingService, IWebHostEnvironment webHostEnvironment) : base(translateService, userProfileService)
        {
            _companySettingService = companySettingService;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.CountriesDropDown = await _companySettingService.GetCountriesAsync();
            return View();
        }

        #region
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(CompanySettingsVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string? logoFilePath = null;
                    if (model.LogoLinkIform != null)
                    {
                        // Sanitize and validate file name
                        string logoFileName = SanitizeFileName(model.LogoLinkIform.FileName);

                        // Validate file extension
                        if (!IsValidImageExtension(logoFileName))
                        {
                            return Json(new { isSuccess = false, message = "Invalid file type for logo. Only .jpg, .jpeg, .png are allowed." });
                        }

                        // Generate GUID for unique file name
                        string uniqueLogoFileName = Guid.NewGuid().ToString() + Path.GetExtension(logoFileName);

                        // Define the file path
                        logoFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "companyImages", uniqueLogoFileName);

                        // Ensure the directory exists
                        var logoDirectory = Path.GetDirectoryName(logoFilePath);
                        if (!Directory.Exists(logoDirectory))
                        {
                            Directory.CreateDirectory(logoDirectory);  // Create the directory if it doesn't exist
                        }

                        // Save the file
                        using (var stream = new FileStream(logoFilePath, FileMode.Create))
                        {
                            await model.LogoLinkIform.CopyToAsync(stream);
                        }

                        // Store only the file name (GUID-based) in the model
                        model.LogoLink = uniqueLogoFileName;
                    }

                    string? faviconFilePath = null;
                    if (model.FaviconLinkIform != null)
                    {
                        // Sanitize and validate file name
                        string faviconFileName = SanitizeFileName(model.FaviconLinkIform.FileName);

                        // Validate file extension
                        if (!IsValidImageExtension(faviconFileName))
                        {
                            return Json(new { isSuccess = false, message = "Invalid file type for favicon. Only .jpg, .jpeg, .png are allowed." });
                        }

                        // Generate GUID for unique file name
                        string uniqueFaviconFileName = Guid.NewGuid().ToString() + Path.GetExtension(faviconFileName);

                        // Define the file path
                        faviconFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "companyImages", uniqueFaviconFileName);

                        // Ensure the directory exists
                        var faviconDirectory = Path.GetDirectoryName(faviconFilePath);
                        if (!Directory.Exists(faviconDirectory))
                        {
                            Directory.CreateDirectory(faviconDirectory);  // Create the directory if it doesn't exist
                        }

                        // Save the file
                        using (var stream = new FileStream(faviconFilePath, FileMode.Create))
                        {
                            await model.FaviconLinkIform.CopyToAsync(stream);
                        }

                        // Store only the file name (GUID-based) in the model
                        model.FaviconLink = uniqueFaviconFileName;
                    }


                    var uniqueName = await _companySettingService.IsNameUniqueAsync(model.OrganizationName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This approvalType already exists!" });
                    }
                    await _companySettingService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.OrganizationName });
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

        public async Task<IActionResult> GetAllData(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "OrganizationID", string sortOrder = "desc", int? organizationID = null)
        {
            var result = await _companySettingService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder, organizationID);
            return Json(result);
        }
        #endregion
        #region sanitize and validate file names and extensions
        // Method to sanitize file names (remove any special characters or path traversal characters)
        private string SanitizeFileName(string fileName)
        {
            return Regex.Replace(fileName, @"[^a-zA-Z0-9\-_\.]", "_");
        }

        // Method to validate file extension (only allow image types)
        private bool IsValidImageExtension(string fileName)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(fileName)?.ToLower();
            return allowedExtensions.Contains(fileExtension);
        }
        #endregion
    }
}
