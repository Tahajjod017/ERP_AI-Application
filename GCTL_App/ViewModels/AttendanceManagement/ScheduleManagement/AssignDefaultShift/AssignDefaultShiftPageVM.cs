using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.AssignDefaultShift;

namespace GCTL_App.ViewModels.AttendanceManagement.ScheduleManagement.AssignDefaultShift
{
    public class AssignDefaultShiftPageVM : BaseViewModel
    {
        public AssignDefaultShiftSetupVM Setup { get; set; } = new AssignDefaultShiftSetupVM();
        public AssignDefaultShiftSetupVM Update { get; set; } = new AssignDefaultShiftSetupVM();
        public List<AssignDefaultShiftSetupVM> List = new List<AssignDefaultShiftSetupVM>();
    }
}
