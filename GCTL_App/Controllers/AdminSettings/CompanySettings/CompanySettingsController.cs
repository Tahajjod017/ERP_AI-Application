using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Core.ViewModels.CRM;
using GCTL.Data.Models;
using GCTL.Service.AdminSettings.OrganizationSettings.CompanyService;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics.Metrics;
using System.Text.RegularExpressions;

namespace GCTL_App.Controllers.AdminSettings.CompanySettings
{
    [Authorize]
    public class CompanySettingsController : BaseController
    {
        private readonly ICompanySettingService _companySettingService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IGenericRepository<Country> _genericRepositoryCouyntry;
        public CompanySettingsController(ITranslateService translateService, IUserProfileService userProfileService, ICompanySettingService companySettingService, IWebHostEnvironment webHostEnvironment, IGenericRepository<Country> genericRepositoryCouyntry) : base(translateService, userProfileService)
        {
            _companySettingService = companySettingService;
            _webHostEnvironment = webHostEnvironment;
            _genericRepositoryCouyntry = genericRepositoryCouyntry;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.CountriesDropDown = await _companySettingService.GetCountriesAsync();
            ViewBag.CountryID = new SelectList(_genericRepositoryCouyntry.AllActive(), "CountryID", "CountryName");
            //ViewBag.OrganizationDD = new SelectList(_organizationRepository.AllActive(), "OrganizationID", "OrganizationName");
            return View();
        }

        #region create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(CompanySettingsVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (model.OrganizationName == null)
                    {
                        return Json(new { isSuccess = false, message = "Organization Name cannot be Empty!" });
                    }

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
                        logoFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "media", "company","logo", uniqueLogoFileName);

                        // Ensure the directory exists
                        var logoDirectory = Path.GetDirectoryName(logoFilePath);
                        CreateDirectoryIfNotExists(logoDirectory);

                       

                        // Save the file
                        using (var stream = new FileStream(logoFilePath, FileMode.Create))
                        {
                            await model.LogoLinkIform.CopyToAsync(stream);
                        }
                        string headerLogoDirectory2 = Path.Combine(_webHostEnvironment.WebRootPath, "media", "logo");
                        CreateDirectoryIfNotExists(headerLogoDirectory2);

                        // Define the fixed file name for the header logo (e.g., "logo.png")
                        string headerLogoPath2 = Path.Combine(headerLogoDirectory2, "logo" + Path.GetExtension(logoFileName));

                        // Overwrite existing file if it exists
                        if (System.IO.File.Exists(headerLogoPath2))
                        {
                            System.IO.File.Delete(headerLogoPath2);
                        }

