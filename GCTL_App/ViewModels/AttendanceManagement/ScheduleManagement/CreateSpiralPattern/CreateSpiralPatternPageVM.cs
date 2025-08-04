using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.CreateSpiralPattern;

namespace GCTL_App.ViewModels.AttendanceManagement.ScheduleManagement.CreateSpiralPattern
{
    public class CreateSpiralPatternPageVM : BaseViewModel
    {
        public CreateSpiralPatternVM Post { get; set; } = new CreateSpiralPatternVM();
    }
}
