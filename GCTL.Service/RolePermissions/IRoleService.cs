using GCTL.Core.ViewModels.Login;
using GCTL.Data.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.RolePermissions
{
    public interface IRoleService
    {
        IQueryable<ApplicationRole> GetRoles(string searchTerm);
        Task<List<ApplicationRole>> GetPagedRolesAsync(string searchTerm, int pageNumber, int pageSize, int? companyId = null, int? tenantId = null);
        Task<int> GetTotalRolesCountAsync(string searchTerm);
        Task<List<RoleUserAssignment>> GetPagedRoleUserAssignmentsAsync(string searchTerm, int pageNumber, int pageSize, int? companyId = null, int? tenantId = null);
    }
}
