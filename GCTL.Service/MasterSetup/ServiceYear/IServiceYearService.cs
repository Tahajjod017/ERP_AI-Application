using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.MasterSetup.ActionTakens;
using GCTL.Core.ViewModels.MasterSetup.ServiceYear;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.MasterSetup.ServiceYear
{
    public interface IServiceYearService
    {
        #region CRUD
        Task<bool> AddAsync(ServiceYearVM model);
        Task<bool> UpdateAsync(ServiceYearVM model);
        Task<ServiceYearVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<ServiceYearVM> GetByIdAsync(int id);
        Task<PaginationService<ServiceYears, ServiceYearVM>.PaginationResult<ServiceYearVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "ServiceYearID", string sortOrder = "desc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}
