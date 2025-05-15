using GCTL.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AccessPermissions
{
    public class AccessControlService:IAccessControlService
    {
        private readonly AppDbContext _Db;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccessControlService(AppDbContext db, IHttpContextAccessor httpContextAccessor)
        {
            _Db = db;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> HasPermissionAsync(int menuTabId, string permissionType)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return false;

            var roleId = await _Db.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .FirstOrDefaultAsync();

            if (roleId == null) return false;

            var permissionId = await _Db.Permissions
                .Where(p => p.Name == permissionType)  // ✅ Matches "Create", "View", "Edit", or "Delete"
                .Select(p => p.Id)
                .FirstOrDefaultAsync();

            if (permissionId == 0) return false;

            return await _Db.RoleModulePermissions
                .AnyAsync(rmp => rmp.RoleId == roleId &&
                                 rmp.MenuTabId == menuTabId &&
                                 rmp.PermissionId == permissionId &&
                                 rmp.IsGranted);
        }
    }
}
