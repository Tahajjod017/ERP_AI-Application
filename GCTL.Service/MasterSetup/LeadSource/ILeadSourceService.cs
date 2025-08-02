using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.MasterSetup.LeadSource;
using GCTL.Core.ViewModels.MasterSetup.LeadStatuses;
using GCTL.Data.Models;
using GCTL.Service.Pagination;

namespace GCTL.Service.MasterSetup.LeadSource
{
    public interface ILeadSourceService
    {
        #region CRUD
        Task<bool> AddAsync(LeadSourceVM model);
        Task<PaginationService<GCTL.Data.Models.LeadSources, LeadSourceVM>.PaginationResult<LeadSourceVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "LeadSourceName", string sortOrder = "asc");
        Task<LeadSourceVM> GetByIdAsync(int id);
        Task<bool> UpdateAsync(LeadSourceVM model);
        Task<LeadSourceVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}