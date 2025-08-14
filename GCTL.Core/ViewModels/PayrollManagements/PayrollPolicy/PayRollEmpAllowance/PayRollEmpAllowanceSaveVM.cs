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

        public bool IsMobileInternetAllowanceEnabled { get; set; }

        public decimal? MobileInternetAllowance { get; set; }

        public bool IsShiftAllowanceEnabled { get; set; }

        public decimal? ShiftAllowance { get; set; }

        public bool IsHouseRentAllowanceEnabled { get; set; }

        public decimal? HouseRentAllowanceRate { get; set; }

        public int? HRentDependsOnSalaryTypeID { get; set; }

        public bool IsMedicalAllowanceEnabled { get; set; }

        public decimal? MedicalAllowanceRate { get; set; }

        public int? MediAllowDepOnSalaryTypeID { get; set; }

        public bool IsConveyanceAllowanceEnabled { get; set; }

        public decimal? ConveyanceAllowanceRate { get; set; }

        public int? ConAllowDepOnSalaryTypeID { get; set; }

    }
}
