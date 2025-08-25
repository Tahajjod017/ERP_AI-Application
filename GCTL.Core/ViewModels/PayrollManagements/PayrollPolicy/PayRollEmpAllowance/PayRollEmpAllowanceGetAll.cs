using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollEmpAllowance
{
    public class PayRollEmpAllowanceGetAll
    {
        public int EmployeeAllowanceID { get; set; }
        public string? OrganizationName { get; set; }
       public List<AllowanceTypeVM> allowanceTypes { get; set; } = new(); 
        
    }

   public class AllowanceTypeVM
    {
        public int EmployeeAllowanceID { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public bool IsActive { get; set; }
        public List<DetailsVM> detailsVMs { get; set; } = new();
    }
    public class DetailsVM
    {
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        public decimal? Value { get; set; }
        public int ? CalculationType { get; set; }
    }
}
