using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.FieldServices;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
namespace GCTL.Service.FieldServices
{
    public interface ICreateJobService
    {
        #region CRUD
        Task<bool> AddAsync(CreateJobVM model);
        Task<bool> UpdateAsync(CreateJobVM model);
        Task<CreateJobVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<CreateJobVM> GetByIdAsync(int id);
        Task<PaginationService<Grade, CreateJobVM>.PaginationResult<CreateJobVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "CreateJobID", string sortOrder = "asc");
        #endregion
    }
}
