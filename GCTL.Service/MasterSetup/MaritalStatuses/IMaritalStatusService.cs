using GCTL.Core.ViewModels.MasterSetup.MaritalStatus;
using GCTL.Core.ViewModels;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Data.Models;

namespace GCTL.Service.MasterSetup.MaritalStatuses
{
    public interface IMaritalStatusService
    {
        #region CRUD
        Task<bool> AddAsync(MaritalStatusVM model);
        Task<bool> UpdateAsync(MaritalStatusVM model);
        Task<MaritalStatusVM> SoftDeleteAsync(BaseViewModel model, List<int> ids);
        Task<MaritalStatusVM> GetByIdAsync(int id);
        Task<PaginationService<MaritalStatus, MaritalStatusVM>.PaginationResult<MaritalStatusVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "MaritalStatusName", string sortOrder = "asc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}
