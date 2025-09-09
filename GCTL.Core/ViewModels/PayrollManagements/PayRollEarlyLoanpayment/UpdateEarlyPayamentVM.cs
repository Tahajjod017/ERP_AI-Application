using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.PayrollManagements.PayRollEarlyLoanpayment
{
    public class UpdateEarlyPayamentVM:BaseViewModel
    {
        public int LoanID { get; set; }
        public int? EmployeeIDs { get; set; }
        public decimal? LoanAmount { get; set; }
        public DateTime? PaymentDateTime { get; set; }
        public decimal? MonthlyEMI { get; set; }
        public decimal? EarlyPayAmount { get; set; }
        public string? TenureMonth { get; set; }
        public int LoanDetailsID { get; set; }
    }

    
}
