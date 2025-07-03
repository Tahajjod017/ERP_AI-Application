using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.OfficeDayRoster
{
    public class RosterInOfficeDaysSetupVM : BaseViewModel
    {
        public int RosterInOfficeDayID { get; set; }

        public int? OrganizationID { get; set; }
        public string? OrganizationName { get; set; }

        public int? DepartmentID { get; set; }
        public List<int>? DepartmentIDs { get; set; }
        public string? DepartmentName { get; set; }

        public int? EmployeeID { get; set; }
        public List<int>? EmployeeIDs { get; set; }
        public string? EmployeeName { get; set; }

        public int? ShiftID { get; set; }
        public string? ShiftName { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? TimeRange { get; set; }

        public List<int>? ExcludedEmployeeIDs { get; set; }
    }
}
