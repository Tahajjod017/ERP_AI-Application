using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollEmpSalary
{
    public class PayRollEmpSalaryGetAllVM
    {
        public int ? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string ? EmpDepartment {  get; set; }
        public decimal? Salary {  get; set; }
        public decimal? Bonus { get; set; }
        public decimal? Deductions { get; set; }
        public decimal ? NetSalary { get; set; }
        public string? EmployeeImage { get; set; }
        public decimal? Deduction { get; set; } 
    }
}
