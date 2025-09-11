using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollEmpSalary
{
    public class PayRollPaySlipEmpVM
    {
        public string? EmployeeName { get; set; }
        public string? EmployeeAddress { get; set; }
        public string? EmployeeEmail { get; set; }
        public string? OrganizationName { get; set; }
        public string?  OrganizationAddress { get; set; }
        public string ? OrganizationEmailAddress { get; set; }
        public string ? OrganizationLogoPic {  get; set; }
        public decimal? BasicSalary { get; set; }
        public decimal HRA { get; set; }
        public decimal DA { get; set; }
        public decimal SpecialAllowance { get; set; }
        public decimal Bonus { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal ProvidentFund { get; set; }
        public decimal ProfessionalTax { get; set; }
        public decimal ESI { get; set; }
        public decimal HomeLoan { get; set; }
        public decimal TDS { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetPay { get; set; }
        public string? PayslipNo { get; set; }
        public DateTime PaymentDate { get; set; }
        public List<AllowanceVM> Allowances { get; set; } = new();
        public decimal TotalSalary { get; set; }
        public string? SalaryInWords { get; set; }
        public decimal TotalBonus { get; set; }
    }
    public class AllowanceVM
    {
        public string? Type { get; set; }
        public decimal Amount { get; set; }
        public string? DisplayValue { get; set; }
        public decimal? AllowanceSalary { get; set; }
    }
}
