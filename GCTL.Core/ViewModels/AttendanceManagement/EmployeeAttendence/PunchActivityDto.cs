using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.EmployeeAttendence
{
    public class PunchActivityDto
    {
        public string? Type { get; set; }       // "Punch In" or "Punch Out"
        public string? Time { get; set; }       // e.g., "10:00 AM"
        public string? Description { get; set; } // same as Type or additional info
    }
}
