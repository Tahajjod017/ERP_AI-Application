using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveSettings;
using GCTL.Core.ViewModels.MasterSetup.ActionTakens;
using GCTL.Core.ViewModels.MasterSetup.BloodGroup;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using Newtonsoft.Json;
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
        private readonly IGenericRepository<LeavePolicyConfiguration> leavepolicy;
        private readonly IUserInfoService userInfoService;
        public LeaveSettingsService(IGenericRepository<LeaveTypes> leaveType, IUserInfoService userInfoService, IGenericRepository<LeavePolicyConfiguration> leavepolicy ) : base(leaveType)
        {
            this.leaveType = leaveType;
            this.userInfoService = userInfoService;
            this.leavepolicy = leavepolicy;
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
                    IsPaid = data.IsPaid,
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

            return dataList.Where(x=>x.DeletedAt==null).Select(data => new GetLeaveTypesListVM
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
                    IsPaid = entityVM.IsPaid,
                    IsActive=entityVM.IsActive,
                    LeaveTypeName = entityVM.LeaveTypeName,
                    LeaveDays = entityVM.LeaveDays,
                    Code = entityVM.Code,
                    CreatedAt = DateTime.Now,
                    CreatedBy = entityVM.CreatedBy,
                    LIP = entityVM.LIP,
                    LMAC = entityVM.LMAC,
                    EffectiveFromMonthYear = entityVM.EffectiveFromMonthYear,
                    EffectiveFrom = entityVM.EffectiveFrom,
                    EffectiveAfter = entityVM.EffectiveAfter,
                    ApplicableYear=DateTime.Now.Year,
                    
                };

                await leaveType.AddAsync(entity);
                await userInfoService.ActionLogAsync("Leave Settings", ActionName.DataAdd, null, entity, entity.LeaveTypeID, entityVM);
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

        #region Update Leave 
        public async Task<CommonReturnViewModel> UpdateLeaveAsynce(UpdateLeaveVM entityVM)
        {
            await leaveType.BeginTransactionAsync();
            try
            {
                // Fetch existing leave type from DB
                var existingLeave = await leaveType.GetByIdAsync(entityVM.LeaveTypeID);

                if (existingLeave == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Leave type not found."
                    };
                }
                var beforeEntity = JsonConvert.DeserializeObject<UpdateLeaveVM>(JsonConvert.SerializeObject(existingLeave, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));

                // Map values from view model to entity
                existingLeave.LeaveTypeName = entityVM.LeaveTypeName;
                existingLeave.OrganizationID = entityVM.OrganizationID;
                existingLeave.IsPaid = entityVM.IsPaid;
               // existingLeave.IsActive = entityVM.IsActive;
                existingLeave.LeaveDays = entityVM.LeaveDays;
                existingLeave.Code = entityVM.Code;
                existingLeave.EffectiveFrom = entityVM.EffectiveFrom;
                existingLeave.EffectiveFromMonthYear = entityVM.EffectiveFromMonthYear;
                existingLeave.EffectiveAfter = entityVM.EffectiveAfter;
                existingLeave.MinimumDaysRequiredEncashement = entityVM.MinimumDaysRequiredEncashement;
                existingLeave.MaximumDaysAllowedEncashement = entityVM.MaximumDaysAllowedEncashement;
                existingLeave.ApplicableYear = DateTime.Now.Year;
                existingLeave.LIP = entityVM.LIP;
                existingLeave.LMAC = entityVM.LMAC;
                existingLeave.UpdatedAt = DateTime.Now;
                existingLeave.UpdatedBy = entityVM.UpdatedBy;
                // Save changes
                await leaveType.UpdateAsync(existingLeave);
                var afterEntity = JsonConvert.DeserializeObject<UpdateLeaveVM>(JsonConvert.SerializeObject(existingLeave, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
                await userInfoService.ActionLogAsync("Leave Settings", ActionName.DataUpdated, beforeEntity, afterEntity, existingLeave.LeaveTypeID, entityVM);
                await leaveType.CommitTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Updated Successfully."
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "An error occurred while updating leave type."
                };
            }
        }


        #endregion
        #region SoftDeleteAsync
        public async Task<CommonReturnViewModel> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await leaveType.BeginTransactionAsync();
            try
            {
                var data = await leaveType.FindAsync(x => requestVM.Ids.Contains(x.LeaveTypeID));
                if (data == null || data.Count == 0)
                {
                    return new CommonReturnViewModel
                    {
                        Message = "No data found to delete."
                    };
                }
               
                var beforeEntity = JsonConvert.DeserializeObject<List<AddNewLeaveSave>>(
             JsonConvert.SerializeObject(data, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
                var targetIds = data.Select(x => (int?)x.LeaveTypeID).ToList();
                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                    item.DeletedBy = requestVM.DeletedBy ?? null;
                }

                await leaveType.UpdateRangeAsync(data);
                await userInfoService.ActionLogDeleteAsync("Leave Settigs", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);
                await leaveType.CommitTransactionAsync();

                return new CommonReturnViewModel
                {
                    Message = $"Deleted Successfully."
                };
            }
            catch (Exception ex)
            {
                await leaveType.RollbackTransactionAsync();
                throw new Exception("Error occurred during the deletion of data.", ex);
            }
        }

        
        #endregion

        #region Leave Policy Configuration 
        public async Task<CommonReturnViewModel> AddLeavepolicyAsync(AddLeavePolicyConfigarationVM entityVM)
        {
            
            if(entityVM == null)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Data Can not be null"
                };
            }
           
            await leavepolicy.BeginTransactionAsync();
            try
            {
                var entity = new LeavePolicyConfiguration
                {
                    IsWeekendCountedAsLeave = entityVM.IsWeekendCountedAsLeave,
                    IsHolidayCountedAsLeave = entityVM.IsHolidayCountedAsLeave,
                    IsExceedLeaveBalance = entityVM.IsExceedLeaveBalance,
                    RoundOffHour = entityVM.RoundOffHour,
                    IsAllowRequestForPastDates = entityVM.IsAllowRequestForPastDates,
                    AllowRequestForFutureDays = entityVM.AllowRequestForFutureDays,
                    MaximumleaveDaysPerAplication = entityVM.MaximumleaveDaysPerAplication,
                     MaximumGapDaysBetweenAplications = entityVM.MaximumGapDaysBetweenAplications,       
                     IsMaximumGapDaysBetweenAplications=entityVM.IsMaximumGapDaysBetweenAplications,
                     IsMaximumleaveDaysPerAplication=entityVM.IsMaximumleaveDaysPerAplication,
                     IsAllowRequestForFutureDays=entityVM.IsAllowRequestForFutureDays,
                     IsRoundOffHour=entityVM.IsRoundOffHour,
                     LeaveBalanceResetDate=entityVM.LeaveBalanceResetDate,
                     EnableLeaveBalanceResetDate=entityVM.EnableLeaveBalanceResetDate,
                     IsAllowCrossLeave=entityVM.IsAllowCrossLeave,
                     WorkingHour=entityVM.WorkingHour,
                     ShortLeaveMaxInADay=entityVM.ShortLeaveMaxInADay,
                     LIP = entityVM.LIP,
                    LMAC = entityVM.LMAC,
                    CreatedBy = entityVM.CreatedBy,
                    CreatedAt = DateTime.Now
                };
                   await leavepolicy.AddAsync(entity);
                   await userInfoService.ActionLogAsync("Add Leave Policy", ActionName.DataAdd, null, entity, entity.LeavePolicyConfigurationID, entityVM);
                  await leaveType.CommitTransactionAsync();
                return new CommonReturnViewModel
               {
                   Success = true,
                    Message = "Saved Successfully."

                };
            }
            catch (Exception ex)
            {
               
                await leavepolicy.RollbackTransactionAsync();
                Console.WriteLine(ex.Message);
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "An error occurred while saving the leave policy."
                };
            }


           
        }

        public async Task<CommonReturnViewModel> UpdateLeavepolicyAsync(AddLeavePolicyConfigarationVM entityVM)
        {
            var response = new CommonReturnViewModel();

            try
            {
                await leavepolicy.BeginTransactionAsync();

                var existingPolicy = await leavepolicy.GetByIdAsync(entityVM.LeavePolicyConfigurationID);

                if (existingPolicy == null)
                {
                    response.Success = false;
                    response.Message = "Leave policy not found.";
                    await leavepolicy.RollbackTransactionAsync();
                    return response;
                }
                var beforeEntity = JsonConvert.DeserializeObject<AddLeavePolicyConfigarationVM>(JsonConvert.SerializeObject(existingPolicy, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
                // Update fields
                existingPolicy.IsWeekendCountedAsLeave = entityVM.IsWeekendCountedAsLeave;
                existingPolicy.IsHolidayCountedAsLeave = entityVM.IsHolidayCountedAsLeave;
                existingPolicy.IsExceedLeaveBalance = entityVM.IsExceedLeaveBalance;
                existingPolicy.RoundOffHour = string.IsNullOrWhiteSpace(entityVM.RoundOffHour) ? "" : entityVM.RoundOffHour;
                existingPolicy.IsRoundOffHour = entityVM.IsRoundOffHour;
                existingPolicy.IsAllowRequestForPastDates = entityVM.IsAllowRequestForPastDates;
                existingPolicy.AllowRequestForFutureDays = entityVM.AllowRequestForFutureDays;
                existingPolicy.MaximumleaveDaysPerAplication = entityVM.MaximumleaveDaysPerAplication;
                existingPolicy.MaximumGapDaysBetweenAplications = entityVM.MaximumGapDaysBetweenAplications;
                existingPolicy.IsAllowRequestForFutureDays = entityVM.IsAllowRequestForFutureDays;
                existingPolicy.IsMaximumleaveDaysPerAplication = entityVM.IsMaximumleaveDaysPerAplication;
                existingPolicy.IsMaximumGapDaysBetweenAplications = entityVM.IsMaximumGapDaysBetweenAplications;
                existingPolicy.LeaveBalanceResetDate = entityVM.LeaveBalanceResetDate;
                existingPolicy.EnableLeaveBalanceResetDate = entityVM.EnableLeaveBalanceResetDate;
                existingPolicy.UpdatedAt = DateTime.Now;
                existingPolicy.UpdatedBy = entityVM.UpdatedBy;
                existingPolicy.IsAllowCrossLeave = entityVM.IsAllowCrossLeave;
                existingPolicy.WorkingHour = entityVM.WorkingHour;
                existingPolicy.ShortLeaveMaxInADay = entityVM.ShortLeaveMaxInADay;
                existingPolicy.LIP = entityVM.LIP;
                existingPolicy.LMAC = entityVM.LMAC;
                await leavepolicy.UpdateAsync(existingPolicy);
                var afterEntity = JsonConvert.DeserializeObject<AddLeavePolicyConfigarationVM>(JsonConvert.SerializeObject(existingPolicy, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
                await userInfoService.ActionLogAsync("Add Leave Policy", ActionName.DataUpdated, beforeEntity, afterEntity, existingPolicy.LeavePolicyConfigurationID, entityVM);
                await leavepolicy.CommitTransactionAsync();

                response.Success = true;
                response.Message = "Updated Successfully.";
            }
            catch (Exception ex)
            {
                await leavepolicy.RollbackTransactionAsync();
                response.Success = false;
                response.Message = $"Error: {ex.Message}";
            }

            return response;
        }

        #endregion
        #region  Get Data 
        public async Task<List<GetDataLeavePolicyConfiguration>> GetDataLeavePolicy()
        {
            await leavepolicy.BeginTransactionAsync();

            try
            {
                var data = await leavepolicy.GetAllAsync();

                if (data == null || !data.Any())
                {
                    return new List<GetDataLeavePolicyConfiguration>();
                }

                var result = data.Select(x => new GetDataLeavePolicyConfiguration
                {
                    LeavePolicyConfigurationID = x.LeavePolicyConfigurationID,
                    IsWeekendCountedAsLeave = x.IsWeekendCountedAsLeave,
                    IsHolidayCountedAsLeave = x.IsHolidayCountedAsLeave,
                    IsExceedLeaveBalance = x.IsExceedLeaveBalance,
                    RoundOffHour = string.IsNullOrWhiteSpace(x.RoundOffHour) ? "" : x.RoundOffHour,
                    IsAllowRequestForPastDates = x.IsAllowRequestForPastDates,
                    IsRoundOffHour = x.IsRoundOffHour,
                    AllowRequestForFutureDays = x.AllowRequestForFutureDays,
                    MaximumleaveDaysPerAplication = x.MaximumleaveDaysPerAplication,
                    MaximumGapDaysBetweenAplications = x.MaximumGapDaysBetweenAplications,
                    IsAllowRequestForFutureDays = x.IsAllowRequestForFutureDays,
                    IsMaximumleaveDaysPerAplication = x.IsMaximumleaveDaysPerAplication,
                    IsMaximumGapDaysBetweenAplications = x.IsMaximumGapDaysBetweenAplications,
                    EnableLeaveBalanceResetDate=x.EnableLeaveBalanceResetDate,
                    LeaveBalanceResetDate=x.LeaveBalanceResetDate,
                    IsAllowCrossLeave=x.IsAllowCrossLeave,
                    WorkingHour=x.WorkingHour,
                    ShortLeaveMaxInADay=x.ShortLeaveMaxInADay,
                    
                }).ToList();

                await leavepolicy.CommitTransactionAsync();
                return result;
            }
            catch (Exception ex)
            {
                await leavepolicy.RollbackTransactionAsync();
                Console.WriteLine(ex.Message);
                throw;
            }
        }

       


        #endregion
    }
}
