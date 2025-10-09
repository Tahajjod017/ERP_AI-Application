using System;
using System.Collections.Generic;
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

    public class AllowanceSetupVM
    {
        public int EmployeeAllowanceSetupID { get; set; }
        public decimal SalaryMin { get; set; }
        public decimal SalaryMax { get; set; }
        public int CalculationTypeID { get; set; }
        public decimal Value { get; set; }
    }
}
