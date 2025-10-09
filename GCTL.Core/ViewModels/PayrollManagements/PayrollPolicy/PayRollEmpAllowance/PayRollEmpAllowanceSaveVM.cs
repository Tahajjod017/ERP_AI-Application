using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollEmpAllowance
{
    
    public class PayRollEmpAllowanceSaveVM : BaseViewModel
    {
        public int OrganizationID { get; set; }
        public List<AllowanceVMSave>? Allowances { get; set; }
    }
    public class AllowanceVMSave
    {
       
        public int EmployeeAllowanceID { get; set; }
        public int? EmployeeAllowanceTypeID { get; set; }
        public bool IsActive { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public List<AllowanceSetupVM>? AllowanceSetups { get; set; }
    }

    public class AllowanceSetupVM:IValidatableObject
    {
        public int EmployeeAllowanceSetupID { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "SalaryMin must be a valid positive number")]
        public decimal SalaryMin { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "SalaryMax must be a valid positive number")]
        public decimal SalaryMax { get; set; }
        public int CalculationTypeID { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Value must be numeric and >= 0")]
        public decimal Value { get; set; }
        public IEnumerable<ValidationResult>Validate(ValidationContext validationContext)
        {
            if(SalaryMin > SalaryMax)
            {
                yield return new ValidationResult(
                   "SalaryMin cannot be greater than SalaryMax",
                   new[] { nameof(SalaryMin), nameof(SalaryMax) });
            }
        }
    }

   
}
