using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.MasterSetup.Organizations;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.MasterSetup.Organizations
{
    public interface IOrganizationsService
    {
        #region CRUD
        Task<bool> AddAsync(OrganizationsVM model);
        Task<bool> UpdateAsync(OrganizationsVM model);
        Task<OrganizationsVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<OrganizationsVM> GetByIdAsync(int id);
        Task<PaginationService<Organization, OrganizationsVM>.PaginationResult<OrganizationsVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "OrganizationID", string sortOrder = "desc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}
