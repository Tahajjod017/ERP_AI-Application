using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Core.ViewModels.ElementPermission;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.ElementPermission
{
    public interface IElementPermissionService
    {
        Task<bool> HasPermissionForElementAsync(string userId, int pageId, string elementKey);
        Task<PaginationService<RoleElementPermissions, ElementPermissionVM>.PaginationResult<ElementPermissionVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "HolidayID", string sortOrder = "desc", int? organizationID = null);
        Task<ElementPermissionVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<List<int?>> GetEmployeesForElementAsync(string elementKey);
    }
}
