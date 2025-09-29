using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.Finance.BaseAccountVM;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.Finance.BaseAccount
{
    public interface IBaseAccountService
    {
        #region CRUD
        Task<bool> AddAsync(CreateBaseAccountVM model);
        Task<bool> UpdateAsync(UpdateBaseAccountVM model);
        Task<DeleteBaseAccountVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<GetByIdBaseAccountVM> GetByIdAsync(int id);
        Task<PaginationService<BaseAccounts, GetAllBaseAccountVM>.PaginationResult<GetAllBaseAccountVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "BaseAccountID", string sortOrder = "desc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name, int? excludeId = null);
        #endregion
    }
}
