using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.EmployeeAttendence
{
    public class MonthlyAttendanceDayVM
    {
        public string? In { get; set; }
        public string? Out { get; set; }
        public string? Break { get; set; }
        public string? Late { get; set; }
        public string? Early { get; set; }
        public string? Ot { get; set; }
        public string? Prod { get; set; }
        public string? Status { get; set; }
    }

    public class MonthlyAttendanceCalendarVM
    {
        public string Month { get; set; } = default!; // "yyyy-MM"
        public Dictionary<string, MonthlyAttendanceDayVM> Data { get; set; } = new();
    }

}
