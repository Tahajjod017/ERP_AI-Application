using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.AssignSpiralPattern;

namespace GCTL_App.ViewModels.AttendanceManagement.ScheduleManagement.AssignSpiralPattern
{
    public class AssignSpiralPatternPageVM : BaseViewModel
    {
        public AssignSpiralPatternSetupVM Create { get; set; } = new AssignSpiralPatternSetupVM();
        public AssignSpiralPatternEditVM Edit { get; set; } = new AssignSpiralPatternEditVM();
    }
}
