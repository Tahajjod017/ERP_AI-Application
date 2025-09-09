using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.AttendenceReportAlls
{
    public class AttendanceMonthlyReport
    {
        public string Date { get; set; }
        public string Shift { get; set; }
        public string CheckInTime { get; set; }
        public string CheckOutTime { get; set; }
        public TimeOnly? RegularHour { get; set; }
        public TimeOnly? OvertimeHour { get; set; }
        public TimeOnly? LateHour { get; set; }
        public TimeOnly? EarlyHour { get; set; }
        public TimeOnly? WorkingHour { get; set; }
        public string Remarks { get; set; }
        public int Status { get; set; }
        public string SpecialDay { get; set; }
    }
}
