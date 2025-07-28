using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ManualAttendence
{
    public class ManualAttendanceViewModel : BaseViewModel
    {
        public string EmployeeName { get; set; }
        public string AttendanceDate { get; set; }
        public string AssignShift { get; set; }
        public string ActualInTime { get; set; }
        public string ActualOutTime { get; set; }
        public string BreakInTime { get; set; }
        public bool ChkBox { get; set; }
    }

}
