using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.EmployeeAttendence
{
    public class EmployeeAttendenceVM :BaseViewModel
    {
        public int AttendanceID { get; set; }

        public int? EmployeeID { get; set; }
        public string? EmployeeName { get; set; }

        public string? AttendanceDate { get; set; }

        public int? StatusID { get; set; }
        public string? StatusName { get; set; }

        public int? ShiftID { get; set; }
        public string? ShiftName { get; set; }
        public string? ShiftStartTime { get; set; }

        public string? CheckInTime { get; set; }
        public bool HasCheckIn { get; set; }
        public string? CheckInShiftTime { get; set; } // 

        public string? CheckOutTime { get; set; }
        public string? CheckOutShiftTime { get; set; } //

        public string? Remarks { get; set; }
        public string? RegularHour { get; set; }

        public string? OvertimeHour { get; set; }
        public string? OverTimeHm { get; set; }

        public string? LateHour { get; set; }

        public string? EarlyHour { get; set; }
        public string? Break { get; set; }
        public string? WorkingHours { get; set; }
        public string? TotalWorkingHours { get; set; }
        public string? ProductiveHours { get; set; }
        public string? BreakHours { get; set; }
        public string? LateHours { get; set; }
        public string? EarlyHours { get; set; }
        public string? TotalWorkingHoursMnt { get; set; }
        public string? ActualWorkingHrsMnt { get; set; }
        public string? TodayWorkHm { get; set; }
        public string? CurrentTime { get; set; }
        public string? JobTitle { get; set; } // Added JobTitle field
        // Add the following fields:
        public string? ProductionTime { get; set; }  // Calculated production time (CheckInTime to CurrentTime)
        public string? ProductionTimeMinute { get; set; }  // Calculated production time (CheckInTime to CurrentTime)
        public string? Overtime { get; set; }        // Calculated overtime if the production time exceeds working hours
        public string? EarlyStartTime { get; set; } // Calculated early start time if CheckInTime is before ShiftStartTime
        public List<SessionData>? SessionTimeline { get; set; }
        // "HH:mm" strings used by the client timeline labels
       // public string ShiftStartTime { get; set; } = "-";
        public string TimelineStartTime { get; set; } = "-";
    }
    public class SessionData
    {
        public string? Type { get; set; }
        public int DurationMinutes { get; set; }
        public string? Duration { get; set; }
        public string? Percentage { get; set; }
        public string StartLocal { get; set; } = "";        // "HH:mm"
        public string EndLocal { get; set; } = "";
    }
}
