using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollEmpAllowance;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollEmpSalary;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.PayRollManagements.PayRollEmpSalary
{
    public interface IPayRollEmpSalaryService
    {
        Task<PaginationService<EmployeeBaseBenefits, PayRollEmpSalaryGetAllVM>.PaginationResult<PayRollEmpSalaryGetAllVM>> GetAllTableAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string currentSortColumn = "", string currentSortOrder = "", int? organizationId = null);
    }
}
