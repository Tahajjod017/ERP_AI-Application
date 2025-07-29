using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.OffDayRoster;

namespace GCTL_App.ViewModels.AttendanceManagement.ScheduleManagement.OffDayRoster
{
    public class RosterInOffDayPageVM : BaseViewModel
    {
        public RosterInOffDaySetupVM Setup { get; set; } = new RosterInOffDaySetupVM();
        public RosterInOffDayEditVM Edit { get; set; } = new RosterInOffDayEditVM();
        public RosterInOffDaySetupVM List { get; set; } = new RosterInOffDaySetupVM();
    }
}
