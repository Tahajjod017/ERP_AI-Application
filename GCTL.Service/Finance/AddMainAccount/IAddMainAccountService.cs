using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.Finance.AddMainAccountVM;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.Finance.AddMainAccount
{
    public interface IAddMainAccountService
    {
        #region CRUD
        Task<bool> AddAsync(CreateAddMainAccountVM model);
        Task<bool> UpdateAsync(UpdateAddMainAccountVM model);
        Task<DeleteAddMainAccountVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<GetByIdAddMainAccountVM> GetByIdAsync(int id);
        Task<PaginationService<MainAccounts, GetAllAddMainAccountVM>.PaginationResult<GetAllAddMainAccountVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5,
            string searchTerm = "", string sortColumn = "MainAccountID", string sortOrder = "desc", int? groupId = null);
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name, int? excludeId = null);
        Task<bool> IsCodeUniqueAsync(string name, int? excludeId = null);
        #endregion
    }
}
