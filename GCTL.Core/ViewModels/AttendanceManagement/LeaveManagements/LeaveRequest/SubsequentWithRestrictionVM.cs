using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest
{
    public class SubsequentWithRestrictionVM
    {
        public bool IsHolidayCountedAsLeave { get; set; }
        public bool IsWeekendCountedAsLeave { get; set; }
        public int? TotalSubsequentDays { get; set; }
        public int TotalDays { get; set; }
        //
        public bool IsAllowRequestForPastDates { get; set; }

        public bool? IsAllowRequestForFutureDays { get; set; }
        public int? AllowRequestForFutureDays { get; set; }

        public bool? IsMaximumleaveDaysPerAplication { get; set; }
        public int? MaximumleaveDaysPerAplication { get; set; }

        public bool? IsMaximumGapDaysBetweenAplications { get; set; }
        public int? MaximumGapDaysBetweenAplications { get; set; }
        public string? Message { get; set; }
        public string? MaxGapdaysMessage { get; set; }
        public int LeaveDays {  get; set; } 
    }
}
