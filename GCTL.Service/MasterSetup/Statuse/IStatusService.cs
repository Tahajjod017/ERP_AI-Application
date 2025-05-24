using GCTL.Core.ViewModels.MasterSetup.Statuses;
using GCTL.Core.ViewModels;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Data.Models;

namespace GCTL.Service.MasterSetup.Statuse
{
    public interface IStatusService
    {
        #region CRUD
        Task<bool> AddAsync(StatusVM model);
        Task<bool> UpdateAsync(StatusVM model);
        Task<StatusVM> SoftDeleteAsync(BaseViewModel model, List<int> ids);
        Task<StatusVM> GetByIdAsync(int id);
        Task<PaginationService<Statuses, StatusVM>.PaginationResult<StatusVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "StatusName", string sortOrder = "asc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}
