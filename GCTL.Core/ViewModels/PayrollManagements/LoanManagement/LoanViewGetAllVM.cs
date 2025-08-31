using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.PayrollManagements.LoanManagement
{
    public class LoanViewGetAllVM
    {
        public int ? EmployeeID { get; set; }
        public string? EmployeeName { get; set; }
        public string? Department { get; set; }
        public decimal ? LoanAmount { get; set; }
        public decimal ? EmployeeEarlyPayment { get; set; }
        public decimal ? TenureMonth { get; set; }
        public decimal? MonthlyEMI { get; set; }
        public decimal ? OutSatndingbalance { get; set; }
    }
}
