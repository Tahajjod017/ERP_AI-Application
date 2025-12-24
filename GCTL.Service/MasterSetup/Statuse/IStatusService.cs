using GCTL.Core.ViewModels.MasterSetup.Statuses;
using GCTL.Core.ViewModels;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Data.Models;
using GCTL.Core.Helpers;

namespace GCTL.Service.MasterSetup.Statuse
{
    public interface IStatusService
    {
        #region CRUD
        Task<bool> AddAsync(StatusVM model);
        Task<bool> UpdateAsync(StatusVM model);
        Task<StatusVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<StatusVM> GetByIdAsync(int id);
        Task<PaginationService<Statuses, StatusVM>.PaginationResult<StatusVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "StatusID", string sortOrder = "desc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion

        Task<int> GetStatusIDAsync(string name , string type = "default");
        int GetStatusID(string name , string type = "default");
    }
}
