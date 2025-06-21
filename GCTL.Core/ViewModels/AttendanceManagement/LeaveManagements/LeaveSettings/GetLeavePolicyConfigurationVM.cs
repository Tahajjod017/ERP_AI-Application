using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveSettings
{
    public class GetLeavePolicyConfigurationVM
    {
        public bool IsWeekendCountedAsLeave { get; set; }

        public bool IsHolidayCountedAsLeave { get; set; }
    }
}
