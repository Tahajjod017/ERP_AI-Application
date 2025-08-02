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
        public decimal? RegularHour { get; set; }
        public decimal? OvertimeHour { get; set; }
        public decimal? LateHour { get; set; }
        public decimal? EarlyHour { get; set; }
        public decimal? WorkingHour { get; set; }
        public string Remarks { get; set; }
        public int Status { get; set; }
        public string SpecialDay { get; set; }
    }
}
