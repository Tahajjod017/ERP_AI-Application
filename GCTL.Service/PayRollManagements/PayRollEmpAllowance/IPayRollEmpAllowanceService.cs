using GCTL.Core.Helpers;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.EmployeeBenefitsVM;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollEmpAllowance;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.PayRollManagements.PayRollEmpAllowance
{
    public interface IPayRollEmpAllowanceService
    {
        Task<CommonReturnViewModel>SavePayRollEmpAllowance(PayRollEmpAllowanceSaveVM entityVM);
        Task<CommonReturnViewModel> UpdatePayRollEmpAllowance(PayRollEmpAllowanceUpdate entityVM);
        Task<CommonReturnViewModel> GetByIdPayRollEmpAllowance(int employeeAllowanceID);
        Task<CommonReturnViewModel> SoftDeletePayRollEmpAllowance(DeleteRequestVM deleteRequestVM);
        public Task<List<AllowanceTypeNameVM>> GetEmpAllowanceType();
        Task<CommonReturnViewModel> GetPayRollEmpAllowanceByIdAsync();

    }
}
