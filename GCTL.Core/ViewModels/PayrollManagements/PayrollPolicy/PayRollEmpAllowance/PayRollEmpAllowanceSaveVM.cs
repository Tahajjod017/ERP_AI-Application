using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollEmpAllowance
{
    public class PayRollEmpAllowanceSaveVM : BaseViewModel
    {

        public int? OrganizationID { get; set; }
        public int? EmployeeAllowanceTypeID { get; set; }
        public bool IsActive { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public List<HouseRentAllowanceDetailVM> HouseRentAllowances { get; set; } = new();
    }
    public class HouseRentAllowanceDetailVM
    {

         public bool IsActive { get; set; }
        public decimal? SalaryMin { get; set; }

        public decimal? SalaryMax { get; set; }

        public int? CalculationTypeID { get; set; }

        public decimal? Value { get; set; }

        public int? EmployeeAllowanceTypeID { get; set; }
    }

}
