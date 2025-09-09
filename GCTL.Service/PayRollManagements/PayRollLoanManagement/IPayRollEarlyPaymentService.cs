using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.PayrollManagements.LoanManagement;
using GCTL.Core.ViewModels.PayrollManagements.PayRollEarlyLoanpayment;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.PayRollManagements.PayRollLoanManagement
{
    public interface IPayRollEarlyPaymentService
    {
        Task<CommonReturnViewModel> GetPayRollEarlyPaymentAsync(int id);
        Task<CommonReturnViewModel> SavePayRollEarlyPaymentAsync(SaveEarlyPaymentVM model);
        Task<PaginationService<LoanDetails, EarlypaymentTableVM>.PaginationResult<EarlypaymentTableVM>> GetAllTableAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
           string currentSortColumn = "", string currentSortOrder = "", string url = "", string userId = "", int? organizationId = null, List<int> departmentIds = null, List<int> employeeIds = null);

        Task<List<CommonSelectVM>> SelectAsync();
        Task<CommonReturnViewModel> GetLaonDetailsAsync(int id);
        Task<CommonReturnViewModel> UpdatePayRollEarlyPaymentAsync(UpdateEarlyPayamentVM model);

        Task<List<CommonSelectVM>> GetDepartmentsByOrgId(int? orgId);
        Task<List<CommonSelectVM>> GetEmployeesByOrgBraDepId(int? orgId, List<int>? branchIds, List<int>? deptIds);
    }
}
