using GCTL.Core.ViewModels.MasterSetup.Country;
using GCTL.Core.ViewModels;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Data.Models;

namespace GCTL.Service.MasterSetup.Countries
{
    public interface ICountryService
    {
        #region CRUD
        Task<bool> AddAsync(CountryVM model);
        Task<bool> UpdateAsync(CountryVM model);
        Task<CountryVM> SoftDeleteAsync(BaseViewModel model, List<int> ids);
        Task<CountryVM> GetByIdAsync(int id);
        Task<PaginationService<Country, CountryVM>.PaginationResult<CountryVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "CountryID", string sortOrder = "desc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}
