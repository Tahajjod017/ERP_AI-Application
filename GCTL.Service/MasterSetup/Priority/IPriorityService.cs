using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.MasterSetup.LeadSource;
using GCTL.Core.ViewModels.MasterSetup.Priority;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.MasterSetup.Priority
{
    public interface IPriorityService
    {
        #region CRUD
        Task<bool> AddAsync(PriorityVM model);
        Task<PaginationService<Priorities, PriorityVM>.PaginationResult<PriorityVM>> GetAllAsync(int organizationID, int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "PriorityName", string sortOrder = "asc");
        Task<PriorityVM> GetByIdAsync(int organizationID, int id);
        Task<bool> UpdateAsync(PriorityVM model);
        Task<PriorityVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<bool> IsNameUniqueAsync(int organizationID, string name);
        #endregion
    }
}
