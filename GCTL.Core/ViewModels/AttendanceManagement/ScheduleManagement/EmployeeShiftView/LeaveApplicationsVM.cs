using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.EmployeeShiftView
{
    public class LeaveApplicationsVM
    {
        public int? EmployeeID { get; set; }
        public bool IsFullDay { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? PartialFromTime { get; set; }
        public string? PartialToTime { get; set; }
        public string? LeaveTypeName { get; set; }
        public bool IsFinalApproved { get; set; }
    }
}
