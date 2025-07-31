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
    }
    public class SpecialDay
    {
        public string Date { get; set; }  // Date in 'dd' format
        public string SpecialDays { get; set; }  // Workday, Weekend, Leave, Holiday
    }
}
