using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.FieldServices;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace GCTL.Service.FieldServices
{
    public interface ICreateJobService
    {
        #region CRUD
        Task<bool> AddAsync(CreateJobVM model);
        Task<bool> UpdateAsync(CreateJobVM model);
        Task<CreateJobVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<CreateJobVM> GetByIdAsync(int id);
        Task<ReturnDataView<CustomerInfoVM>> GetPagedEmployeesAsync(string search, int page, int pageSize, int organizationID);
        Task<ReturnDataView<CustomerInfoVM>> GetCompanyEmployeesAsync(string search, int page, int pageSize, int organizationID);
        Task<ReturnDataView<SelectListItem>> GetCountryList(string search, int page, int pageSize, int organizationID);
        Task<ReturnDataView<CustomerInfoVM>> GetIndividualEmployeesAsync(string search, int page, int pageSize, int organizationID);
        Task<ReturnDataView<CustomerInfoVM>> GetTechnicianListAsync(string search, int page, int pageSize, int organizationID);
        Task<ReturnDataView<CreateJobVM>> GetAllAsync(int organizationID, int pageNumber = 1, int pageSize = 5,
            string searchTerm = "", string sortColumn = "CreateJobID", string sortOrder = "asc");
        #endregion
    }
}
