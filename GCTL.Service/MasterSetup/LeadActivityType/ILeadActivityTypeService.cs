using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.MasterSetup.Grade;
using GCTL.Core.ViewModels.MasterSetup.LeadActivityType;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.MasterSetup.LeadActivityType
{
    public interface ILeadActivityTypeService 
    {
        #region CRUD
        Task<bool> AddAsync(LeadActivityTypeVM model);
        Task<bool> UpdateAsync(LeadActivityTypeVM model);
        Task<LeadActivityTypeVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<LeadActivityTypeVM> GetByIdAsync(int id);
        Task<PaginationService<LeadActivityTypes, LeadActivityTypeVM>.PaginationResult<LeadActivityTypeVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "LeadActivityName", string sortOrder = "asc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}
