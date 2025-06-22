using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AdminSettings.OrganizationSettings.CompanyService
{
    public interface ICompanySettingService
    {
        #region CRUD
        Task<bool> AddAsync(CompanySettingsVM model);
        Task<bool> UpdateAsync(CompanySettingsVM model);
        Task<CompanySettingsVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<CompanySettingsVM> GetByIdAsync(int id);
        Task<PaginationService<Organization, CompanySettingsVM>.PaginationResult<CompanySettingsVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "OrganizationID", string sortOrder = "desc", int? organizationID = null);
        #endregion
        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}
