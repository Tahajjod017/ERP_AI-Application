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

        public bool? IsPaid { get; set; }

        //public List<AllowanceVM> Allowances { get; set; } = new();
        //public List<BeneFitsVM> BeneFits { get; set; } = new();
        //public List<DeductionVM> Deductions { get; set; } = new();
    }

    //public class DeductionVM
    //{
    //    public string? DeductionName { get; set; }
    //    public decimal Amount { get; set; }
    //    public bool IsPercentage { get; set; }
    //}


    public class PaySlipRequestVM: BaseViewModel
    {
        public List<PaySlipEmployeeVM> Employees { get; set; } = new();
    }

    public class PaySlipEmployeeVM
    {
        public int EmployeeID { get; set; }
        public bool IsPaid { get; set; }
    }

}
