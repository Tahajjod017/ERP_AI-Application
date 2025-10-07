using GCTL.Core.Helpers;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.EmployeeBenefitsVM;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.EmployeeUpdateVM;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.PayRollManagements.PayRollPolicy
{
    public interface IEmployeeBenefitsService
    {
        Task<CommonReturnViewModel> SaveEmployeeBenefitsAsync(EmployeeBenefitsVM entityVM);
        Task<List<CommonSelectVMM>> SelectAsync(int id);
        #region Old benefits
        Task<CommonReturnViewModel> SaveEmployeeBenefits(PayRollEmpBenefitsSaveVM entityVM);
        Task<CommonReturnViewModel> UpdateEmployeeBenefits(PayRollEmpBenefitsUpdate entityVM);
        Task<PaginationService<EmployeeBenefits, PayRollEmpBenefitsGetAllVM>.PaginationResult<PayRollEmpBenefitsGetAllVM>> GetAllTableAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
       string currentSortColumn = "", string currentSortOrder = "", int? organizationId = null);
        Task<CommonReturnViewModel> SoftDeletePayRollEmpRequest(DeleteRequestVM deleteRequestVM);
        Task<CommonReturnViewModel> GetById(int employeeBenefitID);
        #endregion

    }
}
