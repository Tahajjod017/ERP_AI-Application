using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.OfficeDayRoster;

namespace GCTL_App.ViewModels.AttendanceManagement.ScheduleManagement.OfficeDayRoster
{
    public class RosterInOfficeDaysPageVM : BaseViewModel
    {
        public RosterInOfficeDaysSetupVM Setup { get; set; } = new RosterInOfficeDaysSetupVM();
        public RosterInOfficeDayEditVM Edit { get; set; } = new RosterInOfficeDayEditVM();
        public RosterInOfficeDayModalAddVM ModalAdd { get; set; } = new RosterInOfficeDayModalAddVM();
        public List<RosterInOfficeDaysSetupVM> RosterList { get; set; } = new List<RosterInOfficeDaysSetupVM>();
    }
}
