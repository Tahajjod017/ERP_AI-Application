using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.MasterSetup.LeadStatuses;
using GCTL.Data.Models;
using GCTL.Service.Pagination;

namespace GCTL.Service.MasterSetup.LeadStatuses
{
    public interface ILeadStatusService
    {
        #region CRUD
        Task<bool> AddAsync(LeadStatusVM model);
        Task<PaginationService<GCTL.Data.Models.LeadStatuses, LeadStatusVM>.PaginationResult<LeadStatusVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "ServiceName", string sortOrder = "asc");
        Task<LeadStatusVM> GetByIdAsync(int id);
        Task<bool> UpdateAsync(LeadStatusVM model);
        Task<LeadStatusVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}