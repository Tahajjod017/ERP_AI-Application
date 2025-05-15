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

namespace GCTL_NBR.Controllers
{
    public class AccountController : Controller
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext _Db;
        private readonly IGenericRepository<ActionLogs> actionLogs;
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager, AppDbContext db, IGenericRepository<ActionLogs> actionLogs = null)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _Db = db; // Add ApplicationDbContext to the constructor
            this.actionLogs = actionLogs;
        }
        public IActionResult Index()
        {
            return View();
        }
        // Login GET
        [HttpGet]
        public IActionResult Login() => View();


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to sign in the user with the provided credentials
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // Fetch the user from the database
                    var user = await _userManager.FindByEmailAsync(model.Email);

                    // Get the user's roles
                    var userRoles = await _userManager.GetRolesAsync(user);

                    // Create the claims for the authenticated user
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, model.Email),
                new Claim(ClaimTypes.Name, user.UserName),
            };

                    // Add roles to claims
                    foreach (var role in userRoles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    // Fetch the user's permissions from the database
                    var userPermissions = await _Db.RoleModulePermissions
                        .Where(rmp => userRoles.Contains(rmp.RoleId) && rmp.IsGranted) // Check if the user's role has the permission
                        .Include(rmp => rmp.MenuTab)
                        .Include(rmp => rmp.Permission)
                        .Select(rmp => $"{rmp.MenuTab.Title}.{rmp.Permission.Name}")
                        .ToListAsync();

                    // Add permissions to claims
                    foreach (var permission in userPermissions)
                    {
                        claims.Add(new Claim("Permission", permission));
                    }

                    // Create claims identity and principal
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    // Set the authentication properties
                    var authProps = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTime.UtcNow.AddDays(1)
                    };

                    // Sign the user in
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProps);

                    //

                    //
                    var actiondata = new ActionLogs
                    {
                        CreatedBy = user.EmployeeId,
                        UserEmail = model.Email,
                        ActionName = ActionName.LogIn,
                        LIP = GetLocalIP(),
                        LMAC = GetMacAddress(),
                        CreatedAt = DateTime.Now
                    };
                    await actionLogs.AddAsync(actiondata);
                    // Redirect to the home page or dashboard upon successful login
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewData["ErrorMessage"] = "Invalid login attempt.";
                    return View(model); // Return the view with error message
                }
            }

            // If model is invalid or login failed, return to the view with the model to display errors
            return View(model);
        }


        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.ForSuperAdmin)
                {
                    var superAdminExists = (await _userManager.GetUsersInRoleAsync("SuperAdmin")).Any();

                    if (superAdminExists)
                    {
                        ModelState.AddModelError("", "A SuperAdmin user already exists. You cannot create another SuperAdmin.");
                        return View(model);
                    }

                    var superuser = new ApplicationUser
                    {
                        UserName = model.Email,
                        Email = model.Email
                    };

                    // Create the user in the system
                    var createUserResult = await _userManager.CreateAsync(superuser, model.Password);

                    if (createUserResult.Succeeded)
                    {
                        var roleExists = await _roleManager.RoleExistsAsync("SuperAdmin");
                        if (!roleExists)
                        {
                            var createRoleResult = await _roleManager.CreateAsync(new ApplicationRole { Name = "SuperAdmin" });
                            if (!createRoleResult.Succeeded)
                            {
                                foreach (var error in createRoleResult.Errors)
                                {
                                    ModelState.AddModelError("", $"Error creating SuperAdmin role: {error.Description}");
                                }
                                return View(model);
                            }
                        }

                        // Assign the SuperAdmin role to the user
                        var addToRoleResult = await _userManager.AddToRoleAsync(superuser, "SuperAdmin");

                        if (addToRoleResult.Succeeded)
                        {
                            // Log the user in and redirect
                            await _signInManager.SignInAsync(superuser, isPersistent: false);
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            // Handle role assignment errors
                            foreach (var error in addToRoleResult.Errors)
                            {
                                ModelState.AddModelError("", $"Error assigning SuperAdmin role: {error.Description}");
                            }
                        }
                    }
                }
                else
                {
                    var user = new ApplicationUser
                    {
                        UserName = model.Email,
                        Email = model.Email
                    };
                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        // Log the user in and redirect
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
            }

            return View(model);
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
                CreatedBy = user.EmployeeId,
                UserEmail = userEmail,
                ActionName = ActionName.LogOut,
                LIP = GetLocalIP(),
                LMAC = GetMacAddress(),
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

        //Added LIP LMacAddress
        public string GetLocalIP()
        {
            string ipAddress = string.Empty;
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback);

            foreach (var networkInterface in networkInterfaces)
            {
                var properties = networkInterface.GetIPProperties();
                var ipv4Address = properties.UnicastAddresses.FirstOrDefault(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                if (ipv4Address != null)
                {
                    ipAddress = ipv4Address.Address.ToString();
                    break;
                }
            }

            return ipAddress;
        }


        public string GetMacAddress()
        {
            string macAddress = string.Empty;
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
           .Where(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback);
            foreach (var networkInterface in networkInterfaces)
            {
                macAddress = networkInterface.GetPhysicalAddress().ToString();
                if (!string.IsNullOrEmpty(macAddress))
                {
                    break;
                }
            }

            return macAddress;
        }

        //



    }
}
