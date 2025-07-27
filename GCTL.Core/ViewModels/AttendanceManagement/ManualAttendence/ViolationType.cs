using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ManualAttendence
{
    public enum ViolationType
    {
        [Display(Name = "Leave Violation")]
        LeaveViolation,

        [Display(Name = "Schedule Violation")]
        ScheduleViolation,

        [Display(Name = "OUT Missing")]
        OutMissing,

        [Display(Name = "IN Missing")]
        InMissing,

        [Display(Name = "Timing")]
        Timing, 

        [Display(Name = "Duration")]
        Duration,

        [Display(Name = "Overtime")]
        Overtime
    }
}
