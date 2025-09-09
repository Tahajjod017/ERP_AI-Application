using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.PayrollManagements.PayRollEarlyLoanpayment
{
    public class EarlypaymentTableVM
    {
        public int LoanDetailsID { get; set; }
        public int LoanID { get; set; }
        public int? EmployeeID { get; set; }
        public string? EmployeeName { get; set; }
        public decimal? LoanAmount { get; set; }
        public string? TenureMonth { get; set; }
        public decimal? MonthlyEMI { get; set; }
        public string? EmployeeDepartment { get; set; }
        public string? EmployeeImage { get; set; }
        public string? CreatedByName { get; set; }
        public string? PaymentDateTime { get; set; }
        public decimal EarlyPayAmount { get; set; }
    }
}
