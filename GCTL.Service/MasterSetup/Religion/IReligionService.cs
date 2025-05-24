using GCTL.Core.ViewModels.MasterSetup.Religions;
using GCTL.Core.ViewModels;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Data.Models;

namespace GCTL.Service.MasterSetup.Religion
{
    public interface IReligionService
    {
        #region CRUD
        Task<bool> AddAsync(ReligionVM model);
        Task<bool> UpdateAsync(ReligionVM model);
        Task<ReligionVM> SoftDeleteAsync(BaseViewModel model, List<int> ids);
        Task<ReligionVM> GetByIdAsync(int id);
        Task<PaginationService<Religions, ReligionVM>.PaginationResult<ReligionVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "ReligionName", string sortOrder = "asc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}
