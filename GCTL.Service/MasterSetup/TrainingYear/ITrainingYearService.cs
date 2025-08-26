using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.MasterSetup.TrainingYear;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.MasterSetup.TrainingYear
{
    public interface ITrainingYearService
    {
        #region CRUD
        Task<bool> AddAsync(TrainingYearVM model);
        Task<bool> UpdateAsync(TrainingYearVM model);
        Task<TrainingYearVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<TrainingYearVM> GetByIdAsync(int id);
        Task<PaginationService<TrainingYears, TrainingYearVM>.PaginationResult<TrainingYearVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "TrainingYearID", string sortOrder = "desc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}
