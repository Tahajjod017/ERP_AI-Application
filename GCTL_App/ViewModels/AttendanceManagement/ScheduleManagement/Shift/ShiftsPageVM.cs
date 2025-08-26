using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.Shift;

namespace GCTL_App.ViewModels.AttendanceManagement.ScheduleManagement.Shift
{
    public class ShiftsPageVM : BaseViewModel
    {
        public ShiftsSetupVM Setup { get; set; } = new ShiftsSetupVM();
        public ShiftUpdateSetupVM Update { get; set; } = new ShiftUpdateSetupVM();
        public List<ShiftsSetupVM> List { get; set; } = new List<ShiftsSetupVM>();
    }
}
