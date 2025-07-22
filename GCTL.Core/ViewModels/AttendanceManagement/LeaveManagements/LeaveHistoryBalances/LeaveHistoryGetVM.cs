using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveHistoryBalances
{
    public class LeaveHistoryGetVM
    {
        public string? EmployeeName { get; set; }
        public string? EmployeeImage { get; set; }
        public string? LeaveType { get; set; }
        public string? EmployeeDepartment { get; set; }
        public string? ToDate { get; set; }
        public string? FromDate { get; set; }
        public double? Period { get; set; }
        public int LeaveApplicationID { get; set; }
        public bool IsFullDay { get; set; }
       
    }
}
