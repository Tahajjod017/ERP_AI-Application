using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.Finance.TransactionAccountVM;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.Finance.TransactionAccount
{
    public interface ITransactionAccountService
    {
        #region CRUD
        Task<bool> AddAsync(CreateTransactionAccountVM model);
        Task<bool> UpdateAsync(UpdateTransactionAccountVM model);
        Task<DeleteTransactionAccountVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<GetByIdTransactionAccountVM> GetByIdAsync(int id);
        Task<PaginationService<TransactionAccounts, GetAllTransactionAccountVM>.PaginationResult<GetAllTransactionAccountVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5,
            string searchTerm = "", string sortColumn = "GroupID", string sortOrder = "desc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name, int? excludeId = null);
        Task<bool> IsCodeUniqueAsync(string name, int? excludeId = null);
        #endregion
    }
}
