using GCTL.Core.ViewModels.Login;
using GCTL.Data.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;
using GCTL.Core.Repository;
using GCTL.Core.Helpers;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Microsoft.AspNetCore.Authorization;
using GCTL_App.EmailServicesMethod;
using System.Security.Cryptography;
using System.Text;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using GCTL.Core;
using System.Web;
using GCTL.Core.Helpers.LipLmacAddress;
using GCTL_App.Controllers.AttendanceManagement.AttentendceReports.DailyReport;
using System.Data.SqlTypes;

namespace GCTL_App.Controllers
{
    [AllowAnonymous]
    public class AccountController : BaseController
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
       
        private readonly AppDbContext _Db;
        private readonly IEmailService _emailService;
        private readonly IGenericRepository<ActionLogs> actionLogs;

        public AccountController(ITranslateService translateService, IUserProfileService userProfileService, RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, AppDbContext db, IEmailService emailService, IGenericRepository<ActionLogs> actionLogs) : base(translateService, userProfileService)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _Db = db;
            _emailService = emailService;
            this.actionLogs = actionLogs;
        }

        public IActionResult Index()
        {
            return View();
        }
        //[HttpGet]
        //public IActionResult Login() => View();

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var isSuperAdmin = string.Equals(model.Email, "superadmin@gmail.com", StringComparison.OrdinalIgnoreCase);

                //IQueryable<ApplicationUser> baseQuery = _Db.Users
                //    .Include(u => u.Employees)
                //        .ThenInclude(e => e.EmployeeOfficeInfoEmployee);

               

                //var user2 = isSuperAdmin
                //   ? await _Db.Users
                //       .Where(u => u.Email == model.Email)
                //       .FirstOrDefaultAsync()
                //   : await _Db.Users
                //       .Where(u => u.Employees != null && u.Employees.EmployeeOfficeInfoEmployee != null &&
                //           u.Employees.EmployeeOfficeInfoEmployee.Any(x => x.OfficeEmail == model.Email))
                //       .FirstOrDefaultAsync();
                // Call the stored procedure to get user by email
                // Call the stored procedure to get user by email
                var user2 =  _Db.Users
                    .FromSqlRaw("EXEC GetUserByEmail @Email = {0}", model.Email)
                    .AsEnumerable() // Move the query execution to client-side to enable LINQ operations
                    .FirstOrDefault(); // Now we can apply FirstOrDefault here as the result is in-memory


                if (user2 == null)
                {
                    ViewData["ErrorEmailMessage"] = "Email not found.";
                    ModelState.AddModelError("Email", "Email not found.");
                    return View(model);
                }

                // Step 1: Attempt login
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    if (user2.IsPasswordResetRequired == true)
                    {
                        var user3 = await _userManager.FindByEmailAsync(model.Email);
                        var claims2 = new List<Claim>
                                {
                                    new Claim(ClaimTypes.NameIdentifier, user3.Id),
                                    new Claim(ClaimTypes.Name, user3.UserName),
                                    new Claim(ClaimTypes.Email, model.Email)
                                };

                        var claimsIdentity2 = new ClaimsIdentity(claims2, CookieAuthenticationDefaults.AuthenticationScheme);
                        var claimsPrincipal2 = new ClaimsPrincipal(claimsIdentity2);

                        var authProps2 = new AuthenticationProperties
                        {
                            IsPersistent = model.RememberMe,
                            ExpiresUtc = DateTime.UtcNow.AddDays(1)
                        };

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal2, authProps2);

                        // Redirect to force password change
                        return RedirectToAction("ForceChangePassword", "Account");
                    }
                    // Step 2: Get user
                    var user = await _userManager.FindByEmailAsync(model.Email);

                    // Step 3: Get user's roles (by name)
                    var userRoles = await _userManager.GetRolesAsync(user);

                    // Step 4: Convert role names to role IDs
                    var roleIds = await _Db.Roles
                                           .Where(r => userRoles.Contains(r.Name))
                                           .Select(r => r.Id)
                                           .ToListAsync();

                    // Step 5: Fetch the user's permissions using role IDs
                    //var userPermissions = await _Db.RoleModulePermissions
                    //                               .Where(rmp => roleIds.Contains(rmp.RoleId) && rmp.IsGranted)
                    //                               .Include(rmp => rmp.MenuTab)
                    //                               .Include(rmp => rmp.Permission)
                    //                               .Select(rmp => $"{rmp.MenuTab.Title}.{rmp.Permission.Name}")
                    //                               .ToListAsync();
                    // Step 5: Fetch the user's permissions using role IDs via stored procedure
                    var roleIdsString = string.Join(",", roleIds);
                    var userPermissions = await _Db.RoleModulePermissions
                                        .FromSqlRaw("EXEC dbo.sp_GetUserPermissionsByRoleIds @RoleIds = {0}", roleIdsString)
                                        .ToListAsync();
                    // Step 6: Create user claims
                    var claims = new List<Claim>
                                    {
                                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                                        new Claim(ClaimTypes.Email, model.Email),
                                        new Claim(ClaimTypes.Name, user.UserName),
                                    };

                    foreach (var role in userRoles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    foreach (var permission in userPermissions)
                    {
                        claims.Add(new Claim("Permission", permission.ToString()));
                    }

                    // Step 7: Create identity and sign in
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    var authProps = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTime.UtcNow.AddDays(1)
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProps);

                    // Step 8: Log the action (optional)
                    var actiondata = new ActionLogs
                    {
                        CreatedBy = user.EmployeeId,
                        UserEmail = model.Email,
                        ActionName = ActionName.LogIn,
                        LIP = NetworkHelper.GetLocalIP(),
                        LMAC = NetworkHelper.GetMacAddress(),
                        CreatedAt = DateTime.Now
                    };

                    // Uncomment if you're saving logs
                    await _Db.ActionLogs.AddAsync(actiondata);
                    await _Db.SaveChangesAsync();

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl); // Goes to /LeaveApprovalDecline/Index if the user clicked that link
                    }
                    // Step 9: Redirect after successful login
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewData["ErrorPasswordMessage"] = "Incorrect password.";
                    ModelState.AddModelError("Password", "Incorrect password.");
                   // ViewData["ErrorMessage"] = "Invalid login attempt.";
                    return View(model);
                }
            }

            // ModelState invalid
            return View(model);
        }


      
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // SuperAdmin Registration Logic
            // SuperAdmin Registration Logic
            if (model.ForSuperAdmin)
            {
                // Step 1: Check if SuperAdmin role already exists
                var superAdminRoleExists = await _roleManager.RoleExistsAsync("SuperAdmin");
                if (superAdminRoleExists)
                {
                    ModelState.AddModelError("", "SuperAdmin role already exists. You cannot create another SuperAdmin.");
                    return View(model);
                }

                // Step 2: Check if required menus exist
                var targetMenus = await _Db.MenuTab
                    .Where(m => m.Title == "Admin" || m.Title == "Role Permission")
                    .ToListAsync();

                if (targetMenus.Count < 2)
                {
                    ModelState.AddModelError("", "Please add 'Admin' and 'Role Permission' menus before creating SuperAdmin.");
                    return View(model);
                }

                // Step 3: Ensure 'VIEW' permission exists
                var readPermission = await _Db.Permissions.FirstOrDefaultAsync(p => p.Name == "VIEW");
                if (readPermission == null)
                {
                    ModelState.AddModelError("", "The 'VIEW' permission is missing. Please add it to the Permissions table.");
                    return View(model);
                }

                // Step 4: Create the SuperAdmin role
                var createRoleResult = await _roleManager.CreateAsync(new ApplicationRole { Name = "SuperAdmin" });
                if (!createRoleResult.Succeeded)
                {
                    foreach (var error in createRoleResult.Errors)
                        ModelState.AddModelError("", $"Error creating SuperAdmin role: {error.Description}");
                    return View(model);
                }

                // Step 5: Create SuperAdmin user
                var superuser = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email
                };

                var createUserResult = await _userManager.CreateAsync(superuser, model.Password);
                if (!createUserResult.Succeeded)
                {
                    foreach (var error in createUserResult.Errors)
                        ModelState.AddModelError("", error.Description);
                    return View(model);
                }
                
                // Step 6: Assign SuperAdmin role to user
                await _userManager.AddToRoleAsync(superuser, "SuperAdmin");

                // Step 7: Create the Employee record and link to user
                var employee = new GCTL.Data.Models.Employees
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                   
                };

                await _Db.Employees.AddAsync(employee);
                await _Db.SaveChangesAsync();

                // Step 8: Update user with linked EmployeeId
                superuser.EmployeeId = employee.EmployeeID;
                await _userManager.UpdateAsync(superuser);

                // Step 9: Assign default permissions
                var superAdminRole = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");

                foreach (var menu in targetMenus)
                {
                    var exists = await _Db.RoleModulePermissions.AnyAsync(rmp =>
                        rmp.RoleId == superAdminRole.Id &&
                        rmp.MenuTabId == menu.MenuTabId &&
                        rmp.PermissionId == readPermission.Id);

                    if (!exists)
                    {
                        await _Db.RoleModulePermissions.AddAsync(new RoleModulePermissions
                        {
                            RoleId = superAdminRole.Id,
                            MenuTabId = menu.MenuTabId,
                            PermissionId = readPermission.Id,
                            IsGranted = true
                        });
                    }
                }

                await _Db.SaveChangesAsync();

                TempData["AlertType"] = "success";
                TempData["AlertMessage"] = "SuperAdmin created successfully. Please log in.";
                return RedirectToAction("Login", "Account");
            }


            // Standard User Registration
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }

            // Step 1: Check if employee exists by email
            var existingEmployee = await _Db.Employees
                .FirstOrDefaultAsync(e => e.Email == model.Email); // <-- Ensure Employee table has Email field

            if (existingEmployee != null)
            {
                // If employee already exists, assign EmployeeId to user
                user.EmployeeId = existingEmployee.EmployeeID;
            }
            else
            {
                // Create new employee
                var employee = new GCTL.Data.Models.Employees
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email // optional but recommended
                };

                await _Db.Employees.AddAsync(employee);
                await _Db.SaveChangesAsync();

                // Assign new employee to user
                user.EmployeeId = employee.EmployeeID;
            }

            // Update user with the linked employee
            await _userManager.UpdateAsync(user);

            // Optional TempData alert
            TempData["AlertType"] = "success";
            TempData["AlertMessage"] = "User registered successfully. Please log in.";

            return RedirectToAction("Login", "Account");
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Get the current user's email before signing out
            var userEmail = User.Identity.IsAuthenticated ? User.FindFirstValue(ClaimTypes.Email) : "Unknown";

            var user = await _userManager.FindByEmailAsync(userEmail);
            // Log the logout action
            var actiondata = new ActionLogs
            {
                CreatedBy = user?.EmployeeId,
                UserEmail = userEmail,
                ActionName = ActionName.LogOut,
                LIP = NetworkHelper.GetLocalIP(),
                LMAC = NetworkHelper.GetMacAddress(),
                CreatedAt = DateTime.Now
            };
            await actionLogs.AddAsync(actiondata);

            // Sign out from both Identity and Cookie Auth
            await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirect to login page
            return RedirectToAction("Login", "Account");
        }

        public async Task AddPermissionToRole(string roleId, int moduleId, int permissionId, bool isGranted)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existingPermission = await _Db.RoleModulePermissions
                .FirstOrDefaultAsync(rmp => rmp.RoleId == roleId &&
                rmp.MenuTabId == moduleId &&
                                            rmp.PermissionId == permissionId);

            if (existingPermission == null)
            {
                var roleModulePermission = new RoleModulePermissions
                {
                    RoleId = roleId,
                    MenuTabId = moduleId,
                    PermissionId = permissionId,
                    IsGranted = isGranted
                };

                _Db.RoleModulePermissions.Add(roleModulePermission);
            }
            else
            {
                existingPermission.IsGranted = isGranted;
                _Db.RoleModulePermissions.Update(existingPermission);
            }

            await _Db.SaveChangesAsync();

        }

        public async Task AddPermissionsAsClaims(UserManager<IdentityUser> userManager, IdentityUser user)
        {
            var userRoles = await userManager.GetRolesAsync(user);
            var rolePermissions = _Db.RoleModulePermissions
                .Where(rmp => userRoles.Contains(rmp.Role.Name) && rmp.IsGranted)
                .Select(rmp => new Claim("Permission", $"{rmp.MenuTab.Title}_{rmp.Permission.Name}"))
                .ToList();

            foreach (var permission in rolePermissions)
            {
                await userManager.AddClaimAsync(user, permission);
            }
        }

        public async Task<IActionResult> TwoStepVerfication()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }
            // Generate a two-step verification code
            var code = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
            // Send the code to the user's email
            // You can use your email service to send the code here
            return View();
        }
        public async Task<IActionResult> VerifyTwoStepCode(string code)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }
            // Verify the two-step verification code
            var result = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", code);
            if (result)
            {
                // Code is valid, proceed with login
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", "Invalid verification code.");
                return View();
            }
        }
        public async Task<IActionResult> EmailOtpPage()
        {
            return View();
        }
        public async Task<IActionResult> SendOtpToEmail(string email)
        {
            try
            {
                var employee = await _Db.Employees
                    .Where(e => e.EmployeeOfficeInfoEmployee
                        .Any(x => x.OfficeEmail != null && x.OfficeEmail == email))
                    .Select(e => new
                    {
                        e.FirstName,
                        e.LastName
                    })
                    .FirstOrDefaultAsync();

                if (employee == null)
                {
                    TempData["AlertType"] = "danger";
                    TempData["AlertMessage"] = "This email address is not registered. Please check for typos or contact support if needed.";
                    return RedirectToAction("EmailOtpPage", new { email });
                }

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    var firstName = employee?.FirstName ?? "";
                    var lastName = employee?.LastName ?? "";
                    var employeeName = $"{firstName} {lastName}".Trim();

                    if (string.IsNullOrWhiteSpace(employeeName))
                    {
                        employeeName = "Human";
                    }

                    TempData["AlertType"] = "danger";
                    TempData["AlertMessage"] = $"Dear {employeeName}, your email is recognized as an employee account but not registered as a user. Please contact the system administrator for access.";
                    return RedirectToAction("EmailOtpPage", new { email });
                }

                var otp = await _userManager.GenerateUserTokenAsync(
                    user, TokenOptions.DefaultEmailProvider, "ResetPasswordOTP");

                var model = new
                {
                    Name = user.UserName,
                    OTP = otp
                };

                var result = await _emailService.SendEmailAsync(
                    toEmail: email,
                    subject: "Password Reset OTP",
                    razorTemplateFile: "OtpTemplateV2.html",
                    model: model,
                    null,
                    null
                );

                if (!result.Contains("success", StringComparison.OrdinalIgnoreCase))
                {
                    TempData["AlertType"] = "danger";
                    TempData["AlertMessage"] = result;
                    return RedirectToAction("Login");
                }

                TempData["AlertType"] = "success";
                TempData["AlertMessage"] = "OTP has been sent successfully!";
                TempData["Email"] = email;
                return RedirectToAction("TwoStepPage");
            }
            catch (SqlNullValueException)
            {
                TempData["AlertType"] = "danger";
                TempData["AlertMessage"] = "There was a problem retrieving employee data. Please contact support.";
                return RedirectToAction("EmailOtpPage", new { email });
            }
            catch (Exception ex)
            {
                TempData["AlertType"] = "danger";
                TempData["AlertMessage"] = "An unexpected error occurred. Please try again later.";
                // Optionally log: _logger.LogError(ex, "Error in SendOtpToEmail");
                return RedirectToAction("EmailOtpPage", new { email });
            }
        }
        //public async Task<IActionResult> SendOtpToEmail(string email)
        //{

        //    // Corrected the LINQ query to fix the CS0119 error
        //    var employee = await _Db.Employees
        //                   .Include(e => e.EmployeeOfficeInfoEmployee)
        //                   .FirstOrDefaultAsync(e => e.EmployeeOfficeInfoEmployee
        // .Any(x => x.OfficeEmail == email));

        //    if (employee == null)
        //    {
        //        TempData["AlertType"] = "danger";
        //        TempData["AlertMessage"] = "This email address is not registered. Please check for typos or contact support if needed.";

        //        return RedirectToAction("EmailOtpPage", new { email = email });
        //    }

        //    var user = await _userManager.FindByEmailAsync(email);
        //    if (user == null)
        //    {
        //        var firstName = employee.FirstName ?? "";
        //        var lastName = employee.LastName ?? "";
        //        var employeeName = $"{firstName} {lastName}".Trim();

        //        if (string.IsNullOrWhiteSpace(employeeName))
        //        {
        //            employeeName = "Human";
        //        }


        //        TempData["AlertType"] = "danger";
        //        TempData["AlertMessage"] = $"{employeeName}, your email is recognized as an employee account but not registered as a user. Please contact the system administrator for access.";

        //        return RedirectToAction("EmailOtpPage", new { email = email });
        //    }

        //    // Generate OTP token for password reset  
        //    var otp = await _userManager.GenerateUserTokenAsync(
        //        user, TokenOptions.DefaultEmailProvider, "ResetPasswordOTP");

        //    // Prepare data model for Razor template  
        //    var model = new
        //    {
        //        Name = user.UserName, // or use user.FullName if available  
        //        OTP = otp
        //    };

        //    // Use IEmailService to send the email with Razor HTML  
        //    var result = await _emailService.SendEmailAsync(
        //        toEmail: email,
        //        subject: "Password Reset OTP",
        //        razorTemplateFile: "OtpTemplateV2.html",
        //        model: model,
        //        null, // Fix: Move optional parameters to the end  
        //        null
        //    );

        //    // Optionally check if email failed  
        //    if (!result.Contains("success", StringComparison.OrdinalIgnoreCase))
        //    {
        //        TempData["AlertType"] = "danger";
        //        TempData["AlertMessage"] = result;
        //        return RedirectToAction("Login"); // or show error message  
        //    }
        //    TempData["AlertType"] = "success";
        //    TempData["AlertMessage"] = "OTP has been sent successfully!";
        //    TempData["Email"] = email;
        //    return RedirectToAction("TwoStepPage");
        //}

        //public async Task<IActionResult> GetEmployeeCodes()
        //{
        //    int? currentEmployeeId = await GetCurrentEmployeeIdAsync();

        //    // Collect the current user's OrganizationIDs (may be multiple)
        //    var orgIds = new List<int?>();
        //    if (currentEmployeeId.HasValue)
        //    {
        //        orgIds = await _Db.Employees
        //            .Where(e => e.EmployeeID == currentEmployeeId.Value)
        //            .SelectMany(e => e.EmployeeOfficeInfoEmployee
        //                .Where(x => x.OrganizationID != null)
        //                .Select(x => x.OrganizationID))
        //            .Distinct()
        //            .ToListAsync();
        //    }

        //    // Base query: only active employees without users
        //    var query = _Db.Employees
        //        .Where(e => e.DeletedAt == null
        //            && (e.HasUser == null || e.HasUser == false)
        //            && (e.IsActive == true || e.IsActive == null));

        //    // If the current user has one or more orgs, filter by those orgs.
        //    if (orgIds.Any())
        //    {
        //        // If Employee has the same navigation for orgs:
        //        query = query.Where(e =>
        //            e.EmployeeOfficeInfoEmployee.Any(x => orgIds.Contains(x.OrganizationID)));

        //        // If instead there's a direct OrganizationID on Employee, use:
        //        // query = query.Where(e => orgIds.Contains(e.OrganizationID));
        //    }

        //    var employeeCodes = await query
        //        .Select(e => new
        //        {
        //            id = e.EmployeeID,
        //            code = e.EmployeeCode + " - " + (((e.FirstName ?? "") + " " + (e.LastName ?? "")).Trim()),
        //            name = (((e.FirstName ?? "") + " " + (e.LastName ?? "")).Trim())
        //        })
        //        .ToListAsync();

        //    return Ok(employeeCodes);
        //}

        public async Task<IActionResult> GetEmployeeCodes()
        {
            int? currentEmployeeId = await GetCurrentEmployeeIdAsync();

            var userOrgId = await _userManager.Users
                .Where(u => u.EmployeeId == currentEmployeeId)
                .Select(u => u.OrganizationID)
                .FirstOrDefaultAsync();


            // If the current employee has an OrganizationID, filter employees by that OrganizationID
            if (userOrgId != null)
            {
                var employeeCodesQuery = _Db.Employees.Include(x => x.EmployeeOfficeInfoEmployee)
                    .Where(e => e.DeletedAt == null
                                && (e.HasUser != true || e.HasUser == null)
                                && (e.IsActive == true || e.IsActive == null)
                                && e.EmployeeOfficeInfoEmployee.Any(x => x.OfficeEmail != null) // Filter by the same OrganizationID
                                && e.EmployeeOfficeInfoEmployee.Any(x => userOrgId.HasValue)) // Filter by OrganizationID
                    .Select(e => new
                    {
                        id = e.EmployeeID,
                        code = $"{e.EmployeeCode} - {(e.FirstName + " " + e.LastName).Trim()}",
                        name = (e.FirstName + " " + e.LastName).Trim()
                    });

                var employeeCodes = await employeeCodesQuery.ToListAsync();
                return Ok(employeeCodes);
            }
            else
            {
                // If the current employee has no OrganizationID or it's null, return all employees
                var employeeCodes = await _Db.Employees
                    .Where(e => e.DeletedAt == null
                                && (e.HasUser != true || e.HasUser == null)
                                && (e.IsActive == true || e.IsActive == null)
                                && e.EmployeeOfficeInfoEmployee.Any(x => x.OfficeEmail != null)
                                && e.EmployeeOfficeInfoEmployee.Any(x => x.OrganizationID != null)) // Only active employees
                    .Select(e => new
                    {
                        id = e.EmployeeID,
                        code = $"{e.EmployeeCode} - {(e.FirstName + " " + e.LastName).Trim()}",
                        name = (e.FirstName + " " + e.LastName).Trim()
                    })
                    .ToListAsync();

                return Ok(employeeCodes);
            }
        }



        [HttpPost]
        [Route("Account/CreateUsers")]
        public async Task<IActionResult> CreateUsers([FromBody] List<CreateUserRequest> users)
        {
            if (users == null || !users.Any())
                return Json(new { success = false, message = "No user data received." });

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                var userOrgId = await _userManager.Users
                    .Where(u => u.Id == currentUserId)
                    .Select(u => new { u.TenantInfoId, u.OrganizationID })
                    .FirstOrDefaultAsync();
                var tenantRoles = await _Db.UserRoles
                    .Where(ur => ur.UserId == currentUserId)
                    .Select(ur => ur.RoleId)
                    .ToListAsync();

                foreach (var user in users)
                {
                    var pureCode = user.EmployeeCode?.Split(" - ")[0]?.Trim();
                    if (string.IsNullOrEmpty(pureCode))
                    {
                        return Json(new { success = false, message = "Invalid employee code format." });
                    }

                    // Check if employee exists
                    var employee = await _Db.Employees
                        .Include(e => e.EmployeeOfficeInfoEmployee)
                        .Where(e => e.EmployeeCode == pureCode
                            && e.DeletedAt == null
                            && e.EmployeeOfficeInfoEmployee.Any(x => x.OrganizationID != null))
                        .FirstOrDefaultAsync();

                    if (employee == null)
                    {
                        return Json(new { success = false, message = $"Employee not found for code: {pureCode}" });
                    }

                    var organizationId = employee.EmployeeOfficeInfoEmployee.FirstOrDefault()?.OrganizationID;

                    // Construct the role name pattern by excluding the "1_2_" part
                    string roleNamePattern = $"1_{organizationId}_GeneralUser";

                    // Check if the "GeneralUser" role exists for the organization and tenant
                    var generalUserRoleExists = await _Db.ApplicationRoles
                        .AnyAsync(ur => ur.Name.EndsWith("GeneralUser")
                            && ur.OrganizationID == organizationId
                            && ur.TenantInfoId == 1); // TenantInfoId = 1

                    if (!generalUserRoleExists)
                    {
                        return Json(new { success = false, message = "The 'GeneralUser' role does not exist for this organization and Tenant. Please create the role first." });
                    }

                    // Check if user already exists
                    var existingUser = await _Db.Users
                        .FirstOrDefaultAsync(u => u.EmployeeId == employee.EmployeeID);

                    if (existingUser != null)
                    {
                        return Json(new { success = false, message = $"User already exists for employee code: {pureCode}" });
                    }

                    // Generate random password
                    var randomPassword = GeneratePassword8();

                    // Create new user
                    var applicationUser = new ApplicationUser
                    {
                        UserName = employee.EmployeeOfficeInfoEmployee.Where(x=>x.EmployeeID==employee.EmployeeID).Select(x => x.OfficeEmail).FirstOrDefault() ?? (employee.EmployeeCode + "@default.com"),
                        Email = employee.EmployeeOfficeInfoEmployee.Where(x=>x.EmployeeID==employee.EmployeeID).Select(x => x.OfficeEmail).FirstOrDefault() ?? (employee.EmployeeCode + "@default.com"),
                        EmployeeId = employee.EmployeeID,
                        IsPasswordResetRequired = true,
                        DefaultPass = randomPassword,
                        TenantInfoId = 1,
                        OrganizationID = employee.EmployeeOfficeInfoEmployee.FirstOrDefault()?.OrganizationID
                    };

                    var result = await _userManager.CreateAsync(applicationUser, randomPassword);

                    if (result.Succeeded)
                    {
                        // After user is created, assign the "GeneralUser" role
                        await _userManager.AddToRoleAsync(applicationUser, roleNamePattern);

                        employee.HasUser = true;
                        employee.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        var errorMessages = result.Errors.Select(e => e.Description).ToList();
                        return Json(new { success = false, message = $"Failed to create user for {pureCode}", errors = errorMessages });
                    }
                }

                await _Db.SaveChangesAsync();
                return Json(new { success = true, message = "Users and default password created successfully!" });
            }
            catch (Exception ex)
            {
                // Log the exception details for debugging
                // Log.Error(ex, "An error occurred while creating users");

                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        private static string GeneratePassword8()
        {
            const int letterCount = 4;
            const int digitCount = 2;
            const int totalLength = 8;
            var sb = new StringBuilder(totalLength);

            // 4 letters, capitalize first
            for (int i = 0; i < letterCount; i++)
            {
                char c = (char)('a' + RandomNumberGenerator.GetInt32(26));
                if (i == 0) c = char.ToUpperInvariant(c);
                sb.Append(c);
            }

            // 2 digits
            for (int i = 0; i < digitCount; i++)
                sb.Append(RandomNumberGenerator.GetInt32(10)); // 0..9

            // 2 special characters
            sb.Append('%').Append('$');

            return sb.ToString(); // always length 8
        }


        [HttpPost]
        [Route("Account/ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("No user ID provided.");

            // Find the user by ID
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            // Generate password reset token
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            var randomPassword = GeneratePassword8();

            // Reset password to a new default password
            var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, randomPassword);

            if (resetResult.Succeeded)
            {
                //   Mark password as needing reset
                user.IsPasswordResetRequired = true;
                user.DefaultPass = randomPassword;

                // Save changes
                await _userManager.UpdateAsync(user);

                return Ok(new { message = "Password reset successfully!" });
            }
            else
            {
                foreach (var error in resetResult.Errors)
                {
                    // Log errors if needed
                    Console.WriteLine(error.Description);
                }
                return BadRequest("Password reset failed.");
            }
        }
        public async Task<IActionResult> TwoStepPage()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TwoStepPageToEmail(string email, string otp)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["AlertType"] = "danger";
                TempData["AlertMessage"] = "Sorry, this email is not registered in our system.";
                return RedirectToAction("EmailOtpPage", new { email });
            }

            // ✅ Step 1: Verify OTP
            var isValid = await _userManager.VerifyUserTokenAsync(
                user,
                TokenOptions.DefaultEmailProvider,
                "ResetPasswordOTP",
                otp
            );

            if (!isValid)
            {
                TempData["AlertType"] = "danger";
                TempData["AlertMessage"] = "Invalid OTP. Please try again.";
                return RedirectToAction("TwoStepPage");
            }

            // ✅ Step 2: Generate password reset token (not OTP!)
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            TempData["AlertType"] = "success";
            TempData["AlertMessage"] = "OTP verified successfully! You can now reset your password.";
            // ✅ Step 3: Redirect to password reset page with email + reset token
            return RedirectToAction("ResetPasswordPage", new { email = user.Email, token = resetToken });
        }



        [HttpGet]
        public async Task<IActionResult> ResetPasswordPage(string email, string token)
        {
            var model = new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ResetPasswordPage(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Do not reveal whether the email exists
                TempData["AlertType"] = "danger";
                TempData["AlertMessage"] = "Sorry, this email is not registered in our system.";
                return View(model);
                //return RedirectToAction("ResetPasswordConfirmation"); // Or a simple success message
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded)
            {
                // ✅ Send confirmation email
                var emailModel = new
                {
                    Name = user.UserName
                };

                var sendResult = await _emailService.SendEmailAsync(
                    user.Email,
                    "Your Password Was Reset",
                    "PasswordResetConfirmationTemplate.html", // Create this template
                    emailModel,
                    null,
                    null
                );
                // Log the password reset action
                TempData["AlertType"] = "success";
                TempData["AlertMessage"] = "Your password has been reset successfully!";
                return RedirectToAction("Login","Account");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }


        public async Task<IActionResult> LockScreenPage()
        {
            return View();
        }

       
        //

        // Forget Password

        // note for on model creating   Set discriminator value for ApplicationUser
        //modelBuilder.Entity<ApplicationUser>()
        //       .HasDiscriminator<string>("Discriminator")
        //       .HasValue<ApplicationUser>("ApplicationUser");

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ForceChangePassword()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Account/ForceChangePassword")]
        public async Task<IActionResult> ForceChangePassword(ForcePasswordRequest model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["AlertType"] = "danger";
                TempData["AlertMessage"] = "User not found. Please log in again.";
                return RedirectToAction("Login", "Account");
            }
            // Check if the new password is same as the current one
            var passwordHasher = new PasswordHasher<IdentityUser>();
            var verificationResult = _userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, model.NewPassword);

            if (verificationResult == PasswordVerificationResult.Success)
            {
                TempData["AlertType"] = "warning";
                TempData["AlertMessage"] = "You cannot reuse your previous password. Please choose a new one.";
                return View(model);
            }
            // Directly generate a password reset token
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);

            if (result.Succeeded)
            {
                user.IsPasswordResetRequired = false;
                user.DefaultPass = null; // Clear the default password
                await _userManager.UpdateAsync(user);
                TempData["AlertType"] = "success";
                TempData["AlertMessage"] = "Password updated successfully. Please log in again.";

                await _signInManager.SignOutAsync(); // Optional: log user out after password change
                return RedirectToAction("Login", "Account");
            }
            else
            {
                TempData["AlertType"] = "danger";
                TempData["AlertMessage"] = "Password update failed. Please check your inputs.";
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }
        }

    }
}
