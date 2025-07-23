using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.OfficeDayRoster
{
    public class RosterInOfficeDayModalAddVM : BaseViewModel
    {
        public int? RosterInOfficeDayIdAdd { get; set; }
        public int? OrganizationIdAdd { get; set; }
        public int? DepartmentIdAdd { get; set; }
        public int? EmployeeIdAdd { get; set; }
        public int? ShiftIdAdd { get; set; }
        public DateTime? StartDateAdd { get; set; }
        public DateTime? EndDateAdd { get; set; }
        public DateTime? DayDateAdd { get; set; }
    }
}
