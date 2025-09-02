using GCTL.Core.Helpers;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Core.ViewModels.PayrollManagements.LoanManagement;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.PayRollManagements.PayRollLoanManagement
{
    public interface IPayRollLoanEntryService
    {
      Task<CommonReturnViewModel> SaveAsync(LoanSaveVM entityVM);
      Task<CommonReturnViewModel> UpdateAsync(LoanUpdateVM entityVM);

        Task<CommonReturnViewModel> UpdateFromAppDecAsync(PayRollLoanViewDeclineApprovedVM entityVM);
        Task<CommonReturnViewModel> DeleteAsync(DeleteRequestVM deleteRequestVM);
      Task<CommonReturnViewModel> GetByAsync(int id);
        Task<PaginationService<Loan, LoanViewGetAllVM>.PaginationResult<LoanViewGetAllVM>> GetAllTableAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
       string currentSortColumn = "", string currentSortOrder = "", string url = "", string userId = "",  int? organizationId = null, List<int> departmentIds = null, List<int> employeeIds = null);
     Task<List<CommonSelectVM>> SelectAsync();
    Task<CommonReturnViewModel>GetByIdApprovedOrDecline(int id);
    }
}
