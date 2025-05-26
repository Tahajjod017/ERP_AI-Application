using GCTL.Core.Helpers;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.MasterSetup.EmployeeType;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.MasterSetup.EmployeeTypes
{
    public interface IEmployeeTypesService
    {
        #region CRUD
        Task<bool> AddAsync(EmployeeTypesVM model);
        Task<bool> UpdateAsync(EmployeeTypesVM model);
        Task<EmployeeTypesVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<EmployeeTypesVM> GetByIdAsync(int id);
        Task<PaginationService<EmployeeType, EmployeeTypesVM>.PaginationResult<EmployeeTypesVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "EmployeeTypeName", string sortOrder = "asc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}
