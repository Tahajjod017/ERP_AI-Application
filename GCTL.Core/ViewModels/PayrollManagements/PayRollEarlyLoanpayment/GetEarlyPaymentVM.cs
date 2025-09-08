using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.PayrollManagements.PayRollEarlyLoanpayment
{
    public class GetEarlyPaymentVM
    {
        public int LoanID { get; set; }

        public int? EmployeeID { get; set; }

        public decimal? LoanAmount { get; set; }

        public int? LoanInstallmentPeriodID { get; set; }

        public DateTime? IssueDate { get; set; }

        public DateTime? StartDate { get; set; }
    }
}
