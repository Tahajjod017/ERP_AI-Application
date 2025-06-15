using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.AssignDefaultShift
{
    public class ConflictViewModel
    {
        public int DefaultShiftID { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public int? OrganizationID { get; set; }
        public int? DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public int? ShiftID { get; set; }
    }
}
