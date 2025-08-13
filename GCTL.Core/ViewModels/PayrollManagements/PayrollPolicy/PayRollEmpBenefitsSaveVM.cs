using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy
{
    public class PayRollEmpBenefitsSaveVM:BaseViewModel
    {
       
        public int EmployeeBenefitID { get; set; }
        [Required(ErrorMessage ="Please Select Organization")]
        public int? OrganizationID { get; set; }

        public bool IsHealthInsuranceEnabled { get; set; }

        public decimal? HealthInsurance { get; set; }

        public bool IsPerformanceBonusEnabled { get; set; }

        public decimal? PerformanceBonus { get; set; }

        public bool IsFastivalBonusEnabled { get; set; }

        public decimal? FastivalBonusRate { get; set; }

        public int? FastivalBonusOnSalaryTypeID { get; set; }

        public bool IsProvidentFundEnabled { get; set; }

        public decimal? ProvidentFundEmployeeContrebution { get; set; }

        public decimal? ProvidentFundOrganizationContrebution { get; set; }

        public int? ProvidentFundOnSalaryTypeID { get; set; }

        public int? ProvidentFundMinimumServiceYear { get; set; }
        public int? FastivalBonusMinimumServiceInMonth { get; set; }
        public bool IsYearEndBonusEnabled { get; set; }
        public int? YearlyEndBonusTypeID { get; set; }
    }
}
