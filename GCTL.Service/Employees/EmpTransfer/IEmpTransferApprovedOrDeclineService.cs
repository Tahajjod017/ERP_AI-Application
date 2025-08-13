using GCTL.Core.ViewModels.Employee.EmpTransfer;
using GCTL.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Data.Models;
using GCTL.Service.Pagination;

namespace GCTL.Service.Employees.EmpTransfer
{
    public interface IEmpTransferApprovedOrDeclineService
    {
        Task<CommonReturnViewModel> GetEmployeeTransferByIdAsync(int employeeTransferID);
        Task<CommonReturnViewModel> UpdateEmployeeTransferAsync(EmployeeTransferApproveOrDecEditVM entityVM);
        Task<PaginationService<EmployeeTransfer, EmployeeTransferTableListVM>.PaginationResult<EmployeeTransferTableListVM>> GetAllTableAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
     string currentSortColumn = "", string currentSortOrder = "", string url = "", string userId = "", int? organizationId = null,
 List<int> departmentIds = null,
 List<int> employeeIds = null, DateOnly? fromDate = null, DateOnly? toDate = null);
        Task<PaginationService<EmployeeTransfer, EmployeeTransferTableListVM>.PaginationResult<EmployeeTransferTableListVM>> GetAllTableListAsyncBelow(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
    string currentSortColumn = "", string currentSortOrder = "", string url = "", string userId = "", int? organizationId = null,
List<int> departmentIds = null,
List<int> employeeIds = null, DateOnly? fromDate = null, DateOnly? toDate = null);
        Task<List<CommonSelectVM>> GetAllEmployee(string userId);
    }
}
