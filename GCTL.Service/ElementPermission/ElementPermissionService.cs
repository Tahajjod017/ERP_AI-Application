using GCTL.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.ElementPermission
{
    public class ElementPermissionService : IElementPermissionService
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        // Constructor with dependency injection
        public ElementPermissionService(AppDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        // Check if the user's role has permission to access the element
        public async Task<bool> HasPermissionForElementAsync(string userId, int pageId, string elementKey)
        {
            // Get the user and their roles
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;  // If user not found, deny permission
            }

            // Get roles of the user
            var roleNames = await _userManager.GetRolesAsync(user);
            // Fetch the matching RoleIds from AspNetRoles
            var roleIds = await _dbContext.Roles
                .Where(r => roleNames.Contains(r.Name))
                .Select(r => r.Id)
                .ToListAsync();


            // Check if any of the user's roles has the specific permission for the element
            var hasPermission = await _dbContext.RoleElementPermissions
                .AnyAsync(p => roleIds.Contains(p.RoleId) && p.PageId == pageId && p.ElementKey == elementKey);

            return hasPermission;
        }
    }
}
