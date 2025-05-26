using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.MasterSetup.ActionTakens;
using GCTL.Core.ViewModels.MasterSetup.ResultType;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.MasterSetup.ResultType
{
    public interface IResultTypeService
    {
        #region CRUD
        Task<bool> AddAsync(ResultTypeVM model);
        Task<bool> UpdateAsync(ResultTypeVM model);
        Task<ResultTypeVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<ResultTypeVM> GetByIdAsync(int id);
        Task<PaginationService<ResultTypes, ResultTypeVM>.PaginationResult<ResultTypeVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "ResultTypeID", string sortOrder = "desc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}
