using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.EmployeeAttendence
{
    public class EmployeeStatusReportVM
    {
        public int Present { get; set; }
        public int Absent { get; set; }
        public int Late { get; set; }
        public int Early { get; set; }
        // NEW
        public int DaysElapsed { get; set; }   // days passed in current month (till today)
        public int DaysInMonth { get; set; }   // total days in current month
    }
}
