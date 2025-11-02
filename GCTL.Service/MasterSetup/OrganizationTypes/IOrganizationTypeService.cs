using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.MasterSetup.OrganizationTypes;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
namespace GCTL.Service.MasterSetup.CompanyTypes
{
    public interface IOrganizationTypeService
    {
        #region CRUD
        Task<bool> AddAsync(OrganizationTypeVM model);
        Task<bool> UpdateAsync(OrganizationTypeVM model);
        Task<OrganizationTypeVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<OrganizationTypeVM> GetByIdAsync(int organizationID, int id);
        Task<PaginationService<OrganizationTypes, OrganizationTypeVM>.PaginationResult<OrganizationTypeVM>> GetAllAsync(int organizationID, int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "CreatedAt", string sortOrder = "asc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(int organizationID, string name);
        #endregion
    }
}
