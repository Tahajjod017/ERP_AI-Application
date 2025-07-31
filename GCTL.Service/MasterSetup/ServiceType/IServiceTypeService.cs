using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.MasterSetup.Genders;
using GCTL.Core.ViewModels.MasterSetup.ServiceType;
using GCTL.Data.Models;
using GCTL.Service.Pagination;

namespace GCTL.Service.MasterSetup.ServiceType
{
    public interface IServiceTypeService
    {
        #region CRUD
        Task<bool> AddAsync(ServiceVM model);
        Task<PaginationService<Services, ServiceVM>.PaginationResult<ServiceVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "ServiceName", string sortOrder = "asc");
        Task<ServiceVM> GetByIdAsync(int id);
        Task<bool> UpdateAsync(ServiceVM model);
        Task<ServiceVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}