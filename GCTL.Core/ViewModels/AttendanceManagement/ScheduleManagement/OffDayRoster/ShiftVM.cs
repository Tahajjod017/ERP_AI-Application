using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.OffDayRoster
{
    public class ShiftVM
    {
        public int? ShiftID { get; set; }
        public string? ShiftName { get; set; }
        public string? TimeRange { get; set; }
        public int? RosterInHolyDayID { get; set; }
    }
}
