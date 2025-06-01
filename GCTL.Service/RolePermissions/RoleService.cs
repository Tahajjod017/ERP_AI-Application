using GCTL.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.RolePermissions
{
    public class RoleService : IRoleService
    {
        private readonly AppDbContext _context;

        public RoleService(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<ApplicationRole> GetRoles(string searchTerm)
        {
            var query = _context.ApplicationRoles.AsQueryable(); // Corrected to use ApplicationRoles instead of Roles  

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(r => r.Name.Contains(searchTerm));
            }

            return query.OrderBy(r => r.Name);
        }

        public async Task<List<ApplicationRole>> GetPagedRolesAsync(string searchTerm, int pageNumber, int pageSize, int? companyId = null, int? tenantId = null)
        {
            var query = _context.ApplicationRoles.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(r => r.Name.Contains(searchTerm));
            }

            if (tenantId.HasValue)
            {
                query = query.Where(r => r.TenantInfoId == tenantId);

                if (companyId.HasValue)
                {
                    query = query.Where(r => r.OrganizationID == companyId);
                }
                else
                {
                    // companyId is null, so filter roles where company is null
                    query = query.Where(r => r.OrganizationID == null);
                }
            }
            else
            {
                // tenantId is null, return empty
                return new List<ApplicationRole>();
            }

            return await query
                .OrderBy(r => r.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalRolesCountAsync(string searchTerm)
        {
            return await GetRoles(searchTerm).CountAsync();
        }

        public async Task<Dictionary<string, List<ApplicationUser>>> GetPagedRoleUserAssignmentsAsync(string searchTerm, int pageNumber, int pageSize, int? companyId = null, int? tenantId = null)
        {
            var roles = await GetPagedRolesAsync(searchTerm, pageNumber, pageSize,companyId,tenantId);

            var result = new Dictionary<string, List<ApplicationUser>>();

            foreach (var role in roles)
            {
                var userIds = await _context.UserRoles
                    .Where(ur => ur.RoleId == role.Id)
                    .Select(ur => ur.UserId)
                    .ToListAsync();

                var users = await _context.Users
                    .Where(u => userIds.Contains(u.Id))
                    .ToListAsync();

                result.Add(role.Name, users);
            }

            return result;
        }
    }

}
