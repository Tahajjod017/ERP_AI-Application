using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.AttendenceReportAlls
{
    public class YearlySpecialDayReportVM
    {
        public string Month { get; set; }
        public List<SpecialDay> SpecialDays { get; set; }
        public int TotalPresent { get; set; }
        public int TotalAbsent { get; set; }
        public int TotalLeave { get; set; }
        public int TotalWeekend { get; set; }
        public int TotalHoliday { get; set; }
    }
    public class SpecialDay
    {
        public string Date { get; set; }  // Date in 'dd' format
        public string SpecialDays { get; set; }  // Workday, Weekend, Leave, Holiday
    }
}
