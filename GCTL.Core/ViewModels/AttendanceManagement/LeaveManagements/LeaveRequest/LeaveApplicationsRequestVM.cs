using GCTL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest
{
    public class LeaveApplicationsRequestVM:BaseViewModel
    {
        public int LeaveApplicationID { get; set; }

        public int? EmployeeID { get; set; }

        public bool IsFullDay { get; set; }

        public DateOnly FromDate { get; set; }

        public DateOnly ToDate { get; set; }

        public TimeOnly? PartialFromTime { get; set; }

        public TimeOnly? PartialToTime { get; set; }

        public int? StatusID { get; set; }

        public int? LeaveTypeID { get; set; }

   
    }
}
