using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.EmployeeBenefitsVM
{
    public class PayRollEmpBenefitsGetByID
    {
        public int EmployeeBenefitID { get; set; }

        public int? OrganizationID { get; set; }

        public bool? IsHealthInsuranceEnabled { get; set; }

        public decimal? HealthInsurance { get; set; }

        public bool? IsPerformanceBonusEnabled { get; set; }

        public decimal? PerformanceBonus { get; set; }

        public bool? IsFastivalBonusEnabled { get; set; }


        public decimal? FastivalBonusRate { get; set; }

        public int? FastivalBonusOnSalaryTypeID { get; set; }

        public bool? IsProvidentFundEnabled { get; set; }

        public decimal? ProvidentFundEmployeeContrebution { get; set; }

        public decimal? ProvidentFundOrganizationContrebution { get; set; }

        public int? ProvidentFundOnSalaryTypeID { get; set; }

        public decimal? ProvidentFundMinimumServiceYear { get; set; }
        public decimal? FastivalBonusMinimumServiceInMonth { get; set; }
        public bool? IsYearEndBonusEnabled { get; set; }
        public int? YearlyEndBonusTypeID { get; set; }
    }
}
