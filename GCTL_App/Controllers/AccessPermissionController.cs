using GCTL.Core.ViewModels.RoleModule;
using GCTL.Data.Models;
using GCTL.Service.AccessPermissions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GCTL_App.Controllers
{
    public class AccessPermissionController : Controller
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAccessControlService _accessControlService;
        private readonly AppDbContext _Db;

        public AccessPermissionController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager, AppDbContext db, IAccessControlService accessControlService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _accessControlService = accessControlService;
            _Db = db;
        }

        //[Authorize(Policy = "Admin.VIEW")]
        public async Task<IActionResult> Index()
        {
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
                //        UserEmail = u.UserEmail,
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


            ViewBag.AvailableRoles = roles;
            ViewBag.CanCreate = await _accessControlService.HasPermissionAsync(menuTabId, "Create");

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(RoleManagementViewModel model)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);


            if (!string.IsNullOrEmpty(model.NewRoleName))
            {
                var role = new ApplicationRole { Name = model.NewRoleName };
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
                TempData["Error"] = "Role name cannot be empty.";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> SearchUsers(string query)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var users = await _Db.ApplicationUsers
                .Where(u => u.UserName.Contains(query))
                .Select(u => new { u.Id, u.UserName })
                .Take(10)
                .ToListAsync();

            return Json(users);
        }
        // POST: Assign selected role to a user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(string role, List<string> selectedUsers)
        {

            if (string.IsNullOrEmpty(role) || selectedUsers == null || !selectedUsers.Any())
            {
                return BadRequest("Invalid input data. Ensure all fields are selected.");
            }


            var roleEntity = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == role);
            if (roleEntity == null)
            {
                return NotFound("Role not found.");
            }


            foreach (var userId in selectedUsers)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    continue;
                }


                // if exists currentRole  then remove previous role and add new Role 
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                }

                // var appUser = await _Db.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == userId);



                var result = await _userManager.AddToRoleAsync(user, role);
                if (!result.Succeeded)
                {
                    return BadRequest("Failed to assign role to one or more users.");
                }
            }


            await _Db.SaveChangesAsync();
            return Ok("Role assigned successfully.");
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
    }
}
