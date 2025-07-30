using GCTL.Core.ViewModels.MasterSetup.ServiceType;

namespace GCTL.Service.MasterSetup.ServiceType
{
    public interface IServiceTypeService
    {
        #region CRUD
        Task<bool> AddAsync(ServiceTypeVM model);
               //Task<bool> UpdateAsync(GenderVM model);
        //        //Task<GenderVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        //        //Task<GenderVM> GetByIdAsync(int id);
        //        //Task<PaginationService<Genders, GenderVM>.PaginationResult<GenderVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        //        //string sortColumn = "GenderName", string sortOrder = "asc");
        //        //#endregion


        //        //#region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}