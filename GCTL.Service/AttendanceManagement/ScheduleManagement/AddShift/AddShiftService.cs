using GCTL.Core;
using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.Shift;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GCTL.Service.AttendanceManagement.ScheduleManagement.AddShift
{
    public class AddShiftService : AppService<Shifts>, IAddShiftService
    {
        #region Repositories
        private readonly IGenericRepository<Shifts> _genericRepository;
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<Organization> _organizationRepository;


        public AddShiftService(IGenericRepository<Shifts> genericRepository, IUserInfoService userInfoService, IGenericRepository<Organization> organizationRepository) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _userInfoService = userInfoService;
            _organizationRepository = organizationRepository;
        }
        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(ShiftsSetupVM model)
        {
            await _genericRepository.BeginTransactionAsync();
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
                        existingEntity.StartTime = model.StartTime;
                        existingEntity.EndTime = model.EndTime;
                        existingEntity.IsLateCount = model.IsLateCount;
                        existingEntity.IsAutomaticORManualBreakTime = model.IsAutomaticORManualBreakTime;
                        existingEntity.IsMealBreakCompulsaryOrComplementaryDeductWithShift = model.IsMealBreakCompulsaryOrComplementaryDeductWithShift;
                        existingEntity.IsAllowStartAndEndTime = model.IsAllowStartAndEndTime;
                        existingEntity.MealBreakStartTime = model.MealBreakStartTime;
                        existingEntity.MealBreakEndTime = model.MealBreakEndTime;
                        existingEntity.IsAllowOvertime = model.IsAllowOvertime;
                        existingEntity.GraceTime = model.GraceTime;
                        existingEntity.MinimumWorkingTime = model.MinimumWorkingTime;
                        existingEntity.MinimumRequiredOvertime = model.MinimumRequiredOvertime;
                        existingEntity.MaximumAllowedOvertime = model.MaximumAllowedOvertime;
                        existingEntity.MealBreakTime = model.MealBreakTime;

                        existingEntity.CreatedAt = DateTime.Now;
                        existingEntity.CreatedBy = model.CreatedBy;
                        existingEntity.LIP = model.LIP;
                        existingEntity.LMAC = model.LMAC;
                        existingEntity.UpdatedBy = model.UpdatedBy ?? null;
                        existingEntity.DeletedAt = null;
                        existingEntity.UpdatedAt = DateTime.Now;

                        await _genericRepository.UpdateAsync(existingEntity);
                        //var afterEntity = JsonConvert.DeserializeObject<ShiftsSetupVM>(JsonConvert.SerializeObject(entityToRestore));
                        //await _userInfoService.ActionLogAsync("Add Shift", ActionName.DataAdd, null, existingEntity, existingEntity.ShiftID, model);
                    }
                    else
                    {
                        Shifts entity = new Shifts();
                        entity.ShiftName = model.ShiftName;
                        entity.OrganizationID = organizationID;
                        entity.StartTime = model.StartTime;
                        entity.EndTime = model.EndTime;
                        entity.IsLateCount = model.IsLateCount;
                        entity.IsAutomaticORManualBreakTime = model.IsAutomaticORManualBreakTime;
                        entity.IsMealBreakCompulsaryOrComplementaryDeductWithShift = model.IsMealBreakCompulsaryOrComplementaryDeductWithShift;
                        entity.IsAllowStartAndEndTime = model.IsAllowStartAndEndTime;
                        entity.MealBreakStartTime = model.MealBreakStartTime;
                        entity.MealBreakEndTime = model.MealBreakEndTime;
                        entity.IsAllowOvertime = model.IsAllowOvertime;
                        entity.GraceTime = model.GraceTime;
                        entity.MinimumWorkingTime = model.MinimumWorkingTime;
                        entity.MinimumRequiredOvertime = model.MinimumRequiredOvertime;
                        entity.MaximumAllowedOvertime = model.MaximumAllowedOvertime;
                        entity.MealBreakTime = model.MealBreakTime;
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


        #region Update
        public async Task<bool> UpdateAsync(ShiftUpdateSetupVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.UpdateShiftID);
                if (entity == null)
                {
                    return false;
                }
                //var beforeEntity = JsonConvert.DeserializeObject<ShiftUpdateSetupVM>(JsonConvert.SerializeObject(entity));

                entity.ShiftName = model.UpdateShiftName;
                entity.OrganizationID = model.UpdateOrganizationID;
                entity.StartTime = model.UpdateStartTime.ToTimeOnly();
                entity.EndTime = model.UpdateEndTime.ToTimeOnly();
                entity.IsLateCount = model.UpdateIsLateCount;
                entity.IsAutomaticORManualBreakTime = model.UpdateIsAutomaticORManualBreakTime;
                entity.IsMealBreakCompulsaryOrComplementaryDeductWithShift = model.UpdateIsMBCompulsaryOrComplementaryDeductWithShift;
                entity.IsAllowStartAndEndTime = model.UpdateIsAllowStartAndEndTime;
                entity.MealBreakStartTime = model.UpdateMealBreakStartTime.ToTimeOnly();
                entity.MealBreakEndTime = model.UpdateMealBreakEndTime.ToTimeOnly();
                entity.IsAllowOvertime = model.UpdateIsAllowOvertime;
                entity.GraceTime = model.UpdateGraceTime;
                entity.MinimumWorkingTime = model.UpdateMinimumWorkingTime;
                entity.MinimumRequiredOvertime = model.UpdateMinimumRequiredOvertime;
                entity.MaximumAllowedOvertime = model.UpdateMaximumAllowedOvertime;
                entity.MealBreakTime = model.UpdateMealBreakTime;

                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;
                entity.UpdatedBy = model.UpdatedBy ?? null;
                await _genericRepository.UpdateAsync(entity);
                //var afterEntity = JsonConvert.DeserializeObject<ShiftUpdateSetupVM>(JsonConvert.SerializeObject(entity));
                //await _userInfoService.ActionLogAsync("Add Shift", ActionName.DataUpdated, beforeEntity, afterEntity, entity.ShiftID, model);
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
                    UpdateStartTime = data.StartTime?.ToString("hh:mm tt"),
                    UpdateEndTime = data.EndTime?.ToString("hh:mm tt"),
                    UpdateIsLateCount = data.IsLateCount,
                    UpdateIsAutomaticORManualBreakTime = data.IsAutomaticORManualBreakTime,
                    UpdateIsMBCompulsaryOrComplementaryDeductWithShift = data.IsMealBreakCompulsaryOrComplementaryDeductWithShift,
                    UpdateIsAllowStartAndEndTime = data.IsAllowStartAndEndTime,
                    UpdateMealBreakStartTime = data.MealBreakStartTime?.ToString("hh:mm tt"),
                    UpdateMealBreakEndTime = data.MealBreakEndTime?.ToString("hh:mm tt"),
                    UpdateIsAllowOvertime = data.IsAllowOvertime,
                    UpdateGraceTime = data.GraceTime,
                    UpdateMinimumWorkingTime = data.MinimumWorkingTime,
                    UpdateMinimumRequiredOvertime = data.MinimumRequiredOvertime,
                    UpdateMaximumAllowedOvertime = data.MaximumAllowedOvertime,
                    UpdateMealBreakTime = data.MealBreakTime,
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

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                    item.DeletedBy = requestVM.DeletedBy ?? null;
                }

                await _genericRepository.UpdateRangeAsync(data);

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
        public async Task<PaginationService<Shifts, ShiftsSetupVM>.PaginationResult<ShiftsSetupVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "ShiftID", string sortOrder = "desc", int? organizationID = null)
        {
            var query = _genericRepository.All().AsNoTracking().Include(x => x.Organization).Where(x => x.DeletedAt == null);

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

            var result = await PaginationService<Shifts, ShiftsSetupVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.ShiftName, $"%{term}%"),
                x => new ShiftsSetupVM
                {
                    ShiftID = x.ShiftID,
                    ShiftName = x.ShiftName ?? "-",
                    OrganizationID = x.OrganizationID,
                    OrganizationName = x.Organization != null ? x.Organization.OrganizationName ?? "-" : "-",
                    //StartTime = x.StartTime.HasValue ? x.StartTime.Value.ToString("hh\\:mm") : "-",
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    IsLateCount = x.IsLateCount,
                    IsAutomaticORManualBreakTime = x.IsAutomaticORManualBreakTime,
                    IsMealBreakCompulsaryOrComplementaryDeductWithShift = x.IsMealBreakCompulsaryOrComplementaryDeductWithShift,
                    IsAllowStartAndEndTime = x.IsAllowStartAndEndTime,
                    MealBreakStartTime = x.MealBreakStartTime,
                    MealBreakEndTime = x.MealBreakEndTime,
                    IsAllowOvertime = x.IsAllowOvertime,
                    GraceTime = x.GraceTime,
                    MinimumWorkingTime = x.MinimumWorkingTime,
                    MinimumRequiredOvertime = x.MinimumRequiredOvertime,
                    MaximumAllowedOvertime = x.MaximumAllowedOvertime,
                    MealBreakTime = x.MealBreakTime,
                });

            return result;
        }
        #endregion


        #region GetOrganizations
        public IEnumerable<CommonSelectVM> GetOrganizations()
        {
            var data = _organizationRepository.AllActive()
                .Select(x => new CommonSelectVM
                {
                    Id = x.OrganizationID,
                    Name = x.OrganizationName
                }).ToList();
            return data;
        }
        #endregion
    }
}
