using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.Shift;
using GCTL.Core.ViewModels.MasterSetup.ActionTakens;

namespace GCTL_App.ViewModels.AttendanceManagement.ScheduleManagement.Shift
{
    public class ShiftsPageVM : BaseViewModel
    {
        public ShiftsSetupVM Setup { get; set; } = new ShiftsSetupVM();
        public List<ShiftsSetupVM> List { get; set; } = new List<ShiftsSetupVM>();
    }
}
