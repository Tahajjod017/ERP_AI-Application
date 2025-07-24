using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.Employee.EmpTransfer;
using GCTL.Data.Models;
using GCTL.Service.Pagination;

namespace GCTL.Service.Employees.EmpTransfer
{
    public interface IEmployeeTransferService
    {
        Task<PaginationService<EmployeeTransfer, EmployeeTransferTableListVM>.PaginationResult<EmployeeTransferTableListVM>> GetAllTableAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
      string currentSortColumn = "", string currentSortOrder = "", string url = "", string userId = "",  int? organizationId = null,
  List<int> departmentIds = null,
  List<int> employeeIds = null, DateOnly? fromDate = null, DateOnly? toDate = null);
        Task<CommonReturnViewModel> SaveEmployeeTansferAsync(EmployeeTransferAddVM entityVM);
    }
}
