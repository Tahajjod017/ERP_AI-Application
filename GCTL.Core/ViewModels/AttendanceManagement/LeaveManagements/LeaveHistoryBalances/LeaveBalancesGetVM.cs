using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveHistoryBalances
{
    public class LeaveBalancesGetVM
    {
        public string? EmployeeName { get; set; }
        public string? EmployeeDesignation { get; set; }

        public string? EmployeeImage { get; set; }
        public string? LeaveType { get; set; }
        public string? EmployeeDepartment { get; set; }

        public decimal? AnnualTaken { get; set; }
        public decimal? AnnualRemaining { get; set; }

        public decimal? CasualTaken { get; set; }
        public decimal? CasualRemaining { get; set; }

        public string? MedicalTaken { get; set; }
        public string? MedicalRemaining { get; set; }

        public decimal? MaternityTaken { get; set; }
        public decimal? MaternityRemaining { get; set; }

        public decimal? PaternityTaken { get; set; }
        public decimal? PaternityRemaining { get; set; }
    }
}
