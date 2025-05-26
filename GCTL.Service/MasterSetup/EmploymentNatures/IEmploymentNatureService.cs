using GCTL.Core.Helpers;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.MasterSetup.EmploymentNature;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.MasterSetup.EmploymentNatures
{
    public interface IEmploymentNatureService
    {
        #region CRUD
        Task<bool> AddAsync(EmploymentNatureVM model);
        Task<bool> UpdateAsync(EmploymentNatureVM model);
        Task<EmploymentNatureVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<EmploymentNatureVM> GetByIdAsync(int id);
        Task<PaginationService<EmploymentNature, EmploymentNatureVM>.PaginationResult<EmploymentNatureVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "EmploymentNatureName", string sortOrder = "asc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}
