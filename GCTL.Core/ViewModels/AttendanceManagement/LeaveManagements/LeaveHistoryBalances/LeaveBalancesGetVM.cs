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

        public string? AnnualTaken { get; set; }
        public string? AnnualRemaining { get; set; }

        public string? CasualTaken { get; set; }
        public string? CasualRemaining { get; set; }

        public string? MedicalTaken { get; set; }
        public string? MedicalRemaining { get; set; }

        public string? MaternityTaken { get; set; }
        public string? MaternityRemaining { get; set; }

        public string? PaternityTaken { get; set; }
        public string? PaternityRemaining { get; set; }
    }
}
