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
        public string? OrganizationName { get; set; }
        public string?  OrganizationAddress { get; set; }
        public string ? EmailAddress { get; set; }
        public decimal Basic { get; set; }
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
    }
}
