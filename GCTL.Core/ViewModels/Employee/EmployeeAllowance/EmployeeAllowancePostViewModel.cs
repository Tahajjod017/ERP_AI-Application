using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmployeeAllowance
{
    public class EmployeeAllowancePostViewModel : EmployeeBaseViewModel
    {
        public int EmployeeBaseAllowanceID { get; set; }

       // public int? EmployeeID { get; set; }

        public decimal? MobileInternetAllowance { get; set; }

        public bool IsMobileInternetAllowanceEnabled { get; set; }

        public decimal? ShiftAllowance { get; set; }

        public bool IsShiftAllowanceEnabled { get; set; }

        public decimal? HouseRentAllowancePercentage { get; set; }

        public bool IsHouseRentAllowancePercentageEnabled { get; set; }

        public decimal? MedicalAllowancePercentage { get; set; }

        public bool IsMedicalAllowancePercentageEnabled { get; set; }

        public decimal? ConveyanceAllowancePercentage { get; set; }

        public bool IsConveyanceAllowancePercentageEnabled { get; set; }
        public bool IsEmployeeAllowanceEnabled { get; set; }
    }
}
