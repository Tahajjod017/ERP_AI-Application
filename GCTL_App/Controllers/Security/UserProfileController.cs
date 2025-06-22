using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Login;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using GCTL_App.Controllers;
using GCTL_App.ViewModels.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Security.Claims;

namespace GCTL_NBR.Controllers
{
    [Authorize]
    public class UserProfileController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IGenericRepository<EmployeeOfficeInfo> _officialInfoRepository;

        public UserProfileController(ITranslateService translateService, IUserProfileService userProfileService, UserManager<ApplicationUser> userManager = null, SignInManager<ApplicationUser> signInManager = null, IGenericRepository<EmployeeOfficeInfo> officialInfoRepository = null) : base(translateService, userProfileService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _officialInfoRepository = officialInfoRepository;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.Users
                                         .Include(u => u.Employees) // if needed
                                         .ThenInclude(o => o.EmployeeOfficeInfoSeniorSupervisor)
                                         .FirstOrDefaultAsync(u => u.Id == userId);

            var roles = await _userManager.GetRolesAsync(user); // Optional
           
            if (user == null)
            {
                return NotFound();
            }

            var model = new UserProfileViewModel
            {
                FullName = (user.Employees.FirstName + " "+user.Employees.LastName) ?? user.UserName,
                Email = user.Email,
                PhoneNumber = user.Employees.MobileNumber ?? "",
                //Designation = string.Join(", ", user.Employees?.EmployeeOfficeInfoSeniorSupervisor?.Select(x => x.Designation?.DesignationName)) ??"",
                //Department = user.Employees?.EmployeeOfficeInfoSeniorSupervisor?.Select(x => x.Department?.DepartmentName).FirstOrDefault() ?? "",

                Designation = _officialInfoRepository.AllActive().Where(AsadVaiValoAsen => AsadVaiValoAsen.EmployeeID == user.EmployeeId).Include(amrVulHoyese => amrVulHoyese.Designation).Select(seeUnotForMind => seeUnotForMind.Designation.DesignationName).FirstOrDefault() ?? "",

                Department = _officialInfoRepository.AllActive().Where(AsadVaiValoAsen => AsadVaiValoAsen.EmployeeID == user.EmployeeId).Include(amrVulHoyese=>amrVulHoyese.Department).Select(seeUnotForMind => seeUnotForMind.Department.DepartmentName).FirstOrDefault() ?? "",
                Role = string.Join(", ", await _userManager.GetRolesAsync(user)).ToCleanRoleName(),
                EmployeeCode = user.Employees?.EmployeeCode
            };
            ViewBag.FullName = (user.Employees.FirstName + " " + user.Employees.LastName) ?? user.UserName;
            return View(model); // Pass user to view
            //return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(UserProfileViewModel model)
        {
            if (model.PasswordRequest == null)
            {
                return Json(new
                {
                    success = false,
                    fieldErrors = new Dictionary<string, string>
            {
                { "PasswordRequest.OldPassword", "Password data is missing." }
            }
                });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(e => e.Value.Errors.Any())
                    .ToDictionary(
                        e => e.Key,
                        e => e.Value.Errors.First().ErrorMessage
                    );

                return Json(new { success = false, fieldErrors = errors });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new
                {
                    success = false,
                    fieldErrors = new Dictionary<string, string>
            {
                { "PasswordRequest.OldPassword", "User not found or not logged in." }
            }
                });
            }
            //INSERT THIS 
            var verificationResult = _userManager.PasswordHasher
                .VerifyHashedPassword(user, user.PasswordHash, model.PasswordRequest.NewPassword);

            if (verificationResult == PasswordVerificationResult.Success)
            {
                return Json(new
                {
                    success = false,
                    fieldErrors = new Dictionary<string, string>
                    {
                        { "PasswordRequest.NewPassword", "You cannot reuse your previous password. Please choose a different one." }
                    }
                });
            }



            var result = await _userManager.ChangePasswordAsync(user, model.PasswordRequest.OldPassword, model.PasswordRequest.NewPassword);

            if (!result.Succeeded)
            {
                var fieldErrors = new Dictionary<string, string>();

                foreach (var error in result.Errors)
                {
                    if (error.Code.Contains("PasswordMismatch") || error.Code.Contains("InvalidPassword"))
                        fieldErrors["PasswordRequest.OldPassword"] = error.Description;
                    else
                        fieldErrors["PasswordRequest.NewPassword"] = error.Description; // fallback
                }

                return Json(new { success = false, fieldErrors });
            }

            return Json(new { success = true, message = "Password changed successfully." });
        }



    }
}
