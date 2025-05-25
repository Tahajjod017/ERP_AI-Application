using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.MasterSetup.PassingYear;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.MasterSetup.PassingYear
{
    public interface IPassingYearService
    {
        #region CRUD
        Task<bool> AddAsync(PassingYearVM model);
        Task<bool> UpdateAsync(PassingYearVM model);
        Task<PassingYearVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<PassingYearVM> GetByIdAsync(int id);
        Task<PaginationService<PassingYears, PassingYearVM>.PaginationResult<PassingYearVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "PassingYearID", string sortOrder = "desc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}
