using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.OfficeDayRoster
{
    public class RosterInOfficeDaysListVM : BaseViewModel
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

        public string? AssignedDates { get; set; }

        public string? WeekdayNumber { get; set; }

        public string? WeekendDays { get; set; }

        public string? HolidayTitle { get; set; }

        public string? HolidayDates { get; set; }
    }
}
