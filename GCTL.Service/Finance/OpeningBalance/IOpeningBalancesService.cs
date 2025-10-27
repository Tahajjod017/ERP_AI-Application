using GCTL.Core.Helpers;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Finance.OpeningBalancesVM;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.Finance.OpeningBalance
{
    public interface IOpeningBalancesService
    {
        #region CRUD
        Task<CommonReturnViewModel> AddAsync(CreateOpeningBalancesVM model);
        Task<CommonReturnViewModel> UpdateAsync(UpdateOpeningBalancesVM model);
        Task<CommonReturnViewModel> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<GetByIdOpeningBalancesVM> GetByIdAsync(int id);
        Task<PaginationService<OpeningBalances, GetAllOpeningBalancesVM>.PaginationResult<GetAllOpeningBalancesVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "OpeningBalanceID", string sortOrder = "desc");
        #endregion


        #region Others
        Task<string> GenerateThreeDigitCodeAsync();
        Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null);
        #endregion
    }
}
