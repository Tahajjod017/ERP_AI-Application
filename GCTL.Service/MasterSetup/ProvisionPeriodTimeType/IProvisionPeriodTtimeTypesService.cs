using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.MasterSetup.ProvisionPeriodTtimeTypes;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.MasterSetup.ProvisionPeriodTimeType
{
    public interface IProvisionPeriodTtimeTypesService
    {
        #region CRUD
        Task<bool> AddAsync(ProvisionPeriodTtimeTypesVM model);
        Task<bool> UpdateAsync(ProvisionPeriodTtimeTypesVM model);
        Task<ProvisionPeriodTtimeTypesVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<ProvisionPeriodTtimeTypesVM> GetByIdAsync(int id);
        Task<PaginationService<ProvisionPeriodTtimeTypes, ProvisionPeriodTtimeTypesVM>.PaginationResult<ProvisionPeriodTtimeTypesVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "ProvisionPeriodTtimeTypeID", string sortOrder = "desc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}
