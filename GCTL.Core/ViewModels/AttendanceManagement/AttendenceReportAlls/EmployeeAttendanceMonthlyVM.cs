using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.AttendenceReportAlls
{
    public class EmployeeAttendanceMonthlyVM
    {
        public int AttendanceID { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string Designation { get; set; }
        public DateTime AttendanceDate { get; set; }
        public string Status { get; set; }  // "P" for Present, "A" for Absent, "W" for Weekend, etc.
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string Remarks { get; set; }
    }
}
