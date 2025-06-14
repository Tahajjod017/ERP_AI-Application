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
        #region GetBy Data By ID 
        public async Task<GetLeaveTypesByIdVM> GetLeaveTypesByIdAsync(int leaveTypeID)
        {
            try
            {
                var data=await leaveType.GetByIdAsync(leaveTypeID);
                if (data == null) { return null; }
                GetLeaveTypesByIdVM dataVM = new GetLeaveTypesByIdVM
                {
                    LeaveTypeID = data.LeaveTypeID,

                    LeaveTypeName = string.IsNullOrWhiteSpace(data.LeaveTypeName) ? "" : data.LeaveTypeName,
                    OrganizationID = data.OrganizationID.HasValue ? data.OrganizationID : 0,
                    IsApid = data.IsPaid,
                    LeaveDays = data.LeaveDays.HasValue ? data.LeaveDays : 0,
                    Code = string.IsNullOrWhiteSpace(data.Code) ? "" : data.Code,
                    EffectiveFrom = data.EffectiveFrom.HasValue ? data.EffectiveFrom : 0,
                    EffectiveFromMonthYear = string.IsNullOrWhiteSpace(data.EffectiveFromMonthYear) ? "" : data.EffectiveFromMonthYear,
                    EffectiveAfter = string.IsNullOrWhiteSpace(data.EffectiveAfter) ? "" : data.EffectiveAfter,
                    MinimumDaysRequiredEncashement = data.MinimumDaysRequiredEncashement.HasValue ? data.MinimumDaysRequiredEncashement : 0,
                    MaximumDaysAllowedEncashement = data.MaximumDaysAllowedEncashement.HasValue ? data.MaximumDaysAllowedEncashement : 0
                };

                return dataVM;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<List<GetLeaveTypesListVM>> GetAllLeaveTypesAsync()
        {
            var dataList = await leaveType.GetAllAsync(); // Assuming you have a repository method for all
            if (dataList == null || !dataList.Any())
                return new List<GetLeaveTypesListVM>();

            return dataList.Select(data => new GetLeaveTypesListVM
            {
                LeaveTypeID = data.LeaveTypeID,
                LeaveTypeName = string.IsNullOrWhiteSpace(data.LeaveTypeName) ? "" : data.LeaveTypeName,
                IsActive=data.IsActive
            }).ToList();
        }

        #endregion


        #region Save Data
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
                    IsPaid = entityVM.IsApid,
                    LeaveTypeName = entityVM.LeaveTypeName,
                    LeaveDays = entityVM.LeaveDays,
                    Code = entityVM.Code,
                    CreatedAt = DateTime.Now,
                    CreatedBy = entityVM.CreatedBy,
                    LIP = entityVM.LIP,
                    LMAC = entityVM.LMAC,
                    EffectiveFromMonthYear = entityVM.EffectiveFromMonthYear,
                    EffectiveFrom = entityVM.EffectiveFrom,
                    EffectiveAfter = entityVM.EffectiveAfter
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
        #endregion

    }
}
