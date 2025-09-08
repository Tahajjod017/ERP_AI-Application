using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.PayrollManagements.PayRollEarlyLoanpayment
{
    public class SaveEarlyPaymentVM:BaseViewModel
    {
        public int LoanID { get; set; }
        public List<int>? EmployeeIDs { get; set; }
        public decimal? LoanAmount { get; set; }
        public int? LoanInstallmentPeriodID { get; set; }
        public DateTime? PaymentDateTime { get; set; }
        public int? OrganizationID { get; set; }
        public List<int>? DepartmentIDs { get; set; }
        public decimal? MonthlyEMI { get; set; }
        public decimal ? EarlyPayAmount { get; set; }
        public string ? TenureMonth {  get; set; }
    }
}
