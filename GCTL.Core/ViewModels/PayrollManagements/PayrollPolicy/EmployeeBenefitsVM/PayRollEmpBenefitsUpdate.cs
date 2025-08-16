using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.EmployeeBenefitsVM
{
    public class PayRollEmpBenefitsUpdate : BaseViewModel
    {
        public int EmployeeBenefitIDEdit { get; set; }

        [Required(ErrorMessage = "Please Select Organization")]
        public int? OrganizationIDEdit { get; set; }

        public bool IsHealthInsuranceEnabledEdit { get; set; }

        public decimal? HealthInsuranceEdit { get; set; }

        public bool IsPerformanceBonusEnabledEdit { get; set; }

        public decimal? PerformanceBonusEdit { get; set; }

        public bool IsFastivalBonusEnabledEdit { get; set; }

        public decimal? FastivalBonusRateEdit { get; set; }

        public int? FastivalBonusOnSalaryTypeIDEdit { get; set; }

        public bool IsProvidentFundEnabledEdit { get; set; }

        public decimal? ProvidentFundEmployeeContrebutionEdit { get; set; }

        public decimal? ProvidentFundOrganizationContrebutionEdit { get; set; }

        public int? ProvidentFundOnSalaryTypeIDEdit { get; set; }

        public int? ProvidentFundMinimumServiceYearEdit { get; set; }

        public int? FastivalBonusMinimumServiceInMonthEdit { get; set; }

        public bool IsYearEndBonusEnabledEdit { get; set; }

        public int? YearlyEndBonusTypeIDEdit { get; set; }
    }
}
