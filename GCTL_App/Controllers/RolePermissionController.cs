using GCTL.Core.ViewModels.RoleModule;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;

namespace GCTL_App.Controllers
{
    [Authorize]
    public class RolePermissionController : BaseController
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext _Db;

        public RolePermissionController(ITranslateService translateService, IUserProfileService userProfileService, RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, AppDbContext db) : base(translateService, userProfileService)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _Db = db;
        }

        public async Task<IActionResult> Index()
        {
            // Get current user ID  
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            //// Load roles into ViewBag (No tenant check here)  
            //ViewBag.Companies = _Db.Organization.Select(x=> new
            //{
            //    Id= x.OrganizationID,
            //    Name = x.OrganizationName // Assuming ToCleanRoleName() is an extension method to format the name
            //}).ToList();
            //ViewBag.Roles = new List<ApplicationRole>();
            var user = await _userManager.Users
                .Where(u => u.Id == currentUserId)
                .Select(u => new { u.TenantInfoId, u.OrganizationID })
                .FirstOrDefaultAsync();

            ViewBag.TenantId = user?.TenantInfoId;
            ViewBag.UserCompanyId = user?.OrganizationID;

            if (user.OrganizationID == null)
            {
                // Super admin: show all companies
                ViewBag.Companies = _Db.Organization
                    .Select(x => new { Id = x.OrganizationID, Name = x.OrganizationName })
                    .ToList();
            }
            else
            {
                // Restricted: show only the user’s company
                ViewBag.Companies = _Db.Organization
                    .Where(x => x.OrganizationID == user.OrganizationID)
                    .Select(x => new { Id = x.OrganizationID, Name = x.OrganizationName })
                    .ToList();
            }

            ViewBag.Roles = new List<ApplicationRole>();
            return View(); 
        }

        [HttpGet]
        public JsonResult GetRolesByCompany(int? companyId)
        {
            int tenantId = 1;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user =  _userManager.Users
                .Where(u => u.Id == userId)
                .Select(u => new { u.TenantInfoId, u.OrganizationID })
                .FirstOrDefault();
            // Convert 0 to null manually
            if (companyId == 0)
            {
                companyId = null;
            }

            // Restriction: If user has a specific company, enforce it
            if (user.OrganizationID != null && user.OrganizationID != companyId)
            {
                return Json(new { success = false, message = "Access denied to selected company." });
            }

            var rolesQuery = _Db.ApplicationRoles.Where(r => r.TenantInfoId == tenantId);

            if (companyId.HasValue)
                rolesQuery = rolesQuery.Where(r => r.OrganizationID == companyId.Value);
            else
                rolesQuery = rolesQuery.Where(r => r.OrganizationID == null);

            var roles = rolesQuery
                .Select(r => new { Id = r.Id, Name = r.Name.ToCleanRoleName() })
                .ToList();

            return Json(roles);
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
            if (model == null || string.IsNullOrEmpty(model.RoleId) || model.Permissions == null || !model.Permissions.Any())
            {
                return BadRequest("Invalid data.");
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Normalize and extract unique permission names and module IDs
            var permissionNames = model.Permissions
                .Select(p => p.Permission?.Trim().ToLower())
                .Where(p => !string.IsNullOrEmpty(p))
                .Distinct()
                .ToList();

            var moduleIds = model.Permissions
                .Select(p => p.MenuTabId)
                .Distinct()
                .ToList();

            // Fetch all permissions from DB and normalize names
            var permissionsInDb = await _Db.Permissions.ToListAsync();
            var permissionIdMap = permissionsInDb
                .Where(p => permissionNames.Contains(p.Name.Trim().ToLower()))
                .ToDictionary(p => p.Name.Trim().ToLower(), p => p.Id);

            // Check for missing permissions
            var matchedPermissionNames = permissionIdMap.Keys.ToList();
            var missingPermissions = permissionNames.Except(matchedPermissionNames).ToList();

            if (missingPermissions.Any())
            {
                return BadRequest($"Permission(s) not found: {string.Join(", ", missingPermissions)}");
            }

            // Fetch all existing RoleModulePermissions for that role and modules
            var existingRolePerms = await _Db.RoleModulePermissions
                .Where(p => p.RoleId == model.RoleId && moduleIds.Contains(p.MenuTabId))
                .ToListAsync();

            // ====== ⛔ REMOVE SECTION ======
            var permissionsToRemove = model.Permissions
                .Where(p => !p.IsGranted)
                .Select(p => new
                {
                    p.MenuTabId,
                    PermissionId = permissionIdMap.ContainsKey(p.Permission.Trim().ToLower())
                        ? permissionIdMap[p.Permission.Trim().ToLower()]
                        : 0
                })
                .Where(x => x.PermissionId > 0)
                .Select(x => existingRolePerms.FirstOrDefault(ep =>
                    ep.RoleId == model.RoleId &&
                    ep.MenuTabId == x.MenuTabId &&
                    ep.PermissionId == x.PermissionId &&
                    ep.IsGranted == true))
                .Where(ep => ep != null)
                .ToList();

            _Db.RoleModulePermissions.RemoveRange(permissionsToRemove);

            // ====== ✅ ADD SECTION ======
            var newPermissions = model.Permissions
                .Where(p => p.IsGranted)
                .Where(p => !existingRolePerms.Any(ep =>
                    ep.RoleId == model.RoleId &&
                    ep.MenuTabId == p.MenuTabId &&
                    ep.PermissionId == permissionIdMap[p.Permission.Trim().ToLower()] &&
                    ep.IsGranted == true))
                .Select(p => new RoleModulePermissions
                {
                    RoleId = model.RoleId,
                    MenuTabId = p.MenuTabId,
                    PermissionId = permissionIdMap[p.Permission.Trim().ToLower()],
                    IsGranted = true
                })
                .ToList();

            // ====== ✅ SAVE WITH ERROR HANDLING ======
            try
            {
                await _Db.RoleModulePermissions.AddRangeAsync(newPermissions);
                await _Db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Database update failed: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }

            return Ok(new { message = "Permissions updated successfully." });
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
