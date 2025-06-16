using GCTL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveSettings
{
    public class AddLeavePolicyConfigarationVM:BaseViewModel
    {
     

        public bool IsWeekendCountedAsLeave { get; set; }

        public bool IsHolidayCountedAsLeave { get; set; }

        public bool IsExceedLeaveBalance { get; set; }

        public string? RoundOffHour { get; set; }

        public bool IsRoundOffHour { get; set; }
        public bool IsAllowRequestForPastDates { get; set; }

        public int? AllowRequestForFutureDays { get; set; }

        public int? MaximumleaveDaysPerAplication { get; set; }

        public int? MaximumGapDaysBetweenAplications { get; set; }

        public bool? IsAllowRequestForFutureDays { get; set; }

        public bool? IsMaximumleaveDaysPerAplication { get; set; }

        public bool? IsMaximumGapDaysBetweenAplications { get; set; }

    }
}
