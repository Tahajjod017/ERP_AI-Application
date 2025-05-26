using GCTL.Core.ViewModels.MasterSetup.BloodGroup;
using GCTL.Core.ViewModels;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Data.Models;
using GCTL.Core.Helpers;

namespace GCTL.Service.MasterSetup.BloodGroups
{
    public interface IBloodGroupService
    {
        #region CRUD
        Task<bool> AddAsync(BloodGroupVM model);
        Task<bool> UpdateAsync(BloodGroupVM model);
        Task<BloodGroupVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<BloodGroupVM> GetByIdAsync(int id);
        Task<PaginationService<BloodGroup, BloodGroupVM>.PaginationResult<BloodGroupVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "BloodGroupID", string sortOrder = "desc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}
