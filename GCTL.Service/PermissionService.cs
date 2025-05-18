using GCTL.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service
{
    public class PermissionService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PermissionService(AppDbContext context, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> HasPermissionAsync(string roleId, int moduleId, string permissionType)
        {
            // Check if the user has the specified permission type on the given module
            var permission = await _context.RoleModulePermissions
                .FirstOrDefaultAsync(p => p.RoleId == roleId && p.MenuTabId == moduleId && p.Permission.Name == permissionType);

            return permission != null && permission.IsGranted;
        }
        public async Task<bool> CanViewAsync(string controllerName)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || !user.Identity.IsAuthenticated)
                return false;

            // Get user's roles (assuming single role per user, or pick first if multiple)
            var appUser = await _userManager.GetUserAsync(user);
            var roles = await _userManager.GetRolesAsync(appUser);
            var roleName = roles.FirstOrDefault();

            if (string.IsNullOrEmpty(roleName)) return false;

            // Find role ID
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null) return false;

            // Match controller to MenuTab
            var module = await _context.MenuTabs.FirstOrDefaultAsync(m => m.ControllerName == controllerName);
            if (module == null) return false;

            // Check VIEW permission for that module
            return await HasPermissionAsync(role.Id, module.MenuTabId, "VIEW");
        }
    }
}
