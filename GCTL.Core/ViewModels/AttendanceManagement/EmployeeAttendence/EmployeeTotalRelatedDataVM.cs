using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.EmployeeAttendence
{
    public class EmployeeTotalRelatedDataVM
    {
        public string CurrentTime { get; set; }
        public string ProductionTime { get; set; }
        public double TotalWorkingHours { get; set; }
        public double Overtime { get; set; }
    }
}
