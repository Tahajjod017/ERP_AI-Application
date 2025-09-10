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
        public int? RegularHour { get; set; }
        public int? OvertimeHour { get; set; }
        public int? LateHour { get; set; }
        public int? EarlyHour { get; set; }
        public int? WorkingHour { get; set; }
        public string Remarks { get; set; }
        public int Status { get; set; }
        public string SpecialDay { get; set; }
    }
}
