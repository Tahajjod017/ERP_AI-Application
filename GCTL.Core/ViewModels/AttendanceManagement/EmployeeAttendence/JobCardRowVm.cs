using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.EmployeeAttendence
{
    public class JobCardRowVm
    {
        public DateTime Date { get; set; }
        public string DayName { get; set; } = "";
        public string ShiftName { get; set; } = "";
        public string InTime { get; set; } = "-";
        public string Late { get; set; } = "-";
        public string OutTime { get; set; } = "-";
        public string EarlyOut { get; set; } = "-";
        public string WorkHours { get; set; } = "-";
        public string OvertimeHours { get; set; } = "-";
        public string StatusCode { get; set; } = "-";   // P, L, A, W, H, etc.
        public string Remarks { get; set; } = "-";
    }

    public class JobCardReportVm
    {
        // Header info
        public string EmployeeCode { get; set; } = "";
        public string EmployeeName { get; set; } = "";
        public string Designation { get; set; } = "";
        public string Department { get; set; } = "";
        public string CompanyName { get; set; } = "GCTL Infosys";
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        // Detail rows
        public List<JobCardRowVm> Rows { get; set; } = new();

        // Summary
        public int TotalPresent { get; set; }
        public int TotalAbsent { get; set; }
        public int TotalLate { get; set; }       // count of late days
        public int TotalLeave { get; set; }      // CL / SL / etc.
        public int TotalHoliday { get; set; }
        public int TotalWeekend { get; set; }
        public int TotalAttendance { get; set; } // P + L + etc.
    }

}
