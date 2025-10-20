using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.MasterSetup.Genders;
using GCTL.Core.ViewModels.MasterSetup.JobTypes;
using GCTL.Data.Models;
using GCTL.Service.Pagination;

namespace GCTL.Service.MasterSetup.JobTypes
{
    public interface IJobTypeService
    {

        #region CRUD
        Task<bool> AddAsync(JobTypeVM model);
        Task<bool> UpdateAsync(JobTypeVM model);
        Task<JobTypeVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<JobTypeVM> GetByIdAsync(int id);
        Task<PaginationService<GCTL.Data.Models.JobTypes, JobTypeVM>.PaginationResult<JobTypeVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "JobTypeName", string sortOrder = "asc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}
