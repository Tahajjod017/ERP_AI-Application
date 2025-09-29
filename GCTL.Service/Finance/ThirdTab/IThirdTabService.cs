using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.Finance.ThirdTabVM;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.Finance.ThirdTab
{
    public interface IThirdTabService
    {
        #region CRUD
        Task<bool> AddAsync(CreateThirdTabVM model);
        Task<bool> UpdateAsync(UpdateThirdTabVM model);
        Task<DeleteThirdTabVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<GetByIdThirdTabVM> GetByIdAsync(int id);
        Task<PaginationService<Groups, GetAllThirdTabVM>.PaginationResult<GetAllThirdTabVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, 
            string searchTerm = "", string sortColumn = "GroupID", string sortOrder = "desc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name, int id, int? excludeId = null);
        #endregion
    }
}
