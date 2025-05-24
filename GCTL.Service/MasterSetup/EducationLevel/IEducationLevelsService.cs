using GCTL.Core.ViewModels.MasterSetup.EducationLevels;
using GCTL.Core.ViewModels;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Data.Models;

namespace GCTL.Service.MasterSetup.EducationLevel
{
    public interface IEducationLevelsService
    {
        #region CRUD
        Task<bool> AddAsync(EducationLevelVM model);
        Task<bool> UpdateAsync(EducationLevelVM model);
        Task<EducationLevelVM> SoftDeleteAsync(BaseViewModel model, List<int> ids);
        Task<EducationLevelVM> GetByIdAsync(int id);
        Task<PaginationService<EducationLevels, EducationLevelVM>.PaginationResult<EducationLevelVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "EducationLevelName", string sortOrder = "asc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}
