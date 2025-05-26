using GCTL.Core.ViewModels.MasterSetup.Degree;
using GCTL.Core.ViewModels;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Data.Models;
using GCTL.Core.Helpers;

namespace GCTL.Service.MasterSetup.Degrees
{
    public interface IDegreeService
    {
        #region CRUD
        Task<bool> AddAsync(DegreeVM model);
        Task<bool> UpdateAsync(DegreeVM model);
        Task<DegreeVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<DegreeVM> GetByIdAsync(int id);
        Task<PaginationService<Degree, DegreeVM>.PaginationResult<DegreeVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "DegreeID", string sortOrder = "desc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}
