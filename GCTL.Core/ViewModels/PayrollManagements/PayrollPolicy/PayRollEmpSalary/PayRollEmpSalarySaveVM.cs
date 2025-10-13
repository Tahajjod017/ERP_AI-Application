using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollEmpSalary
{
    public class PayRollEmpSalarySaveVM:BaseViewModel
    {

        public int? EmployeeID { get; set; }
        public  List<int> ? EmployeeIDs {  get; set; }
        public DateOnly PayPeriodStart { get; set; }

        public DateOnly PayPeriodEnd { get; set; }

        public decimal BasicSalary { get; set; }

        public bool? IsPaid { get; set; }=true;
    }
}
