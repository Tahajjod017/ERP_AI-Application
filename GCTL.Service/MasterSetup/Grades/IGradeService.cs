using GCTL.Core.Helpers;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.MasterSetup.Grade;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.MasterSetup.Grades
{
    public interface IGradeService
    {
        #region CRUD
        Task<bool> AddAsync(GradeVM model);
        Task<bool> UpdateAsync(GradeVM model);
        Task<GradeVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<GradeVM> GetByIdAsync(int id);
        Task<PaginationService<Grade, GradeVM>.PaginationResult<GradeVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "GradeName", string sortOrder = "asc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}