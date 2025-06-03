using GCTL.Core.ViewModels.RoleModule;
using GCTL.Data.Models;
using GCTL.Service;
using GCTL.Service.AccessPermissions;
using GCTL.Service.Language;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using GCTL_App.EmailServicesMethod;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GCTL_App.Controllers 
{
   
    public class AccessPermissionController : BaseController
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAccessControlService _accessControlService;
        private readonly AppDbContext _Db;
        private readonly IRoleService _roleService;
        private readonly ITranslateService _translationService;
        private readonly IEmailService _emailService;

        public AccessPermissionController(ITranslateService translateService, IUserProfileService userProfileService, RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IAccessControlService accessControlService, AppDbContext db, IRoleService roleService, ITranslateService translationService, IEmailService emailService) : base(translateService, userProfileService)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _accessControlService = accessControlService;
            _Db = db;
            _roleService = roleService;
            _translationService = translationService;
            _emailService = emailService;
        }


        //[Authorize(Policy = "Admin.VIEW")]
        //[Permission("VIEW", "AccessPermission")]
        public async Task<IActionResult> Index()
        {
            var languageCode = HttpContext.Items["Language"] as string ?? "en";
            int PageCode = 513000;
            int menuTabId = 34;  // Change this to match your actual MenuTabId
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Fetch available roles
            var roles = await _roleManager.Roles
            .Select(r => r.Name)
                .ToListAsync();

            // Check if the user is SuperAdmin
            var isSuperAdmin = await _userManager.IsInRoleAsync(
                await _userManager.FindByIdAsync(currentUserId),
                "SuperAdmin"
            );

            // Fetch all companies and branches if the user is SuperAdmin
            List<ApplicationUser> users;
            List<ApplicationUser> globalUsers = new List<ApplicationUser>();
            var userRoles = new List<UserRoleAssignment>();
            var roleUserAssignments = new Dictionary<string, List<UserRoleAssignment>>();
            if (isSuperAdmin)
            {
                //users = await _Db.AspNetUsers
                //    .Where(u => u.Discriminator == nameof(ApplicationUser))
                //    .Select(u => new ApplicationUser
                //    {
                //        Id = u.Id,
                //        UserName = u.UserName,
                //        Email = u.Email,
                //        FullName = u.FullName,
                //        Gender = u.Gender,
                //        EmployeeId = u.EmployeeId,
                //        NIDNumber = u.NIDNumber,
                //        PresentAddress = u.PresentAddress,
                //        PermanentAddress = u.PermanentAddress,
                //        CreatedDateTime = u.CreatedDateTime,
                //        UpdatedDateTime = u.UpdatedDateTime,
                //        JoiningDate = u.JoiningDate,
                //        DateofBirth = u.DateofBirth
                //    }).ToListAsync();

                users = await _Db.ApplicationUsers.ToListAsync();
                //  Build user role assignments
                //-------------------------------------
                foreach (var user in users)
                {
                    var userRolesAsync = await _userManager.GetRolesAsync(user);
                    userRoles.Add(new UserRoleAssignment
                    {
                        UserId = user.Id,
                        UserName = user.UserName,
                        AvailableRoles = roles,
                        SelectedRole = userRolesAsync.FirstOrDefault(),
                    });
                }
                // Fetch assigned users for each role
                // Fetch assigned users for each role and store their company/branch names

                foreach (var role in roles)
                {
                    var usersInRole = new List<UserRoleAssignment>();

                    foreach (var user in users)
                    {
                        var userRoles2 = await _userManager.GetRolesAsync(user);
                        if (userRoles2.Contains(role))
                        {


                            usersInRole.Add(new UserRoleAssignment
                            {
                                UserId = user.Id,
                                UserName = user.UserName,
                                SelectedRole = role,
                                AssignedRoles = userRoles2.ToList(),

                            });
                        }
                    }

                    roleUserAssignments[role] = usersInRole;
                }
            }
            else
            {
                users = await _Db.ApplicationUsers

                   .ToListAsync();

            }



            // Build and return the view model
            var viewModel = new RoleManagementViewModel
            {
                Users = userRoles,
                RoleUserAssignments = roleUserAssignments
            };
            var companies = _Db.Organization
                         .Select(c => new SelectListItem
                         {
                             Value = c.OrganizationID.ToString(),
                             Text = c.OrganizationName.ToString(),
                         }).ToList();

            ViewBag.CompanyList = companies;


            ViewBag.AvailableRoles = roles;
            ViewBag.CanCreate = await _accessControlService.HasPermissionAsync(menuTabId, "Create");
            ViewBag.CreateNewRl = await _translationService.GetTranslationAsyncInd("Create New Role", (PageCode++).ToString(), languageCode);
            return View(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> GetAvailableRoles(int pageNumber = 1, int pageSize = 5, string searchTerm = "", int? companyId = null, int? tenantId = null)
        {
            var roles = await _roleService.GetPagedRolesAsync(searchTerm, pageNumber, pageSize,companyId,tenantId);
            var totalCount = await _roleService.GetTotalRolesCountAsync(searchTerm);

            var roleDtos = roles.Select(r => new 
            {
                roleId = r.Id,
                roleName = r.Name.ToCleanRoleName() 
            }).ToList();



            return Json(new { data = roleDtos, totalCount });
        }

        [HttpGet]
        public async Task<IActionResult> GetRoleUserAssignments(int pageNumber = 1, int pageSize = 5, string searchTerm = "", int? companyId = null, int? tenantId = null)
        {
            var data = await _roleService.GetPagedRoleUserAssignmentsAsync(searchTerm, pageNumber, pageSize,companyId,tenantId);
            var total = await _roleService.GetTotalRolesCountAsync(searchTerm);

            // Shape data for JSON: dictionary of role => list of users (userName only for now)
            var result = data.ToDictionary(
                entry => entry.Key,
                entry => entry.Value.Select(u => new { u.UserName,u.Id }).ToList()
            );


            return Json(new { data = result, totalCount = total });
        }


        [Permission("CREATE", "AccessPermission")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(RoleManagementViewModel model)
        {
            var normalizedRoleName = model.NewRoleName?.Trim() ?? "";
            var tenantId = 1; // Fixed tenant ID
            var combinedRoleName = $"{tenantId}_{model.SelectedCompanyId}_{normalizedRoleName}";

            var exists = await _roleManager.Roles.AnyAsync(r => r.Name == combinedRoleName);
            if (exists)
            {
                TempData["Error"] = "Role with the same name already exists.";
                return RedirectToAction("Index");
            }

            if (!string.IsNullOrEmpty(normalizedRoleName) && model.SelectedCompanyId.HasValue)
            {
                var role = new ApplicationRole
                {
                    Name = combinedRoleName,
                    OrganizationID = model.SelectedCompanyId,
                    TenantInfoId = tenantId
                };

                var result = await _roleManager.CreateAsync(role);

                if (result.Succeeded)
                {
                    TempData["Message"] = "Role created successfully.";
                }
                else
                {
                    TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
                }
            }
            else
            {
                TempData["Error"] = "Role name, tenant and company must be provided.";
            }

            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<IActionResult> SearchUsers(string query)
        {
            var employees = await _Db.Employees
                .Where(e => e.FirstName.Contains(query) || e.LastName.Contains(query) || e.EmployeeCode.Contains(query))
                .Select(e => new
                {
                    id = e.EmployeeID,
                    userName = e.FirstName + " " + e.LastName,
                    hasUser = e.HasUser,
                    employeeCode = e.EmployeeCode,
                    currentRole = (from user in _Db.Users
                                   join userRole in _Db.UserRoles on user.Id equals userRole.UserId
                                   join role in _Db.Roles on userRole.RoleId equals role.Id
                                   where user.EmployeeId == e.EmployeeID
                                   select role.Name).FirstOrDefault()
                })               
                .ToListAsync();

            return Json(employees);
        }
        [HttpGet]
        //public async Task<IActionResult> CheckUserRole(string userId)
        //{
        //    if (string.IsNullOrEmpty(userId))
        //        return BadRequest("User ID is required.");
        //    // Find userId by EmployeeId
        //    var userIdFromEmp = await _Db.Users
        //        .Where(u => u.EmployeeId.ToString() == userId)
        //        .Select(u => u.Id)
        //        .FirstOrDefaultAsync();
        //    var roleName = await (from user in _Db.Users
        //                          join userRole in _Db.UserRoles on user.Id equals userRole.UserId
        //                          join role in _Db.Roles on userRole.RoleId equals role.Id
        //                          where user.Id == userIdFromEmp
        //                          select role.Name)
        //                         .FirstOrDefaultAsync();

        //    return Json(new { hasRole = !string.IsNullOrEmpty(roleName), currentRole = roleName  });
        //}
        public async Task<IActionResult> CheckUserRole(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("User ID is required.");

            var user = await _Db.Users.FirstOrDefaultAsync(u => u.EmployeeId.ToString() == userId);

            if (user == null)
            {
                return Json(new
                {
                    found = false,
                    message = "No user found. Do you want to create a user account?"
                });
            }

            if (string.IsNullOrEmpty(user.Email))
            {
                return Json(new
                {
                    found = true,
                    hasEmail = false,
                    message = "User found but no email. Please set email first in the Employee module."
                });
            }

            var roleName = await (from userRole in _Db.UserRoles
                                  join role in _Db.Roles on userRole.RoleId equals role.Id
                                  where userRole.UserId == user.Id
                                  select role.Name).FirstOrDefaultAsync();

            return Json(new
            {
                found = true,
                hasEmail = true,
                hasRole = !string.IsNullOrEmpty(roleName),
                currentRole = roleName
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserForEmployee(string employeeId, int? companyId, int? tenantId)

        {
            if (string.IsNullOrEmpty(employeeId))
                return Json(new { success = false, message = "Employee ID is required." });

            var employee = await _Db.Employees.FirstOrDefaultAsync(e => e.EmployeeID.ToString() == employeeId);
            if (employee == null)
                return Json(new { success = false, message = "Employee not found." });

            if (string.IsNullOrEmpty(employee.Email))
                return Json(new { success = false, message = "Email is required to create a user." });

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(employee.Email);
            if (existingUser != null)
                return Json(new { success = false, message = "User already exists for this email." });

            var user = new ApplicationUser
            {
                UserName = employee.Email,
                Email = employee.Email,
                EmailConfirmed = true,
                EmployeeId = employee.EmployeeID,
                TenantInfoId = tenantId,
                OrganizationID = companyId,


            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
                return Json(new { success = false, message = "User creation failed." });

            // Optionally assign default role
            //await _userManager.AddToRoleAsync(user, "Employee");

            // Optionally send a password setup link (reset token)
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action("ResetPasswordPage", "Account", new { token, email = user.Email }, Request.Scheme);

            // Send email here using IEmailSender or your own email service
            // Send email using your service
            // Prepare data model for Razor template  
            var model = new
            {
                Name = user.UserName, // or use user.FullName if available  
                ResetLink = resetLink,
            };

            var emailResult = await _emailService.SendEmailAsync(
                toEmail: employee.Email,
                subject: "Set Your Password",
                razorTemplateFile: "WelcomeAccountSetupTemplate.html", // Rename if needed, e.g., ResetPasswordTemplate.html
                model: model,
                 null,
                 null
            );

            return Json(new { success = true });
        }

        [Permission("CREATE", "AccessPermission")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(string roleId, string role, List<string> selectedUsers)
        {
            if (string.IsNullOrEmpty(role) || selectedUsers == null || !selectedUsers.Any())
            {
                return Json(new { success = false, message = "Invalid input data. Please select both role and users." });
            }
            var roleEntity = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
            if (roleEntity == null)
            {
                return Json(new { success = false, message = "Specified role not found." });
            }
            var roleName = roleEntity.Name;

            foreach (var empIdString in selectedUsers)
            {
                if (!int.TryParse(empIdString, out int employeeId))
                {
                    continue; // Skip invalid employeeId
                }

                // Find userId by EmployeeId
                var userId = await _Db.Users
                    .Where(u => u.EmployeeId == employeeId)
                    .Select(u => u.Id)
                    .FirstOrDefaultAsync();

                ApplicationUser user = null;

                if (string.IsNullOrEmpty(userId))
                {
                    // No user found – try to create one
                    var employee = await _Db.Employees.FirstOrDefaultAsync(e => e.EmployeeID == employeeId && e.DeletedAt == null);

                    if (employee == null)
                        continue;

                    user = new ApplicationUser
                    {
                        UserName = employee.EmployeeCode,
                        Email = employee.Email ?? (employee.EmployeeCode + "@default.com"),
                        EmployeeId = employee.EmployeeID
                    };

                    var createResult = await _userManager.CreateAsync(user, "##Emp123%");
                    if (createResult.Succeeded)
                    {//   Mark password as needing reset
                        //user.IsPasswordResetRequired = true;
                        // Save changes
                        await _userManager.UpdateAsync(user);
                    }
                    else
                    {
                        var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                        return Json(new { success = false, message = $"Failed to create user for Employee ID {employeeId}: {errors}" });

                    }

                    employee.HasUser = true;
                    employee.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    user = await _userManager.FindByIdAsync(userId);
                    if (user == null)
                        continue; // Defensive check
                }

                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Contains(roleName))

                {
                    continue;
                }

                // Enforce one-role-per-user
                if (currentRoles.Any())
                {
                    var removalResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!removalResult.Succeeded)
                    {
                        return Json(new { success = false, message = $"Failed to remove existing roles for {user.UserName}." });
                    }
                }

                var result = await _userManager.AddToRoleAsync(user, roleName);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    return Json(new { success = false, message = $"Failed to assign role to {user.UserName}: {errors}" });
                }
            }

            await _Db.SaveChangesAsync();
            return Json(new { success = true, message = "Role(s) assigned successfully." });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUserFromRole(string roleName, string userName)
        {
            try
            {
                // Assuming you have a method to get the user by username
                var user = await _userManager.FindByNameAsync(userName);

                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Remove the user from the role
                var result = await _userManager.RemoveFromRoleAsync(user, roleName);

                // Check if the removal was successful
                if (result.Succeeded)
                {
                    return Json(new { success = true, message = "User removed from role successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to remove user from role" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> DeleteRoleById(string roleId)
        {
            if (string.IsNullOrWhiteSpace(roleId))
                return BadRequest("Role ID is required.");

            // Find the role by ID
            var role = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
            if (role == null)
                return NotFound("Role not found.");
            // Remove related RoleModulePermissions
            var relatedPermissions = _Db.RoleModulePermissions.Where(r => r.RoleId == role.Id);
            _Db.RoleModulePermissions.RemoveRange(relatedPermissions);
            await _Db.SaveChangesAsync();

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
                return Ok();
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return StatusCode(500, $"Failed to delete role: {errors}");
        }



    }
}
