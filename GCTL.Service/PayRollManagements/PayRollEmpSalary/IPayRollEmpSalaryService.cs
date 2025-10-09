using GCTL.Core.ViewModels;
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
        Task<PaginationService<EmployeeSalarySettings, PayRollEmpSalaryGetAllVM>.PaginationResult<PayRollEmpSalaryGetAllVM>> GetAllTableAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string currentSortColumn = "", string currentSortOrder = "", int? organizationId = null, string imgSrcThumb = null,List<int> ? deptID=null, List<int> ? empID=null);
        Task<CommonReturnViewModel> GetPaySlip(int id);
        Task<CommonReturnViewModel> SaveAsync(PayRollEmpSalarySaveVM entityVM);
    }
}
