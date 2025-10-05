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
        Task<List<CommonSelectVMM>> SelectAsync(int id);
    }

    public class CommonSelectVMM:BaseViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public List<EmpAllowanceVMM> EmpAllowanceVMM { get; set; } = new List<EmpAllowanceVMM>();
    }

    public class EmpAllowanceVMM
    {
        public int EmployeeAllowanceID { get; set; }
        public int? OrganizationID { get; set; }
        public int? EmployeeAllowanceTypeID { get; set; }
        public string? EmployeeAllowanceTypeName { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public List<EmpAllowanceSetupVMM> EmployeeAllowanceSetups { get; set; } = new List<EmpAllowanceSetupVMM>();
    }

    public class EmpAllowanceSetupVMM
    {
        public int EmployeeAllowanceSetupID { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public int? CalculationTypeID { get; set; }
        public decimal? Value { get; set; }
    }
    
}
