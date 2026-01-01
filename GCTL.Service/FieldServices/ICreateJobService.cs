using GCTL.Core.Helpers;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.FieldServices;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using GCTL_App.ViewModels.FieldServiceOne;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace GCTL.Service.FieldServices
{
    public interface ICreateJobService
    {
        #region CRUD
        Task<bool> AddAsync(CreateJobVM model, string FileLink);
        Task<bool> UpdateAsync(CreateJobVM model);
        Task<CreateJobVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<CreateJobVM> GetByIdAsync(int organizationID, int jobId);
        Task<ReturnDataView<CustomerInfoVM>> GetPagedEmployeesAsync(string search, int page, int pageSize, int organizationID);
        Task<ReturnDataView<CustomerInfoVM>> GetCompanyEmployeesAsync(string search, int page, int pageSize, int organizationID);
        Task<ReturnDataView<SelectListItem>> GetCountryList(string search, int page, int pageSize, int organizationID);
        Task<ReturnDataView<CustomerInfoVM>> GetIndividualEmployeesAsync(string search, int page, int pageSize, int organizationID);
        Task<ReturnDataView<CustomerInfoVM>> GetTechnicianListAsync(string search, int page, int pageSize, int organizationID);
        Task<ReturnDataView<SelectListItem>> GetJobAsync(int customerId, string search, int page, int pageSize, int organizationID);
        Task<ReturnDataView<CreateJobVM>> GetAllAsync(int organizationID, int pageNumber = 1, int pageSize = 5,
            string searchTerm = "", string sortColumn = "CreateJobID", string sortOrder = "asc");
        Task<CommonReturnViewModel> GetCalenderData(int organizationID, DateTime start, DateTime end, string searchTerm = "");
        CustomerInfoVM GetCustomerInfo(int jobId, int organizationID);
        Task<ReturnDataView<SelectListItem>> GetDivisionsAsync(string search);
        Task<ReturnDataView<SelectListItem>> GetStatusesAsync(string search);
        Task<ReturnDataView<SelectListItem>> GetJobTypesAsync(string search);
        Task<bool> EditAsync(CreateJobVM model, string FileLink);
        Task<CommonReturnViewModel> SaveJobTeamActivityAsync(
            SaveJobTeamActivityRequest request,
           int organizationID,
           int currentUserId);
        #endregion
        //Task<CommonReturnViewModel>  SaveActivityAsync( SaveActivityRequest request, int organizationID, int currentUserId);
    }
}
