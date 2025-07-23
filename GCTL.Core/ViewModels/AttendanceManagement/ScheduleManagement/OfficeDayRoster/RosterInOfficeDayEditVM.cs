using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.OfficeDayRoster
{
    public class RosterInOfficeDayEditVM : BaseViewModel
    {
        public int? RosterInOfficeDayIdEdit { get; set; }
        public int? OrganizationIdEdit { get; set; }
        public int? DepartmentIdEdit { get; set; }
        public int? EmployeeIdEdit { get; set; }
        public int? ShiftIdEdit { get; set; }
        public DateTime? StartDateEdit { get; set; }
        public DateTime? EndDateEdit { get; set; }
        public DateTime? DayDateEdit { get; set; }
    }
}
