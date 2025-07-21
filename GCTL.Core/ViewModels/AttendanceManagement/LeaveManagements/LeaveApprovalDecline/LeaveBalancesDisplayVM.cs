using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveApprovalDecline
{
    public class LeaveBalancesDisplayVM
    {
        public int LeaveBalanceID { get; set; }
        public int? EmployeeID { get; set; }
        public int? LeaveTypeID { get; set; }
        public string? LeaveTypeName { get; set; }       
        public decimal? TotalLeave { get; set; }
        public decimal? Taken { get; set; }
        public decimal? RemainingDays { get; set; }     
        public int? ApplicableYear { get; set; }
        public int? DaysTaken { get; set; } 
        public int? HoursTaken { get; set; } 
        public int? MinutesTaken { get; set; }
        public decimal? TakenPartialHours { get; set; }
    }
}
