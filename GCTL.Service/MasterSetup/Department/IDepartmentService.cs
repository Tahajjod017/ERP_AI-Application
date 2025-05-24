using GCTL.Core.ViewModels.MasterSetup.Departments;
using GCTL.Core.ViewModels;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Data.Models;

namespace GCTL.Service.MasterSetup.Department
{
    public interface IDepartmentService
    {
        #region CRUD
        Task<bool> AddAsync(DepartmentVM model);
        Task<bool> UpdateAsync(DepartmentVM model);
        Task<DepartmentVM> SoftDeleteAsync(BaseViewModel model, List<int> ids);
        Task<DepartmentVM> GetByIdAsync(int id);
        Task<PaginationService<Departments, DepartmentVM>.PaginationResult<DepartmentVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "DepartmentID", string sortOrder = "desc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}
