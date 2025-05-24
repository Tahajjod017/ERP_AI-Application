using GCTL.Core.ViewModels.MasterSetup.EducationBoard;
using GCTL.Core.ViewModels;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Data.Models;

namespace GCTL.Service.MasterSetup.EducationBoards
{
    public interface IEducationBoardService
    {
        #region CRUD
        Task<bool> AddAsync(EducationBoardVM model);
        Task<bool> UpdateAsync(EducationBoardVM model);
        Task<EducationBoardVM> SoftDeleteAsync(BaseViewModel model, List<int> ids);
        Task<EducationBoardVM> GetByIdAsync(int id);
        Task<PaginationService<EducationBoard, EducationBoardVM>.PaginationResult<EducationBoardVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "EducationBoardID", string sortOrder = "desc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}
