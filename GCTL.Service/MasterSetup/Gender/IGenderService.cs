using GCTL.Core.ViewModels.MasterSetup.EmployeeType;
using GCTL.Core.ViewModels;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.MasterSetup.Genders;
using GCTL.Data.Models;
using GCTL.Core.Helpers;

namespace GCTL.Service.MasterSetup.Gender
{
    public interface IGenderService
    {
        #region CRUD
        Task<bool> AddAsync(GenderVM model);
        Task<bool> UpdateAsync(GenderVM model);
        Task<GenderVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<GenderVM> GetByIdAsync(int id);
        Task<PaginationService<Genders, GenderVM>.PaginationResult<GenderVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "GenderName", string sortOrder = "asc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}
