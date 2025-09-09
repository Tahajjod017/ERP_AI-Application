using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.PayrollManagements.LoanManagement
{
    public class LoanViewGetAllVM
    {
        public int LoanID { get; set; }
        public int ? EmployeeID { get; set; }
        public string? EmployeeName { get; set; }
        public decimal ? LoanAmount { get; set; }
        public decimal ? EmployeeEarlyPayment { get; set; }
        public string ? TenureMonth { get; set; }
        public decimal? MonthlyEMI { get; set; }
        public decimal ? OutSatndingbalance { get; set; }
        public string? EmployeeDepartment { get; set; }
        public string? EmployeeImage { get; set; }
        public string? ApplicationDate { get; set; }
        public string? StatusName { get; set; }
        public int? ApproverStep { get; set; }
    }
}
