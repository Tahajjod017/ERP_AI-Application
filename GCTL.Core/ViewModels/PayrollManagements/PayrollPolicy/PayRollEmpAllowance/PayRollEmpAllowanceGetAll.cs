using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollEmpAllowance
{
    public class PayRollEmpAllowanceGetAll
    {
        public int EmployeeAllowanceID { get; set; }
        public string? OrganizationName { get; set; }

        public decimal? MobileInternetAllowance { get; set; }

        public decimal? ShiftAllowance { get; set; }

        public decimal? HouseRentAllowanceRate { get; set; }

        public string? HRentDependsOnSalaryTypeIDName { get; set; }

        public decimal? MedicalAllowanceRate { get; set; }

        public string? MediAllowDepOnSalaryTypeIDName { get; set; }

        public decimal? ConveyanceAllowanceRate { get; set; }

        public string? ConAllowDepOnSalaryTypeIDName { get; set; }
    }
}
