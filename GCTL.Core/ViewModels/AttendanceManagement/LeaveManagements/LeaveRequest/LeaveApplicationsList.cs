using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest
{
    public class LeaveApplicationsList
    {

        public int LeaveApplicationID { get; set; }
        public string? EmployeeName { get; set; }
        public string? LeaveType { get; set; }
        public string? UnitYpe { get; set; }
        public string? StatusName { get; set; }
        public int? Period { get; set; }
        public string? UserType { get; set; }
        public string? ToDate { get; set; }
        public string? FromDate { get; set; }
        public bool IsFullDay { get; set; }
    }
}

