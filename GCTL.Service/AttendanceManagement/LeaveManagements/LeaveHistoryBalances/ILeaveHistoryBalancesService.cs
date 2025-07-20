using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveHistoryBalances;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.LeaveManagements.LeaveHistoryBalances
{
    public interface ILeaveHistoryBalancesService
    {
        Task<PaginationService<LeaveBalances, LeaveBalancesGetVM>.PaginationResult<LeaveBalancesGetVM>> GetAllTableAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
      string currentSortColumn = "", string currentSortOrder = "", string url = "", string userId = "", int? leaveTypeID = null, int? statusID = null, int? organizationId = null,
  List<int> departmentIds = null,
  List<int> employeeIds = null, DateOnly? fromDate = null, DateOnly? toDate = null);
    }
}
