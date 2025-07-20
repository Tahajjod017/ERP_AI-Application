using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveSettings;
using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.MasterSetup.ActionTakens;

namespace GCTL.Service.AttendanceManagement.LeaveManagements.LeaveSettings
{
    public interface ILeaveSettingsService
    {
        Task<CommonReturnViewModel> SaveAddNewLeaveAsync(AddNewLeaveSave entityVM);
        Task<CommonReturnViewModel> UpdateLeaveAsynce(UpdateLeaveVM entityVM);
        Task<CommonReturnViewModel> UpdateLeaveIsActiveAsynce(LeaveTypeStatusUpdateIsActiveVM entityVM);
        Task<CommonReturnViewModel> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<GetLeaveTypesByIdVM> GetLeaveTypesByIdAsync(int leaveTypeID);
        Task<List<GetLeaveTypesListVM>> GetAllLeaveTypesAsync();

        // Leave Policy Configaration
        Task<CommonReturnViewModel> AddLeavepolicyAsync(AddLeavePolicyConfigarationVM entityVM);
        Task<CommonReturnViewModel> UpdateLeavepolicyAsync(AddLeavePolicyConfigarationVM entityVM);
        Task<List<GetDataLeavePolicyConfiguration>> GetDataLeavePolicy();
    }
}
