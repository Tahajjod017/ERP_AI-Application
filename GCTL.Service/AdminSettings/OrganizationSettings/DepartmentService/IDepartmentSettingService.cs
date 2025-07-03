using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace GCTL.Service.AdminSettings.OrganizationSettings.DepartmentService
{
    public interface IDepartmentSettingService
    {
        #region CRUD
        Task<bool> AddAsync(DepartmentSettingsVM model);
        Task<bool> UpdateAsync(DepartmentSettingsVM model);
        Task<DepartmentSettingsVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<DepartmentSettingsVM> GetByIdAsync(int id);
        Task<PaginationService<Departments, DepartmentSettingsVM>.PaginationResult<DepartmentSettingsVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "OrganizationDepartmentID", string sortOrder = "desc", int? organizationID = null);
        #endregion
        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        Task<List<SelectListItem>> GetOrganizationsAsync();
        Task<List<SelectListItem>> GetEmployeeCodeAsync();
        #endregion
    }
}
