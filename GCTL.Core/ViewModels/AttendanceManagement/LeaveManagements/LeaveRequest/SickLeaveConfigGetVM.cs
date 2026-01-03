using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest
{
    public class SickLeaveConfigGetVM
    {
        public bool IsSickLeaveDocumentRequired { get; set; }

        public int? SickLeaveDocumentWithinDays { get; set; }

        public string ? Code { get; set; }
    }
}
