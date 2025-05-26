using GCTL.Core.ViewModels.MasterSetup.Currencies;
using GCTL.Core.ViewModels;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Data.Models;
using GCTL.Core.Helpers;

namespace GCTL.Service.MasterSetup.Currency
{
    public interface ICurrencyService
    {
        #region CRUD
        Task<bool> AddAsync(CurrencyVM model);
        Task<bool> UpdateAsync(CurrencyVM model);
        Task<CurrencyVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<CurrencyVM> GetByIdAsync(int id);
        Task<PaginationService<Currencies, CurrencyVM>.PaginationResult<CurrencyVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "CurrencyID", string sortOrder = "desc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}
