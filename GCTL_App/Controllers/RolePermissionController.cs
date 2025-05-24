using GCTL.Core.ViewModels.RoleModule;
using GCTL.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GCTL_App.Controllers
{
    public class RolePermissionController : Controller
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext _Db;

        public RolePermissionController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager, AppDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _Db = db;

        }
        public async Task<IActionResult> Index()
        {// Get current user ID
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Load roles into ViewBag (No tenant check here)
            ViewBag.Roles = await _Db.Roles
                .Select(r => new { r.Id, r.Name })
                .ToListAsync();

            return View("Index");
        }


        [HttpGet]
        public async Task<IActionResult> LoadPermissions(string roleId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Load all modules (no tenant filtering)
            var modules = await _Db.MenuTab.ToListAsync();

            var primaryModules = modules
                .Where(m => m.Type == "Primary")
                .Select(pm => new PrimaryModuleViewModel
                {
                    Id = pm.MenuTabId,
                    Name = pm.Title,
                    CanCreate = GetPermissionStatus(roleId, pm.MenuTabId, "CREATE"),
                    CanView = GetPermissionStatus(roleId, pm.MenuTabId, "VIEW"),
                    CanEdit = GetPermissionStatus(roleId, pm.MenuTabId, "EDIT"),
                    CanDelete = GetPermissionStatus(roleId, pm.MenuTabId, "DELETE"),
                    CanExport = GetPermissionStatus(roleId, pm.MenuTabId, "EXPORT"),
                    CanDownload = GetPermissionStatus(roleId, pm.MenuTabId, "DOWNLOAD"),
                    SecondaryModules = modules
                        .Where(sm => sm.ParentId == pm.MenuTabId && sm.Type == "Secondary")
                        .Select(sm => new SecondaryModuleViewModel
                        {
                            Id = sm.MenuTabId,
                            Name = sm.Title,
                            CanCreate = GetPermissionStatus(roleId, sm.MenuTabId, "CREATE"),
                            CanView = GetPermissionStatus(roleId, sm.MenuTabId, "VIEW"),
                            CanEdit = GetPermissionStatus(roleId, sm.MenuTabId, "EDIT"),
                            CanDelete = GetPermissionStatus(roleId, sm.MenuTabId, "DELETE"),
                            CanExport = GetPermissionStatus(roleId, pm.MenuTabId, "EXPORT"),
                            CanDownload = GetPermissionStatus(roleId, pm.MenuTabId, "DOWNLOAD"),
                            TertiaryModules = modules
                                .Where(tm => tm.ParentId == sm.MenuTabId && tm.Type == "Tertiary")
                                .Select(tm => new TertiaryModuleViewModel
                                {
                                    Id = tm.MenuTabId,
                                    Name = tm.Title,
                                    CanCreate = GetPermissionStatus(roleId, tm.MenuTabId, "CREATE"),
                                    CanView = GetPermissionStatus(roleId, tm.MenuTabId, "VIEW"),
                                    CanEdit = GetPermissionStatus(roleId, tm.MenuTabId, "EDIT"),
                                    CanDelete = GetPermissionStatus(roleId, tm.MenuTabId, "DELETE"),
                                    CanExport = GetPermissionStatus(roleId, pm.MenuTabId, "EXPORT"),
                                    CanDownload = GetPermissionStatus(roleId, pm.MenuTabId, "DOWNLOAD"),
                                    QuaternaryModules = modules
                                        .Where(qm => qm.ParentId == tm.MenuTabId && qm.Type == "Quaternary")
                                        .Select(qm => new QuaternaryModuleViewModel
                                        {
                                            Id = qm.MenuTabId,
                                            Name = qm.Title,
                                            CanCreate = GetPermissionStatus(roleId, qm.MenuTabId, "CREATE"),
                                            CanView = GetPermissionStatus(roleId, qm.MenuTabId, "VIEW"),
                                            CanEdit = GetPermissionStatus(roleId, qm.MenuTabId, "EDIT"),
                                            CanDelete = GetPermissionStatus(roleId, qm.MenuTabId, "DELETE"),
                                            CanExport = GetPermissionStatus(roleId, pm.MenuTabId, "EXPORT"),
                                            CanDownload = GetPermissionStatus(roleId, pm.MenuTabId, "DOWNLOAD"),
                                            QuinaryModules = modules
                                                .Where(qn => qn.ParentId == qm.MenuTabId && qn.Type == "Quinary")
                                                .Select(qn => new QuinaryModuleViewModel
                                                {
                                                    Id = qn.MenuTabId,
                                                    Name = qn.Title,
                                                    CanCreate = GetPermissionStatus(roleId, qn.MenuTabId, "CREATE"),
                                                    CanView = GetPermissionStatus(roleId, qn.MenuTabId, "VIEW"),
                                                    CanEdit = GetPermissionStatus(roleId, qn.MenuTabId, "EDIT"),
                                                    CanDelete = GetPermissionStatus(roleId, qn.MenuTabId, "DELETE"),
                                                    CanExport = GetPermissionStatus(roleId, pm.MenuTabId, "EXPORT"),
                                                    CanDownload = GetPermissionStatus(roleId, pm.MenuTabId, "DOWNLOAD")
                                                }).ToList()
                                        }).ToList()
                                }).ToList()
                        }).ToList()
                }).ToList();

            var viewModel = new PermissionViewModel
            {
                PrimaryModules = primaryModules
            };

            return PartialView("_PermissionTablePartial", viewModel);
        }


        private bool GetPermissionStatus(string roleId, int moduleId, string permission)
        {
            // Check permissions for the given role and module (No tenant logic)
            var modulePermission = _Db.RoleModulePermissions
                .FirstOrDefault(p => p.RoleId == roleId && p.MenuTabId == moduleId && p.Permission.Name == permission);

            if (modulePermission != null)
            {
                return modulePermission.IsGranted;
            }
            // If no direct permission found, attempt to inherit from the parent module
            var parentModuleId = _Db.MenuTab
                .Where(m => m.MenuTabId == moduleId)
                .Select(m => m.ParentId)
                .FirstOrDefault();

            // Default to false if no permission exists
            return false;
        }



        [HttpPost]
        public async Task<IActionResult> UpdatePermissions([FromBody] BulkPermissionUpdateModel model)
        {
            // Check if the incoming model is valid
            if (model == null || string.IsNullOrEmpty(model.RoleId) || model.Permissions == null || !model.Permissions.Any())
            {
                return BadRequest("Invalid data.");
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get distinct permission names from the incoming permission model
            var permissionNames = model.Permissions.Select(p => p.Permission).Distinct().ToList();

            // Get the permission IDs from the database based on the permission names
            // List to store the PermissionIds based on the provided permission names
            List<int> permissionIds = new List<int>();

            // Loop through each permission in the request model and fetch its PermissionId
            foreach (var perm in model.Permissions)
            {
                // Fetch the PermissionId from the Permissions table based on the permission name
                var permission = await _Db.Permissions
                     .FirstOrDefaultAsync(p => p.Name == perm.Permission);

                if (permission == null)
                {
                    // Handle the case where the permission doesn't exist
                    return BadRequest($"Permission '{perm.Permission}' not found in the database.");
                }

                // Add the PermissionId to the list
                permissionIds.Add(permission.Id);
            }



            // Get distinct module IDs from the incoming permissions
            var moduleIds = model.Permissions.Select(p => p.MenuTabId).Distinct().ToList();

            // Retrieve existing permissions for the given role and modules
            // Retrieve existing permissions for the given role, modules, and permission types (by PermissionId)
            var existingPermissions = await _Db.RoleModulePermissions
                 .Where(p => p.RoleId == model.RoleId && moduleIds.Contains(p.MenuTabId) && permissionIds.Contains(p.PermissionId))
                 .ToListAsync();

            // List to hold new permissions that are not yet added to the database
            var newPermissions = new List<RoleModulePermissions>();

            // Loop through the permissions to either update existing or add new
            foreach (var perm in model.Permissions)
            {
                // Get the PermissionId for the current permission
                var permissionId = permissionIds.First(id => _Db.Permissions.Any(p => p.Id == id && p.Name == perm.Permission));

                // Check if the permission already exists based on RoleId, ModuleId, and PermissionId
                var existing = existingPermissions.FirstOrDefault(p =>
                     p.RoleId == model.RoleId &&
                     p.MenuTabId == perm.MenuTabId &&
                     p.PermissionId == permissionId);

                if (existing != null)
                {
                    // Update existing permission
                    existing.IsGranted = perm.IsGranted;
                    _Db.Update(existing);
                }
                else
                {


                    // Now create the new RoleModulePermission
                    newPermissions.Add(new RoleModulePermissions
                    {
                        RoleId = model.RoleId,
                        MenuTabId = perm.MenuTabId,
                        PermissionId = permissionId, // Use permission ID
                        IsGranted = perm.IsGranted
                    });
                }
            }

            // Add new permissions to the database if any
            if (newPermissions.Any())
            {
                await _Db.RoleModulePermissions.AddRangeAsync(newPermissions);
            }

            // Save changes to the database
            await _Db.SaveChangesAsync();
            return RedirectToAction("Index", "RolePermission");
            // return Ok(new { message = "Permissions updated successfully" });
        }


        [HttpGet]
        public async Task<IActionResult> GetUserNavModules()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get the role of the logged-in user
            var roleId = await _Db.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(roleId))
            {
                return BadRequest("Role not found.");
            }

            // Get View permission ID
            var viewPermissionId = await _Db.Permissions
                .Where(p => p.Name == "View")
                .Select(p => p.Id)
                .FirstOrDefaultAsync();

            if (viewPermissionId == 0)
            {
                return BadRequest("View permission not found.");
            }

            // Fetch allowed modules based on View permission
            var modules = await _Db.MenuTab
                .Where(m => _Db.RoleModulePermissions
                    .Any(rmp => rmp.RoleId == roleId && rmp.MenuTabId == m.MenuTabId && rmp.PermissionId == viewPermissionId && rmp.IsGranted))
                .OrderBy(m => m.OrderBy)
                .Select(m => new
                {
                    m.MenuTabId,
                    m.Title,
                    m.ParentId,
                    m.ControllerName,
                    m.Icon
                })
                .ToListAsync();

            return Json(modules);
        }
    }
}
