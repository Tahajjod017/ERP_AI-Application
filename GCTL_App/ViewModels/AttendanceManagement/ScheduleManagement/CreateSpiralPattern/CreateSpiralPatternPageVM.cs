using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.CreateSpiralPattern;

namespace GCTL_App.ViewModels.AttendanceManagement.ScheduleManagement.CreateSpiralPattern
{
    public class CreateSpiralPatternPageVM : BaseViewModel
    {
        public CreateSpiralPatternVM Create { get; set; } = new CreateSpiralPatternVM();
        public AddSpiralWeeklyPatternVM AddWeekly { get; set; } = new AddSpiralWeeklyPatternVM();
        public AddSpiralFortMonthlyPatternVM AddSpiralFortMonthly { get; set; } = new AddSpiralFortMonthlyPatternVM();
        public UpdateSpiralPatternVM Edit { get; set; } = new UpdateSpiralPatternVM();
    }
}
