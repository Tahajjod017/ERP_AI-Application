using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.AttendenceReportAlls
{
    public class AttendanceSummaryDto
    {
        public int Present { get; set; }
        public int LatePresent { get; set; }
        public int Leave { get; set; }
        public int Absent { get; set; }
        public int Total => Present + LatePresent + Leave + Absent;

        public int PresentPercent { get; set; }
        public int LatePresentPercent { get; set; }
        public int LeavePercent { get; set; }
        public int AbsentPercent { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
    }
}
