using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.EmployeeShiftView
{
    public class EmployeeShiftViewSetupVM : BaseViewModel
    {
        public int? OrganizationID { get; set; }

        public string? OrganizationName { get; set; }

        public List<int>? DepartmentIDs { get; set; }

        public string? DepartmentName { get; set; }


        public List<int>? EmployeeIDs { get; set; }

        public string? EmployeeName { get; set; }

        public int EmployeeFinalShiftID { get; set; }

        public int? DepartmentID { get; set; }

        public int? EmployeeID { get; set; }

        public int? ShiftID { get; set; }

        public DateTime? DayDate { get; set; }

        public int? StatusID { get; set; }

        public string? AssignedDates { get; set; }

        public string? WeekdayNumber { get; set; }

        public string? HolidayDates { get; set; }

        public string? HolidayTitle { get; set; }

        public string? LeaveDates { get; set; }

        public string? LeaveTypeName { get; set; }
    }
}
