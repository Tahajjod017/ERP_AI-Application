using GCTL.Core.ViewModels.MasterSetup.Designations;
using GCTL.Core.ViewModels;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Data.Models;
using GCTL.Core.Helpers;

namespace GCTL.Service.MasterSetup.Designation
{
    public interface IDesignationService
    {
        #region CRUD
        Task<bool> AddAsync(DesignationVM model);
        Task<bool> UpdateAsync(DesignationVM model);
        Task<DesignationVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<DesignationVM> GetByIdAsync(int id);
        Task<PaginationService<Designations, DesignationVM>.PaginationResult<DesignationVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "DesignationID", string sortOrder = "desc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        IEnumerable<CommonSelectVM> GetDepartments();
        #endregion
    }
}
