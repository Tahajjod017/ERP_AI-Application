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

    public class CommonSelectVMM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<EmpAllowanceVMM> EmpAllowanceVMM { get; set; } = new List<EmpAllowanceVMM>();
    }

    public class EmpAllowanceVMM
    {
        public int EmployeeAllowanceID { get; set; }
        public int? OrganizationID { get; set; }
        public int? EmployeeAllowanceTypeID { get; set; }
        public string EmployeeAllowanceTypeName { get; set; }
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
        public DateTime? EffectiveDate { get; set; }
    }
    //public class CommonSelectVMM
    //{
    //    public int? Id { get; set; }
    //    public string? Name { get; set; }
    //    public List<EmpBenefitVMM>? EmpBenefitVMM { get; set; } = new List<EmpBenefitVMM>();
    //}

    //public class EmpBenefitVMM
    //{
    //    public int BenefitID { get; set; }
    //    public int? OrganizationID { get; set; }
    //    public int? BenefitTypeID { get; set; }
    //    public string? BenefitTypeName { get; set; }
    //    public bool? IsActive { get; set; }
    //    public DateTime? EffectiveDate { get; set; }
    //    public List<EmpBenefitSetupVMM> BenefitSetups { get; set; } = new List<EmpBenefitSetupVMM>();
    //}

    //public class EmpBenefitSetupVMM
    //{
    //    public int? BenefitSetupID { get; set; }
    //    public decimal? SalaryMin { get; set; }
    //    public decimal? SalaryMax { get; set; }
    //    public int? CalculationTypeID { get; set; }
    //    public decimal? Value { get; set; }
    //}
}
