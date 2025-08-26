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

        public int? OrganizationID { get; set; }
        public int? EmployeeAllowanceTypeID { get; set; }
        public string? EmployeeAllowanceTypeName { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public List<HouseRentAllowanceDetailGetVM> HouseRentAllowances { get; set; } = new();
    }
    public class HouseRentAllowanceDetailGetVM
    {

        public decimal? SalaryMin { get; set; }

        public decimal? SalaryMax { get; set; }

        public int? CalculationTypeID { get; set; }
        public string? CalculationType { get; set; }
        public decimal? Value { get; set; }
        public DateTime? EffectiveDate { get; set; }

    }

}
