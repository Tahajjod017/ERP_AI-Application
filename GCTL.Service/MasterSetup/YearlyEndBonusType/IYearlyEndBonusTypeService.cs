using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.MasterSetup.ActionTakens;
using GCTL.Core.ViewModels.MasterSetup.YearlyEndBonusType;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.MasterSetup.YearlyEndBonusType
{
    public interface IYearlyEndBonusTypeService
    {
        #region CRUD
        Task<bool> AddAsync(YearlyEndBonusTypeVM model);
        Task<bool> UpdateAsync(YearlyEndBonusTypeVM model);
        Task<YearlyEndBonusTypeVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<YearlyEndBonusTypeVM> GetByIdAsync(int id);
        Task<PaginationService<YearlyEndBonusTypes, YearlyEndBonusTypeVM>.PaginationResult<YearlyEndBonusTypeVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "YearlyEndBonusTypeID", string sortOrder = "desc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}
