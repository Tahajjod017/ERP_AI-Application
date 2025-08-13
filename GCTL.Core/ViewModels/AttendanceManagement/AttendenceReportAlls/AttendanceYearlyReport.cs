using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.AttendenceReportAlls
{
    public class AttendanceYearlyReportVM
    {
        public string Month { get; set; }
        public List<AttendanceMonthlyReport> SpecialDays { get; set; }
    }

    public class AttendanceMonthlyReportVM
    {
        public string Date { get; set; }
        public string SpecialDay { get; set; }
    }
}
