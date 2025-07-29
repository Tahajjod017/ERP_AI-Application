using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.OffDayRoster
{
    public class RosterInOffDayListVM : BaseViewModel
    {
        public int RosterInOffDayID { get; set; }

        public int? OrganizationID { get; set; }
        public string? OrganizationName { get; set; }

        public int? BranchID { get; set; }
        public List<int>? BranchIDs { get; set; }
        public string? BranchName { get; set; }

        public int? DepartmentID { get; set; }
        public List<int>? DepartmentIDs { get; set; }
        public string? DepartmentName { get; set; }

        public int? EmployeeID { get; set; }
        public List<int>? EmployeeIDs { get; set; }
        public string? EmployeeName { get; set; }

        public int? ShiftID { get; set; }
        public string? ShiftName { get; set; }

        public DateTime? DayDate { get; set; }

        public int? CompensationTypeID { get; set; }

        public string? TimeRange { get; set; }

        public Dictionary<DateTime, ShiftVM> ShiftsPerDay { get; set; } = new();
    }

    public class ShiftVM
    {
        public string ShiftName { get; set; }
        public string TimeRange { get; set; } 
    }
}
