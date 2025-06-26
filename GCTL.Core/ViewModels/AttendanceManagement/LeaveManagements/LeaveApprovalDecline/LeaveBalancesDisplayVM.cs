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
        public string? LeaveTypeName { get; set; }       // ✅ Add this
        public decimal? TotalLeave { get; set; }
        public decimal? Taken { get; set; }
        public decimal? RemainingDays { get; set; }      // ✅ Add this
        public int? ApplicableYear { get; set; }

    }
}
