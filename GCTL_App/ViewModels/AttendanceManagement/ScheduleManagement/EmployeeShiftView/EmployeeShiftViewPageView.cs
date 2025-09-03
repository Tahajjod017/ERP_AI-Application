using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.EmployeeShiftView;

namespace GCTL_App.ViewModels.AttendanceManagement.ScheduleManagement.EmployeeShiftView
{
    public class EmployeeShiftViewPageView : BaseViewModel
    {
        public  EmployeeShiftViewSetupVM List { get; set; } = new EmployeeShiftViewSetupVM();
    }
}
