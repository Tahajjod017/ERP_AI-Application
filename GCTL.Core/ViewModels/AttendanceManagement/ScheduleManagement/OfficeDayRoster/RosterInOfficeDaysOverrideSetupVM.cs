using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.OfficeDayRoster
{
    public class RosterInOfficeDaysOverrideSetupVM : BaseViewModel
    {
        public int RosterInOfficeDaysOverrideID { get; set; }

        public int? RosterInOfficeDayID { get; set; }

        public DateTime? OverrideDate { get; set; }

        public int? ShiftID { get; set; }

        public virtual RosterInOfficeDaysSetupVM? RosterInOfficeDaysSetupVM { get; set; }
    }
}
