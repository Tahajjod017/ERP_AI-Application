using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveSettings;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.LeaveManagements.LeaveSettings
{
    public class LeaveSettingsService : AppService<LeaveTypes>, ILeaveSettingsService
    {
        private readonly IGenericRepository<LeaveTypes> leaveType;
        private readonly IUserInfoService userInfoService;
        public LeaveSettingsService(IGenericRepository<LeaveTypes> leaveType, IUserInfoService userInfoService) : base(leaveType)
        {
            this.leaveType = leaveType;
            this.userInfoService = userInfoService;
        }

        public async Task<CommonReturnViewModel> SaveAddNewLeaveAsync(AddNewLeaveSave entityVM)
        {
       
            if (entityVM == null)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Data Can not be null"
                };
            }

            await leaveType.BeginTransactionAsync();

            try
            {
                var entity = new LeaveTypes
                {
                    OrganizationID = entityVM.OrganizationID,
                    IsApid = entityVM.IsApid,
                    LeaveTypeName = entityVM.LeaveTypeName,
                    LeaveDays = entityVM.LeaveDays,
                    Code=entityVM.Code,
                    CreatedAt = DateTime.Now,
                    CreatedBy = entityVM.CreatedBy,
                    LIP = entityVM.LIP,
                    LMAC = entityVM.LMAC
                };

                await leaveType.AddAsync(entity);
                await userInfoService.ActionLogAsync("Add New Leave", ActionName.DataAdd, null, entity, entity.LeaveTypeID, entityVM);
                await leaveType.CommitTransactionAsync();

                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Saved Successfully."

                };
            }
            catch (Exception ex)
            {

                await leaveType.RollbackTransactionAsync();
                Console.WriteLine(ex.Message);
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "An error occurred while saving the leave request."
                };
            }
        
    }
    }
}
