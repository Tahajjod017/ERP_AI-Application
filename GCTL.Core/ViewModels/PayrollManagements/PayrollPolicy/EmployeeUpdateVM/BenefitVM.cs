using System;
using System.Collections.Generic;
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

    public class BenefitSetupVM
    {
        public int BenefitSetupID { get; set; }
        public decimal SalaryMin { get; set; }
        public decimal SalaryMax { get; set; }
        public int CalculationTypeID { get; set; }
        public decimal Value { get; set; }
    }


}
