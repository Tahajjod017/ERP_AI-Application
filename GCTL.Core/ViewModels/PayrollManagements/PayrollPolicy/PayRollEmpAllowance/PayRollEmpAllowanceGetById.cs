using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollEmpAllowance
{
    public class PayRollEmpAllowanceGetById
    {
        public int EmployeeAllowanceID { get; set; }
        public int? OrganizationIDEdit { get; set; }

        public bool? IsMobileInternetAllowanceEnabledEdit { get; set; }

        public decimal? MobileInternetAllowanceEdit { get; set; }

        public bool? IsShiftAllowanceEnabledEdit { get; set; }

        public decimal? ShiftAllowanceEdit { get; set; }

        public bool? IsHouseRentAllowanceEnabledEdit { get; set; }

        public decimal? HouseRentAllowanceRateEdit { get; set; }

        public int? HRentDependsOnSalaryTypeIDEdit { get; set; }

        public bool? IsMedicalAllowanceEnabledEdit { get; set; }

        public decimal? MedicalAllowanceRateEdit { get; set; }

        public int? MediAllowDepOnSalaryTypeIDEdit { get; set; }

        public bool? IsConveyanceAllowanceEnabledEdit { get; set; }

        public decimal? ConveyanceAllowanceRateEdit { get; set; }

        public int? ConAllowDepOnSalaryTypeIDEdit { get; set; }
    }
}
