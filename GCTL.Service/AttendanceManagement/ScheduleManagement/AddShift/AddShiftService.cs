using GCTL.Core;
using GCTL.Core.Helpers;
using GCTL.Core.Helpers.Jsonserialize;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.Shift;
using GCTL.Core.ViewModels.MasterSetup.Genders;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.AdminSettings.GeneralSettings;
using GCTL.Service.Pagination;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;
using static GCTL.Service.AdminSettings.GeneralSettings.UtcTimeHelper;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GCTL.Service.AttendanceManagement.ScheduleManagement.AddShift
{
    public class AddShiftService : AppService<Shifts>, IAddShiftService
    {
        #region Repositories
        private readonly IGenericRepository<Shifts> _genericRepository;
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IDbConnection _dbConnection;
        private readonly ILocalizationContext _localizationContext;


        public AddShiftService(IGenericRepository<Shifts> genericRepository, IUserInfoService userInfoService, IGenericRepository<Organization> organizationRepository, IDbConnection dbConnection, ILocalizationContext localizationContext) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _userInfoService = userInfoService;
            _organizationRepository = organizationRepository;
            _dbConnection = dbConnection;
            _localizationContext = localizationContext;
        }
        #endregion


        #region AddAsync

        #region By EFCore.BulkExtensions
        //public async Task<bool> AddAsync(ShiftsSetupVM model)
        //{
        //    await _genericRepository.BeginTransactionAsync();
        //    try
        //    {
        //        var now = DateTime.Now;

        //        // Fetch all soft-deleted entries that match the shift name + organization IDs
        //        var existingEntities = await _genericRepository.FindAsync(b =>
        //            b.DeletedAt != null &&
        //            b.ShiftName == model.ShiftName &&
        //            model.OrganizationIDs.Contains((int)b.OrganizationID)
        //        );

        //        // Separate insert and update lists
        //        var shiftsToUpdate = new List<Shifts>();
        //        var shiftsToInsert = new List<Shifts>();

        //        foreach (var orgId in model.OrganizationIDs)
        //        {
        //            var existing = existingEntities.FirstOrDefault(x => x.OrganizationID == orgId);

        //            if (existing != null)
        //            {
        //                existing.StartTime = model.StartTime;
        //                existing.EndTime = model.EndTime;
        //                existing.IsLateCount = model.IsLateCount;
        //                existing.IsAutomaticORManualBreakTime = model.IsAutomaticORManualBreakTime;
        //                existing.IsMealBreakCompulsaryOrComplementaryDeductWithShift = model.IsMealBreakCompulsaryOrComplementaryDeductWithShift;
        //                existing.IsAllowStartAndEndTime = model.IsAllowStartAndEndTime;
        //                existing.MealBreakStartTime = model.MealBreakStartTime;
        //                existing.MealBreakEndTime = model.MealBreakEndTime;
        //                existing.IsAllowOvertime = model.IsAllowOvertime;
        //                existing.GraceTime = model.GraceTime;
        //                existing.MinimumWorkingTime = model.MinimumWorkingTime;
        //                existing.MinimumRequiredOvertime = model.MinimumRequiredOvertime;
        //                existing.MaximumAllowedOvertime = model.MaximumAllowedOvertime;
        //                existing.MealBreakTime = model.MealBreakTime;

        //                existing.CreatedAt = now;
        //                existing.CreatedBy = model.CreatedBy;
        //                existing.UpdatedBy = model.UpdatedBy ?? null;
        //                existing.UpdatedAt = now;
        //                existing.DeletedAt = null;
        //                existing.DeletedBy = null;
        //                existing.LIP = model.LIP;
        //                existing.LMAC = model.LMAC;

        //                shiftsToUpdate.Add(existing);
        //            }
        //            else
        //            {
        //                var newShift = new Shifts
        //                {
        //                    ShiftName = model.ShiftName,
        //                    OrganizationID = orgId,
        //                    StartTime = model.StartTime,
        //                    EndTime = model.EndTime,
        //                    IsLateCount = model.IsLateCount,
        //                    IsAutomaticORManualBreakTime = model.IsAutomaticORManualBreakTime,
        //                    IsMealBreakCompulsaryOrComplementaryDeductWithShift = model.IsMealBreakCompulsaryOrComplementaryDeductWithShift,
        //                    IsAllowStartAndEndTime = model.IsAllowStartAndEndTime,
        //                    MealBreakStartTime = model.MealBreakStartTime,
        //                    MealBreakEndTime = model.MealBreakEndTime,
        //                    IsAllowOvertime = model.IsAllowOvertime,
        //                    GraceTime = model.GraceTime,
        //                    MinimumWorkingTime = model.MinimumWorkingTime,
        //                    MinimumRequiredOvertime = model.MinimumRequiredOvertime,
        //                    MaximumAllowedOvertime = model.MaximumAllowedOvertime,
        //                    MealBreakTime = model.MealBreakTime,
        //                    CreatedAt = now,
        //                    CreatedBy = model.CreatedBy,
        //                    LIP = model.LIP,
        //                    LMAC = model.LMAC
        //                };

        //                shiftsToInsert.Add(newShift);
        //            }
        //        }

        //        // ✅ Perform bulk update
        //        if (shiftsToUpdate.Any())
        //        {
        //            await _genericRepository.BulkUpdateAsync(shiftsToUpdate);
        //        }

        //        // ✅ Perform bulk insert
        //        if (shiftsToInsert.Any())
        //        {
        //            await _genericRepository.BulkInsertAsync(shiftsToInsert);

        //            // Optional: Log insert actions
        //            foreach (var entity in shiftsToInsert)
        //            {
        //                await _userInfoService.ActionLogAsync("Add Shift", ActionName.DataAdd, null, entity, entity.ShiftID, model);
        //            }
        //        }

        //        await _genericRepository.CommitTransactionAsync();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        await _genericRepository.RollbackTransactionAsync();
        //        throw new Exception("Bulk operation failed: " + ex.Message, ex);
        //    }
        //}
        #endregion


        #region Normal
        public async Task<bool> AddAsync(ShiftsSetupVM model)
        {
            await _genericRepository.BeginTransactionAsync();

            var userTimeZone = _localizationContext.Zone;  // Assuming _localizationContext is your ILocalizationContext

            var utcStartTime = TimeConversionHelper.ConvertTimeOnlyToUtc(model.StartTime.Value, _localizationContext);
            var utcEndTime = TimeConversionHelper.ConvertTimeOnlyToUtc(model.EndTime.Value, _localizationContext);

            int? inPunchCountFromMin = (model.EarlyInTimeHour * 60 ?? 0) + model.EarlyInTimeMinute ?? 0;
            int? outPunchCountToMin = (model.EarlyOutTimeHour * 60 ?? 0) + model.EarlyOutTimeMinute ?? 0;
            int? graceTime = (model.GraceTimeHour * 60 ?? 0) + model.GraceTimeMinute ?? 0;
            int? minimumWorkingTime = (model.MinimumWorkingTimeHour * 60 ?? 0) + model.MinimumWorkingTimeMinute ?? 0;
            int? minimumRequiredOvertime = (model.MinimumRequiredOvertimeHour * 60 ?? 0) + model.MinimumRequiredOvertimeMinute ?? 0;
            int? maximumAllowedOvertime = (model.MaximumAllowedOvertimeHour * 60 ?? 0) + model.MaximumAllowedOvertimeMinute ?? 0;
            int? mealBreakTime = (model.MealBreakTimeHour * 60 ?? 0) + model.MealBreakTimeMinute ?? 0;

            try
            {
                foreach (var organizationID in model.OrganizationIDs)
                {
                    var entityToRestore = await _genericRepository.FindAsync(b => b.DeletedAt != null && b.ShiftName == model.ShiftName && b.OrganizationID == organizationID);

                    var existingEntity = entityToRestore.FirstOrDefault();
                    if (existingEntity != null)
                    {
                        existingEntity.ShiftName = model.ShiftName;
                        existingEntity.OrganizationID = organizationID;
                        existingEntity.StartTime = utcStartTime;
                        existingEntity.EndTime = utcEndTime;
                        existingEntity.IsLateCount = model.IsLateCount;
                        existingEntity.IsAutomaticORManualBreakTime = model.IsAutomaticORManualBreakTime;
                        existingEntity.IsMealBreakCompulsaryOrComplementaryDeductWithShift = model.IsMealBreakCompulsaryOrComplementaryDeductWithShift;
                        existingEntity.IsAllowStartAndEndTime = model.IsAllowStartAndEndTime;
                        existingEntity.MealBreakStartTime = model.MealBreakStartTime;
                        existingEntity.MealBreakEndTime = model.MealBreakEndTime;
                        existingEntity.IsAllowOvertime = model.IsAllowOvertime;
                        existingEntity.GraceTime = graceTime;
                        existingEntity.MinimumWorkingTime = minimumWorkingTime;
                        existingEntity.MinimumRequiredOvertime = minimumRequiredOvertime;
                        existingEntity.MaximumAllowedOvertime = maximumAllowedOvertime;
                        existingEntity.MealBreakTime = mealBreakTime;
                        existingEntity.IsRestrictFlexibleInTime = model.IsRestrictFlexibleInTime;
                        existingEntity.InPunchCountFromMin = inPunchCountFromMin;
                        existingEntity.IsRestrictFlexibleOutTime = model.IsRestrictFlexibleOutTime;
                        existingEntity.OutPunchCountToMin = outPunchCountToMin;

                        existingEntity.CreatedAt = DateTime.Now;
                        existingEntity.CreatedBy = model.CreatedBy;
                        existingEntity.LIP = model.LIP;
                        existingEntity.LMAC = model.LMAC;
                        existingEntity.UpdatedBy = model.UpdatedBy ?? null;
                        existingEntity.DeletedAt = null;
                        existingEntity.UpdatedAt = DateTime.Now;

                        await _genericRepository.UpdateAsync(existingEntity);
                        var afterEntity = JsonConvert.DeserializeObject<ShiftsSetupVM>(JsonConvert.SerializeObject(entityToRestore, JsonSettings.IgnoreReferenceLoop));
                        await _userInfoService.ActionLogAsync("Add Shift", ActionName.DataAdd, null, existingEntity, existingEntity.ShiftID, model);
                    }
                    else
                    {
                        Shifts entity = new Shifts();
                        entity.ShiftName = model.ShiftName;
                        entity.OrganizationID = organizationID;
                        entity.StartTime = utcStartTime;
                        entity.EndTime = utcEndTime;
                        entity.IsLateCount = model.IsLateCount;
                        entity.IsAutomaticORManualBreakTime = model.IsAutomaticORManualBreakTime;
                        entity.IsMealBreakCompulsaryOrComplementaryDeductWithShift = model.IsMealBreakCompulsaryOrComplementaryDeductWithShift;
                        entity.IsAllowStartAndEndTime = model.IsAllowStartAndEndTime;
                        entity.MealBreakStartTime = model.MealBreakStartTime;
                        entity.MealBreakEndTime = model.MealBreakEndTime;
                        entity.IsAllowOvertime = model.IsAllowOvertime;
                        entity.GraceTime = graceTime;
                        entity.MinimumWorkingTime = minimumWorkingTime;
                        entity.MinimumRequiredOvertime = minimumRequiredOvertime;
                        entity.MaximumAllowedOvertime = maximumAllowedOvertime;
                        entity.MealBreakTime = mealBreakTime;
                        entity.IsRestrictFlexibleInTime = model.IsRestrictFlexibleInTime;
                        entity.InPunchCountFromMin = inPunchCountFromMin;
                        entity.IsRestrictFlexibleOutTime = model.IsRestrictFlexibleOutTime;
                        entity.OutPunchCountToMin = outPunchCountToMin;

                        entity.CreatedAt = DateTime.Now;
                        entity.CreatedBy = model.CreatedBy ?? null;
                        entity.LIP = model.LIP;
                        entity.LMAC = model.LMAC;
                        await _genericRepository.AddAsync(entity);
                        await _userInfoService.ActionLogAsync("Add Shift", ActionName.DataAdd, null, entity, entity.ShiftID, model);
                    }
                }

                await _genericRepository.CommitTransactionAsync();

                return true;
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                throw new Exception(ex.Message, ex);
                //return false;
            }
        }
        #endregion

        #endregion


        #region Update
        public async Task<bool> UpdateAsync(ShiftUpdateSetupVM model)
        {
            await _genericRepository.BeginTransactionAsync();

            var userTimeZone = _localizationContext.Zone;  // Assuming _localizationContext is your ILocalizationContext

            var utcStartTime = TimeConversionHelper.ConvertTimeOnlyToUtc(model.UpdateStartTime.Value, _localizationContext);
            var utcEndTime = TimeConversionHelper.ConvertTimeOnlyToUtc(model.UpdateEndTime.Value, _localizationContext);

            int? inPunchCountFromMin = (model.UpdateEarlyInTimeHour * 60 ?? 0) + model.UpdateEarlyInTimeMinute ?? 0;
            int? outPunchCountToMin = (model.UpdateEarlyOutTimeHour * 60 ?? 0) + model.UpdateEarlyOutTimeMinute ?? 0;
            int? graceTime = (model.UpdateGraceTimeHour * 60 ?? 0) + model.UpdateGraceTimeMinute ?? 0;
            int? minimumWorkingTime = (model.UpdateMinimumWorkingTimeHour * 60 ?? 0) + model.UpdateMinimumWorkingTimeMinute ?? 0;
            int? minimumRequiredOvertime = (model.UpdateMinimumRequiredOvertimeHour * 60 ?? 0) + model.UpdateMinimumRequiredOvertimeMinute ?? 0;
            int? maximumAllowedOvertime = (model.UpdateMaximumAllowedOvertimeHour * 60 ?? 0) + model.UpdateMaximumAllowedOvertimeMinute ?? 0;
            int? mealBreakTime = (model.UpdateMealBreakTimeHour * 60 ?? 0) + model.UpdateMealBreakTimeMinute ?? 0;

            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.UpdateShiftID);
                if (entity == null)
                {
                    return false;
                }
                var beforeEntity = JsonConvert.DeserializeObject<ShiftUpdateSetupVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));

                entity.ShiftName = model.UpdateShiftName;
                entity.OrganizationID = model.UpdateOrganizationID;
                entity.StartTime = utcStartTime;
                entity.EndTime = utcEndTime;
                entity.IsLateCount = model.UpdateIsLateCount;
                entity.IsAutomaticORManualBreakTime = model.UpdateIsAutomaticORManualBreakTime;
                entity.IsMealBreakCompulsaryOrComplementaryDeductWithShift = model.UpdateIsMBCompulsaryOrComplementaryDeductWithShift;
                entity.IsAllowStartAndEndTime = model.UpdateIsAllowStartAndEndTime;
                entity.MealBreakStartTime = model.UpdateMealBreakStartTime.ToTimeOnly();
                entity.MealBreakEndTime = model.UpdateMealBreakEndTime.ToTimeOnly();
                entity.IsAllowOvertime = model.UpdateIsAllowOvertime;
                entity.GraceTime = graceTime;
                entity.MinimumWorkingTime = minimumWorkingTime;
                entity.MinimumRequiredOvertime = minimumRequiredOvertime;
                entity.MaximumAllowedOvertime = maximumAllowedOvertime;
                entity.MealBreakTime = mealBreakTime;
                entity.IsRestrictFlexibleInTime = model.UpdateIsRestrictFlexibleInTime;
                entity.InPunchCountFromMin = inPunchCountFromMin;
                entity.IsRestrictFlexibleOutTime = model.UpdateIsRestrictFlexibleOutTime;
                entity.OutPunchCountToMin = outPunchCountToMin;

                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;
                entity.UpdatedBy = model.UpdatedBy ?? null;
                await _genericRepository.UpdateAsync(entity);
                var afterEntity = JsonConvert.DeserializeObject<ShiftUpdateSetupVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));
                await _userInfoService.ActionLogAsync("Add Shift", ActionName.DataUpdated, beforeEntity, afterEntity, entity.ShiftID, model);
                await _genericRepository.CommitTransactionAsync();

                return true;
            }
            catch
            {
                await _genericRepository.RollbackTransactionAsync();
                return false;
            }
        }
        #endregion


        #region GetByIdAsync
        public async Task<ShiftUpdateSetupVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.GetByIdAsync(id);
                if (data == null) return null;

                return new ShiftUpdateSetupVM
                {
                    UpdateShiftID = data.ShiftID,
                    UpdateShiftName = data.ShiftName,
                    UpdateOrganizationID = data.OrganizationID,
                    UpdateStartTime = data.StartTime.HasValue ? TimeConversionHelper.ConvertTimeOnlyToUtc(data.StartTime.Value, _localizationContext) : null,
                    UpdateEndTime = data.EndTime.HasValue ? TimeConversionHelper.ConvertTimeOnlyToUtc(data.EndTime.Value, _localizationContext) : null,
                    UpdateIsLateCount = data.IsLateCount,
                    UpdateIsAutomaticORManualBreakTime = data.IsAutomaticORManualBreakTime,
                    UpdateIsMBCompulsaryOrComplementaryDeductWithShift = data.IsMealBreakCompulsaryOrComplementaryDeductWithShift,
                    UpdateIsAllowStartAndEndTime = data.IsAllowStartAndEndTime,
                    UpdateMealBreakStartTime = data.MealBreakStartTime?.ToString("hh:mm tt"),
                    UpdateMealBreakEndTime = data.MealBreakEndTime?.ToString("hh:mm tt"),
                    UpdateIsAllowOvertime = data.IsAllowOvertime,
                    UpdateGraceTimeHour = data.GraceTime / 60,
                    UpdateGraceTimeMinute = data.GraceTime % 60,
                    UpdateMinimumWorkingTimeHour = data.MinimumWorkingTime / 60,
                    UpdateMinimumWorkingTimeMinute = data.MinimumWorkingTime % 60,
                    UpdateMinimumRequiredOvertimeHour = data.MinimumRequiredOvertime / 60,
                    UpdateMinimumRequiredOvertimeMinute = data.MinimumRequiredOvertime % 60,
                    UpdateMaximumAllowedOvertimeHour = data.MaximumAllowedOvertime / 60,
                    UpdateMaximumAllowedOvertimeMinute = data.MaximumAllowedOvertime % 60,
                    UpdateMealBreakTimeHour = data.MealBreakTime / 60,
                    UpdateMealBreakTimeMinute = data.MealBreakTime % 60,
                    UpdateIsRestrictFlexibleInTime = data.IsRestrictFlexibleInTime,
                    UpdateEarlyInTimeHour = data.InPunchCountFromMin / 60,
                    UpdateEarlyInTimeMinute = data.InPunchCountFromMin % 60,
                    UpdateIsRestrictFlexibleOutTime = data.IsRestrictFlexibleOutTime,
                    UpdateEarlyOutTimeHour = data.OutPunchCountToMin / 60,
                    UpdateEarlyOutTimeMinute = data.OutPunchCountToMin % 60
                };
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., to a file or logging service)  
                throw; // Rethrow or return an error-specific response  
            }
        }
        #endregion


        #region IsNameUniqueAsync
        public async Task<bool> IsNameUniqueAsync(int id, string name)
        {
            var existingNames = await _genericRepository.FindAsync(b => b.DeletedAt == null && b.ShiftName != null && b.OrganizationID == id);

            var nameList = existingNames.Select(b => b.ShiftName);

            return !DuplicateChecker.IsDuplicate(name, nameList);
        }
        #endregion


        #region SoftDeleteAsync
        public async Task<ShiftsSetupVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.ShiftID));
                if (data == null || data.Count == 0)
                {
                    return new ShiftsSetupVM
                    {
                        Message = "No data found to delete."
                    };
                }
                var beforeEntity = JsonConvert.DeserializeObject<List<ShiftsSetupVM>>(JsonConvert.SerializeObject(data, JsonSettings.IgnoreReferenceLoop));
                var targetIds = data.Select(x => (int?)x.ShiftID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                    item.DeletedBy = requestVM.DeletedBy ?? null;
                }

                await _genericRepository.UpdateRangeAsync(data);
                  await _userInfoService.ActionLogDeleteAsync("Add Shift", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);
                await _genericRepository.CommitTransactionAsync();

                return new ShiftsSetupVM
                {
                    Message = $"{data.Count} data(s) deleted successfully."
                };
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                throw new Exception("Error occurred during the deletion of data.", ex);
            }
        }
        #endregion


        #region GetAllAsync
        public async Task<PaginationService<Shifts, ShiftsListVM>.PaginationResult<ShiftsListVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "ShiftID", string sortOrder = "desc", int? organizationID = null)
        {
            var query = _genericRepository.AllActive().AsNoTracking().Include(x => x.Organization).Where(x => x.DeletedAt == null);

            if (organizationID.HasValue && organizationID.Value > 0)
            {
                query = query.Where(x => x.OrganizationID == organizationID.Value);
            }

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "ShiftID" => sortOrder == "desc" ? query.OrderByDescending(x => x.ShiftID) : query.OrderBy(x => x.ShiftID),
                    "ShiftName" => sortOrder == "desc" ? query.OrderByDescending(x => x.ShiftName) : query.OrderBy(x => x.ShiftName),
                    "OrganizationName" => sortOrder == "desc" ? query.OrderByDescending(x => x.Organization.OrganizationName) : query.OrderBy(x => x.Organization.OrganizationName),
                    "StartTime" => sortOrder == "desc" ? query.OrderByDescending(x => x.StartTime) : query.OrderBy(x => x.StartTime),
                    "EndTime" => sortOrder == "desc" ? query.OrderByDescending(x => x.EndTime) : query.OrderBy(x => x.EndTime),
                    "GraceTime" => sortOrder == "desc" ? query.OrderByDescending(x => x.GraceTime) : query.OrderBy(x => x.GraceTime),
                    "MealBreakTime" => sortOrder == "desc" ? query.OrderByDescending(x => x.MealBreakTime) : query.OrderBy(x => x.MealBreakTime),
                    _ => query.OrderBy(x => x.ShiftID)
                };
            }

            if (pageSize == 0)
            {
                pageSize = await query.CountAsync();
                pageNumber = 1;
            }

            var result = await PaginationService<Shifts, ShiftsListVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.ShiftName, $"%{term}%"),
                x => new ShiftsListVM
                {
                    ShiftID = x.ShiftID,
                    ShiftName = x.ShiftName ?? "-",
                    OrganizationID = x.OrganizationID,
                    OrganizationName = x.Organization != null ? x.Organization.OrganizationName ?? "-" : "-",
                    StartTime = x.StartTime.HasValue ? TimeConversionHelper.ConvertUtcTimeOnlyToLocalFormatted(x.StartTime.Value, _localizationContext) : null,
                    EndTime = x.EndTime.HasValue ? TimeConversionHelper.ConvertUtcTimeOnlyToLocalFormatted(x.EndTime.Value, _localizationContext) : null,
                    IsLateCount = x.IsLateCount,
                    IsAutomaticORManualBreakTime = x.IsAutomaticORManualBreakTime,
                    IsMealBreakCompulsaryOrComplementaryDeductWithShift = x.IsMealBreakCompulsaryOrComplementaryDeductWithShift,
                    IsAllowStartAndEndTime = x.IsAllowStartAndEndTime,
                    MealBreakStartTime = x.MealBreakStartTime,
                    MealBreakEndTime = x.MealBreakEndTime,
                    IsAllowOvertime = x.IsAllowOvertime,
                    GraceTimeHour = x.GraceTime,
                    MinimumWorkingTimeHour = x.MinimumWorkingTime,
                    MinimumRequiredOvertimeHour = x.MinimumRequiredOvertime,
                    MaximumAllowedOvertimeHour = x.MaximumAllowedOvertime,
                    MealBreakTimeHour = x.MealBreakTime,
                    IsRestrictFlexibleInTime = x.IsRestrictFlexibleInTime,
                    IsRestrictFlexibleOutTime = x.IsRestrictFlexibleOutTime
                });

            return result;
        }
        #endregion
    }
}
