using GCTL.Core.ViewModels.Login;
using GCTL.Core.ViewModels.RoleModule;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.Pagination;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers.Security
{
    public class RoleCreateAssignController : BaseController
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRoleService _roleService;
        private readonly AppDbContext _Db;

        public RoleCreateAssignController(ITranslateService translateService, IUserProfileService userProfileService, RoleManager<ApplicationRole> roleManager, AppDbContext db, IRoleService roleService, UserManager<ApplicationUser> userManager) : base(translateService, userProfileService)
        {
            _roleManager = roleManager;
            _Db = db;
            _roleService = roleService;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var companies = _Db.Organization
                         .Select(c => new SelectListItem
                         {
                             Value = c.OrganizationID.ToString(),
                             Text = c.OrganizationName.ToString(),
                         }).ToList();

            ViewBag.CompanyList = companies;
            return View();
        }

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
        public async Task<PaginationService<ApplicationRole, RoleVM>.PaginationResult<RoleVM>> GetRoles(
                    int pageNumber = 1,
                    int pageSize = 5,
                    string searchTerm = "",
                    string sortColumn = "RoleName",
                    string sortOrder = "asc",
                        int? tenantId = null,
                    int? companyId = null)
        {
            var query = _Db.ApplicationRoles.Include(x=>x.Organization).AsQueryable();

            // Filter by tenant and company if specified
            if (tenantId.HasValue)
                query = query.Where(r => r.TenantInfoId == tenantId.Value);

            if (companyId.HasValue)
                query = query.Where(r => r.OrganizationID == companyId.Value);

            // Example: filter out deleted roles if you track deleted
            //query = query.Where(r => r.dele == null);

            // Sorting
            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "RoleName" => sortOrder == "desc" ? query.OrderByDescending(r => r.Name) : query.OrderBy(r => r.Name),
                    "CompanyName" => sortOrder == "desc" ? query.OrderByDescending(r => r.Organization.OrganizationName) : query.OrderBy(r => r.Organization.OrganizationName),
                    _ => query.OrderBy(r => r.Name)
                };
            }

            // Use your pagination service
            return await PaginationService<ApplicationRole, RoleVM>.GetPaginatedData(
                query,
                pageNumber,
                pageSize,
                searchTerm,
                sortColumn,
                sortOrder,
                term => r => EF.Functions.Like(r.Name, $"%{term}%") || EF.Functions.Like(r.Organization.OrganizationName, $"%{term}%"),
                r => new RoleVM
                {
                    RoleId = r.Id,
                    RoleName = r.Name.ToCleanRoleName(),
                    CompanyName = r.Organization != null ? r.Organization.OrganizationName : "-",
                    CreatedBy =   "System"
                });
        }

        public async Task<List<ApplicationUser>> SearchUsers(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<ApplicationUser>();

            return await _Db.Users
                .Where(u => EF.Functions.Like(u.Email, $"%{searchTerm}%") || EF.Functions.Like(u.UserName, $"%{searchTerm}%"))
                .OrderBy(u => u.UserName)
                .Take(20)  // limit results for performance
                .ToListAsync();
        }

        public async Task<List<RoleDto>> GetRolesByCompany(int companyId)
        {
            return await _Db.ApplicationRoles
                .Where(r => r.OrganizationID == companyId)
                .OrderBy(r => r.Name)
                .Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name.ToCleanRoleName()
                })
                .ToListAsync();
        }

        public async Task<IActionResult> AssignRole(AssignRoleRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            ApplicationUser user = null;
            if (model.UserId != null)
            {
                user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                TempData["Error"] = "User ID is required.";
                return RedirectToAction("Index");
            }
            try
            {
                var result = await _userManager.AddToRoleAsync(user, model.RoleId);

                if (result.Succeeded) // Fix: Check the `Succeeded` property of `IdentityResult` instead of treating it as a boolean.
                {
                    TempData["Message"] = "Role assigned successfully.";
                    return RedirectToAction("Index"); // Or your desired redirect
                }
                else
                {
                    TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Index");
            }
        }



    }
}
