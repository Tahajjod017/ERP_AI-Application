using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveApprovalDecline
{
    public class LeaveApplicationApprovalModifyVM:BaseViewModel
    {
        public int LeaveApplicationID { get; set; }
        public int? EmployeeIDEdit { get; set; }
        public int? LeaveTypeIDEdit { get; set; }
        public double LeaveDaysEdit { get; set; }
        public bool IsFullDayEdit { get; set; }
        public DateOnly? FromDateEdit { get; set; }
        public DateOnly? ToDateEdit { get; set; }

        public DateTime? ToDateFromDateCombinedEdit { get; set; }
        public TimeOnly? PartialFromTimeEdit { get; set; }
        public TimeOnly? PartialToTimeEdit { get; set; }
        public string? ReasonEdit { get; set; }
        public double? Period { get; set; }

        public int? TotalSubsequentDays { get; set; }
        public bool IsHolidayCountedAsLeave { get; set; }
        public bool IsWeekendCountedAsLeave { get; set; }
    }
}
