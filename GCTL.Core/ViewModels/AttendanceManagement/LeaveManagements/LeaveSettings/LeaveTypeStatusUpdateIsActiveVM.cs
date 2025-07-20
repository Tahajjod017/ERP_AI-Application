using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveSettings
{
    public class LeaveTypeStatusUpdateIsActiveVM:BaseViewModel
    {
        public int LeaveTypeID { get; set; }
        public bool IsActive { get; set; }
    }
}
