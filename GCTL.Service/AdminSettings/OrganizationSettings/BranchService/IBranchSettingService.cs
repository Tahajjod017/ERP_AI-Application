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

namespace GCTL.Service.AdminSettings.OrganizationSettings.BranchService
{
    public interface IBranchSettingService
    {  
        #region CRUD
        Task<bool> AddAsync(BranchSettingsVM model);
        Task<bool> UpdateAsync(BranchSettingsVM model);
        Task<BranchSettingsVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<BranchSettingsVM> GetByIdAsync(int id);
        Task<PaginationService<OrganizationBranches, BranchSettingsVM>.PaginationResult<BranchSettingsVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "OrganizationBranchID", string sortOrder = "desc", int? organizationID = null);

        #endregion

        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        Task<List<SelectListItem>> GetCountriesAsync();
        Task<List<SelectListItem>> GetOrganizationsAsync();
        #endregion

    }
}
