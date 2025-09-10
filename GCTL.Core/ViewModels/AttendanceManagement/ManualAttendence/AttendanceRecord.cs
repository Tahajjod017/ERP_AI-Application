using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ManualAttendence
{
    public class AttendanceRecord
    {
        public bool isOvertimeEligible;

        public int Id { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeRole { get; set; }
        public string Department { get; set; }
        public string EmployeeImage { get; set; }
        public string AttendanceDate { get; set; }
        public string ScheduleTime { get; set; }
        public string ActualInTime { get; set; }
        public string ActualOutTime { get; set; }
        public string BreakInTime { get; set; }
        public string BreakOutTime { get; set; }
        public int? Overtime { get; set; }
        public int BiometricHits { get; set; }
        public string PossibleReason { get; set; }
        public List<PunchData> PunchData { get; set; }
        public int? EmployeeId { get; set; }
        public TimeOnly? GraceTime { get; set; }
        public TimeOnly? MaximumOverTime { get; set; }
        public TimeOnly? MinimumOverTime { get; set; }
        public TimeOnly? MinimumWorkHour { get; set; }
        public bool IsOnFullLeave { get; set; }
        public string? PartialLeaveTimeRange { get; set; }
        public bool IsPartialLeave { get; set; }
        public string AbnormalType { get; set; }
    }

    
}
