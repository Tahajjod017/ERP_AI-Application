using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.Finance.SecondTabVM;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.Finance.SecondTab
{
    public interface ISecondTabService
    {
        #region CRUD
        Task<bool> AddAsync(CreateSecondTabVM model);
        Task<bool> UpdateAsync(UpdateSecondTabVM model);
        Task<DeleteSecondTabVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<GetByIdSecondTabVM> GetByIdAsync(int id);
        Task<PaginationService<Classes, GetAllSecondTabVM>.PaginationResult<GetAllSecondTabVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "ClassID", string sortOrder = "desc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name, int baseAccountID, int? excludeId = null);
        #endregion
    }
}
