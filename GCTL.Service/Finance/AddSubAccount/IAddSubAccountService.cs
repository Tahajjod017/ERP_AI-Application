using GCTL.Core.Helpers;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Finance.AddSubAccountVM;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.Finance.AddSubAccount
{
    public interface IAddSubAccountService
    {
        #region CRUD
        Task<bool> AddAsync(CreateAddSubAccountVM model);
        Task<CommonReturnViewModel> UpdateAsync(UpdateAddSubAccountVM model);
        Task<DeleteAddSubAccountVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<GetByIdAddSubAccountVM> GetByIdAsync(int id);
        Task<PaginationService<SubAccounts, GetAllAddSubAccountVM>.PaginationResult<GetAllAddSubAccountVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5,
            string searchTerm = "", string sortColumn = "SubAccountID", string sortOrder = "desc", int? mainAccId = null);
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name, int? excludeId = null);
        Task<bool> IsCodeUniqueAsync(string name, int? excludeId = null);
        Task<string> GenerateNextCodeAsync(int mainAccId);
        #endregion
    }
}
