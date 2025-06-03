using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveSettings;

namespace GCTL.Service.AttendanceManagement.LeaveManagements.LeaveSettings
{
    public interface ILeaveSettingsService
    {
        Task<CommonReturnViewModel> SaveAddNewLeaveAsync(AddNewLeaveSave entityVM);
    }
}