                        // Copy the uploaded logo to the header logo path
                        System.IO.File.Copy(logoFilePath, headerLogoPath2);
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
                        faviconFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "media", "company", "fevicon", uniqueFaviconFileName);

                        // Ensure the directory exists
                        var faviconDirectory = Path.GetDirectoryName(faviconFilePath);
                        CreateDirectoryIfNotExists(faviconDirectory);

                        

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
                        return Json(new { isSuccess = false, message = "This OrganizationName already exists!" });
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

        public async Task<IActionResult> GetById(int id)
        {

            var data = await _companySettingService.GetByIdAsync(id);
            if (data == null)
            {
                return Json(new { isSuccess = false, message = "No data found against this id." });
            }
            //ViewBag.CountriesDropDown = await _companySettingService.GetCountriesAsync();
            return Json(new { isSuccess = true, data = data });
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Updates([FromForm] CompanySettingsVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errorMessage = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
                    return Json(new { isSuccess = false, message = errorMessage ?? "Invalid input." });
                }

                if (string.IsNullOrWhiteSpace(model.OrganizationName))
                    return Json(new { isSuccess = false, message = "Organization Name cannot be Empty!" });

                // Load existing (so we can keep/replace/delete files safely)
                var existing = await _companySettingService.GetByIdAsync(model.OrganizationID);
                if (existing == null)
                    return Json(new { isSuccess = false, message = "Organization not found." });

                // Read remove flags from form (keeps your VM untouched)
                bool removeLogo = bool.TryParse(Request.Form["RemoveLogo"], out var rl) && rl;
                bool removeFavicon = bool.TryParse(Request.Form["RemoveFavicon"], out var rf) && rf;

                // Start from existing names unless changed by actions below
                string currentLogoFileName = existing.LogoLink;
                string currentFaviconFileName = existing.FaviconLink;

                // === LOGO ===
                if (removeLogo && model.LogoLinkIform == null && !string.IsNullOrWhiteSpace(currentLogoFileName))
                {
                    // delete existing without replacement
                    SafeDeleteFile(Path.Combine(_webHostEnvironment.WebRootPath, "media", "company", "logo", currentLogoFileName));
                    var headerDir = Path.Combine(_webHostEnvironment.WebRootPath, "media", "logo");
                    SafeDeleteFile(Path.Combine(headerDir, "logo.png"));
                    SafeDeleteFile(Path.Combine(headerDir, "logo.jpg"));
                    SafeDeleteFile(Path.Combine(headerDir, "logo.jpeg"));
                    currentLogoFileName = null;
                }
                else if (model.LogoLinkIform != null)
                {
                    // Sanitize & validate
                    string logoFileName = SanitizeFileName(model.LogoLinkIform.FileName);
                    if (!IsValidImageExtension(logoFileName))
                        return Json(new { isSuccess = false, message = "Invalid file type for logo. Only .jpg, .jpeg, .png are allowed." });

                    // Save new
                    string newLogoFileName = Guid.NewGuid() + Path.GetExtension(logoFileName);
                    string logoFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "media", "company", "logo", newLogoFileName);
                    CreateDirectoryIfNotExists(Path.GetDirectoryName(logoFilePath));
                    using (var stream = new FileStream(logoFilePath, FileMode.Create))
                        await model.LogoLinkIform.CopyToAsync(stream);

                    // Optional: header copy (/media/logo/logo.ext) and cleanup variants
                    string headerLogoDir = Path.Combine(_webHostEnvironment.WebRootPath, "media", "logo");
                    CreateDirectoryIfNotExists(headerLogoDir);
                    string headerLogoPath = Path.Combine(headerLogoDir, "logo" + Path.GetExtension(logoFileName));
                    if (System.IO.File.Exists(headerLogoPath)) System.IO.File.Delete(headerLogoPath);
                    System.IO.File.Copy(logoFilePath, headerLogoPath);

                    // Delete old physical logo(s)
                    if (!string.IsNullOrWhiteSpace(currentLogoFileName))
                        SafeDeleteFile(Path.Combine(_webHostEnvironment.WebRootPath, "media", "company", "logo", currentLogoFileName));
                    SafeDeleteFile(Path.Combine(headerLogoDir, "logo.png"));
                    SafeDeleteFile(Path.Combine(headerLogoDir, "logo.jpg"));
                    SafeDeleteFile(Path.Combine(headerLogoDir, "logo.jpeg"));

                    currentLogoFileName = newLogoFileName;
                }
                // else: no change → keep currentLogoFileName (from hidden field / existing)

                // === FAVICON ===
                if (removeFavicon && model.FaviconLinkIform == null && !string.IsNullOrWhiteSpace(currentFaviconFileName))
                {
                    SafeDeleteFile(Path.Combine(_webHostEnvironment.WebRootPath, "media", "company", "fevicon", currentFaviconFileName));
                    currentFaviconFileName = null;
                }
                else if (model.FaviconLinkIform != null)
                {
                    string faviconFileName = SanitizeFileName(model.FaviconLinkIform.FileName);
                    if (!IsValidImageExtension(faviconFileName))
                        return Json(new { isSuccess = false, message = "Invalid file type for favicon. Only .jpg, .jpeg, .png are allowed." });

                    string newFaviconFileName = Guid.NewGuid() + Path.GetExtension(faviconFileName);
                    string faviconFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "media", "company", "fevicon", newFaviconFileName);
                    CreateDirectoryIfNotExists(Path.GetDirectoryName(faviconFilePath));
                    using (var stream = new FileStream(faviconFilePath, FileMode.Create))
                        await model.FaviconLinkIform.CopyToAsync(stream);

                    // Delete old physical favicon
                    if (!string.IsNullOrWhiteSpace(currentFaviconFileName))
                        SafeDeleteFile(Path.Combine(_webHostEnvironment.WebRootPath, "media", "company", "fevicon", currentFaviconFileName));

                    currentFaviconFileName = newFaviconFileName;
                }
                // else: no change → keep currentFaviconFileName

                // Push the final filenames into the model that goes to the service
                model.LogoLink = currentLogoFileName;          // may be null, kept, or new
                model.FaviconLink = currentFaviconFileName;    // may be null, kept, or new

                // Now update DB
                var ok = await _companySettingService.UpdateAsync(model);
                return Json(new { isSuccess = ok, message = ok ? "Organization updated successfully." : "Update failed." });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }

        // helper
        private static void SafeDeleteFile(string path)
        {
            try { if (System.IO.File.Exists(path)) System.IO.File.Delete(path); } catch { /* log if you want */ }
        }

        //public async Task<IActionResult> Updates(CompanySettingsVM model)
        //{

        //    try
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            if(model.OrganizationName == null)
        //            {
        //                return Json(new { isSuccess = false, message = "Organization Name cannot be Empty!" });
        //            }

        //            string? logoFilePath = null;
        //            if (model.LogoLinkIform != null)
        //            {
        //                // Sanitize and validate file name
        //                string logoFileName = SanitizeFileName(model.LogoLinkIform.FileName);
        //                // Validate file extension
        //                if (!IsValidImageExtension(logoFileName))
        //                {
        //                    return Json(new { isSuccess = false, message = "Invalid file type for logo. Only .jpg, .jpeg, .png are allowed." });
        //                }
        //                // Generate GUID for unique file name
        //                string uniqueLogoFileName = Guid.NewGuid().ToString() + Path.GetExtension(logoFileName);
        //                // Define the file path
        //                logoFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "media", "company", "logo", uniqueLogoFileName);
        //                // Ensure the directory exists
        //                var logoDirectory = Path.GetDirectoryName(logoFilePath);
        //                CreateDirectoryIfNotExists(logoDirectory);

        //                // Save the file
        //                using (var stream = new FileStream(logoFilePath, FileMode.Create))
        //                {
        //                    await model.LogoLinkIform.CopyToAsync(stream);
        //                }
        //                // Store only the file name (GUID-based) in the model
        //                model.LogoLink = uniqueLogoFileName;
        //            }
        //            string? faviconFilePath = null;
        //            if (model.FaviconLinkIform != null)
        //            {
        //                // Sanitize and validate file name
        //                string faviconFileName = SanitizeFileName(model.FaviconLinkIform.FileName);
        //                // Validate file extension
        //                if (!IsValidImageExtension(faviconFileName))
        //                {
        //                    return Json(new { isSuccess = false, message = "Invalid file type for favicon. Only .jpg, .jpeg, .png are allowed." });
        //                }
        //                // Generate GUID for unique file name
        //                string uniqueFaviconFileName = Guid.NewGuid().ToString() + Path.GetExtension(faviconFileName);
        //                // Define the file path
        //                faviconFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "media", "company", "fevicon", uniqueFaviconFileName);
        //                // Ensure the directory exists
        //                var faviconDirectory = Path.GetDirectoryName(faviconFilePath);
        //                CreateDirectoryIfNotExists(faviconDirectory);
        //                // Save the file
        //                using (var stream = new FileStream(faviconFilePath, FileMode.Create))
        //                {
        //                    await model.FaviconLinkIform.CopyToAsync(stream);
        //                }
        //                // Store only the file name (GUID-based) in the model
        //                model.FaviconLink = uniqueFaviconFileName;
        //            }


        //            var result = await _companySettingService.UpdateAsync(model);
        //            // Return success response
        //            return Json(new { isSuccess = true, message = "Organization updated successfully." });
        //        }
        //        // Get the first error message if model state is invalid
        //        var errorMessage = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
        //        // Return failure response with the error message
        //        return Json(new { isSuccess = false, message = errorMessage ?? "Something went wrong." });
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log exception here if needed
        //        // Return failure response with exception message
        //        return Json(new { isSuccess = false, message = ex.Message });
        //    }
        //}

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
                var result = await _companySettingService.SoftDeleteAsync(requestVM);
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

        #region Helper function to create directories if they do not exist
        private void CreateDirectoryIfNotExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }
        #endregion

    }
}
