using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest
{
    public class MultiDropDown
    {
       

        public int? OrganizationID { get; set; }

        public string? OrganizationName { get; set; }

        public int? DepartmentID { get; set; }

        public List<int>? DepartmentIDs { get; set; }

        public string? DepartmentName { get; set; }

        public int? EmployeeID { get; set; }

        public List<int>? EmployeeIDs { get; set; }

        public string? EmployeeName { get; set; }

    }
}
