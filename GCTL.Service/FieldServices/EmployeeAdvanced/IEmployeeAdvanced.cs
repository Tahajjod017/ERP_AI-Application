
using GCTL.Core.Helpers;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.FieldServices;
using GCTL.Core.ViewModels.Finance.AddSubAccountVM;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL.Service.FieldServices.EmployeeAdvanced
{
    #region CRUD
    public interface IEmployeeAdvanced
    {
        Task<CommonReturnViewModel> AddAsync(EmployeeAdvancedVM emp);

        Task<CommonReturnViewModel> UpdateAsync(EmployeeAdvancedVM emp);

        Task<CommonReturnViewModel> SoftDeleteAsync(DeleteRequestVM requestVM);

        Task<IEnumerable<CommonSelectVM>> EmployeeDD(); //Modern Dropdown for Employees

        //Task<CommonReturnViewModel> FirstApprovalAsync(int advanceId, bool approve, int approverId);

        Task<PaginationService<EmployeeAdvances, EmployeeAdvancedVM>.PaginationResult<EmployeeAdvancedVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5,
             string searchTerm = "", string sortColumn = "EmployeeAdvanceID", string sortOrder = "desc", int? mainAccId = null);

        #region Get
        Task<EmployeeAdvancedVM> GetByIdAsync(int id);
        Task<List<EmployeeAdvancedVM>> GetJobByCusId(int customerId); //Cascading

        Task<ReturnDataView<SelectListItem>> GetJobTypeAsync(string search, int page, int pageSize, int organizationID);//Modern Dropdown for Job Types
        #endregion



    }
    #endregion
}
