using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;

namespace GCTL_App.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest
{
    public class LeaveApplicationsRequestPageVM:BaseViewModel
    {
        public LeaveApplicationsRequestVM SetupForm = new LeaveApplicationsRequestVM();
        public LeaveApplicationEditVM SetupFormEdit = new LeaveApplicationEditVM();
    }
}
