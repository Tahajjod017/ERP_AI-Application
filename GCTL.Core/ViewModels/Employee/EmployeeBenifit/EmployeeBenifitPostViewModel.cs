using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmployeeBenifit
{
    public class EmployeeBenifitPostViewModel : BaseViewModel
    {
        public int EmployeePersonalId { get; set; }
        public string? PersonalPhone { get; set; }
        public string? PersonalEmail { get; set; }


        public int EmployeeBaseBenefitID { get; set; }

        

        public decimal? HealthInsurance { get; set; }

        public bool IsHealthInsuranceEnabled { get; set; }

        public decimal? PerformanceBonus { get; set; }

        public bool IsPerformanceBonusEnabled { get; set; }

        public int? YearlyEndBonusTypeID { get; set; }

        public bool IsYearlyEndBonusTypeIDEnabled { get; set; }

        public decimal? FastivalBonusPercentage { get; set; }

        public bool IsFastivalBonusPercentageEnabled { get; set; }

        public decimal? ProvidantFundEmployeePercentage { get; set; }

        public decimal? ProvidantFundOrganizationPercentage { get; set; }

        public bool IsProvidantFundEnabled { get; set; }

        public int? ServiceYearID { get; set; }

        
    }
}
