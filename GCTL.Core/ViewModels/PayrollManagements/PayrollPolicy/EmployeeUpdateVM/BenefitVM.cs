using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.EmployeeUpdateVM
{
    public class EmployeeBenefitsVM:BaseViewModel
    {
        public int OrganizationID { get; set; }
        public List<BenefitVM>? Benefits { get; set; }
    }
    public class BenefitVM
    {
        public int BenefitID { get; set; }
        public int BenefitTypeID { get; set; }
        public bool IsActive { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public List<BenefitSetupVM>? BenefitSetups { get; set; }
    }


    public class BenefitSetupVM : IValidatableObject
    {
        public int BenefitSetupID { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "SalaryMin must be a valid positive number")]
        public decimal SalaryMin { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "SalaryMax must be a valid positive number")]
        public decimal SalaryMax { get; set; }

        public int CalculationTypeID { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Value must be numeric and >= 0")]
        public decimal Value { get; set; }

        // ✅ Custom validation
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (SalaryMin > SalaryMax)
            {
                yield return new ValidationResult(
                    "SalaryMin cannot be greater than SalaryMax",
                    new[] { nameof(SalaryMin), nameof(SalaryMax) });
            }
        }
    }



    #region for get 
    public class CommonSelectVMM
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public List<EmpBenefitVMM>? EmpBenefitVMM { get; set; } = new List<EmpBenefitVMM>();
    }

    public class EmpBenefitVMM
    {
        public int BenefitID { get; set; }
        public int? OrganizationID { get; set; }
        public int? BenefitTypeID { get; set; }
        public string? BenefitTypeName { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public List<EmpBenefitSetupVMM> BenefitSetups { get; set; } = new List<EmpBenefitSetupVMM>();
    }

    public class EmpBenefitSetupVMM
    {
        public int? BenefitSetupID { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public int? CalculationTypeID { get; set; }
        public decimal? Value { get; set; }
    }

    #endregion

}
