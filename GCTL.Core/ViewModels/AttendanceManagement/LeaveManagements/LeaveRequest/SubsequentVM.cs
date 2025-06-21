using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest
{
    public class SubsequentVM
    {
        public bool IsHolidayCountedAsLeave { get; set; }
        public bool IsWeekendCountedAsLeave { get; set; }
        public int ? TotalSubsequentDays { get; set; }  
        
    }
}
