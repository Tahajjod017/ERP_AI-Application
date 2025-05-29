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
                var existingEntity = await _genericRepository.FindAsync(b => b.ShiftName == model.ShiftName && b.DeletedAt != null);
                if (existingEntity.Any())
                {

                    var entityToRestore = existingEntity.FirstOrDefault();
                    if (entityToRestore == null) return false;

                    entityToRestore.ShiftName = model.ShiftName;
                    //entityToRestore.OrganizationID = model.OrganizationID;
                    entityToRestore.StartTime = model.StartTime;
                    entityToRestore.EndTime = model.EndTime;
                    entityToRestore.IsLateCount = model.IsLateCount;
                    entityToRestore.IsAutomaticORManualBreakTime = model.IsAutomaticORManualBreakTime;
                    entityToRestore.IsMealBreakCompulsaryOrComplementaryDeductWithShift = model.IsMealBreakCompulsaryOrComplementaryDeductWithShift;
                    entityToRestore.IsAllowStartAndEndTime = model.IsAllowStartAndEndTime;
                    entityToRestore.MealBreakStartTime = model.MealBreakStartTime;
                    entityToRestore.MealBreakEndTime = model.MealBreakEndTime;
                    entityToRestore.IsAllowOvertime = model.IsAllowOvertime;
                    entityToRestore.GraceTime = model.GraceTime;
                    entityToRestore.MinimumWorkingTime = model.MinimumWorkingTime;
                    entityToRestore.MinimumRequiredOvertime = model.MinimumRequiredOvertime;
                    entityToRestore.MaximumAllowedOvertime = model.MaximumAllowedOvertime;
                    entityToRestore.MealBreakTime = model.MealBreakTime;

                    entityToRestore.CreatedAt = DateTime.Now;
                    entityToRestore.CreatedBy = model.CreatedBy;
                    entityToRestore.LIP = model.LIP;
                    entityToRestore.LMAC = model.LMAC;
                    entityToRestore.UpdatedBy = model.UpdatedBy ?? null;
                    entityToRestore.DeletedAt = null;
                    entityToRestore.UpdatedAt = DateTime.Now;

                    await _genericRepository.UpdateAsync(entityToRestore);
                    var afterEntity = JsonConvert.DeserializeObject<ShiftsSetupVM>(JsonConvert.SerializeObject(entityToRestore));
                    await _userInfoService.ActionLogAsync("Action Taken", ActionName.DataAdd, null, entityToRestore, entityToRestore.ShiftID, model);
                }
                else
                {
                    foreach(var organizationID in model.OrganizationIDs)
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
                        await _userInfoService.ActionLogAsync("Action Taken", ActionName.DataAdd, null, entity, entity.ShiftID, model);
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
        public async Task<bool> UpdateAsync(ShiftsSetupVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.ShiftID);
                if (entity == null)
                {
                    return false;
                }
                var beforeEntity = JsonConvert.DeserializeObject<ShiftsSetupVM>(JsonConvert.SerializeObject(entity));

                entity.ShiftName = model.ShiftName;
                //entity.OrganizationID = model.OrganizationID;
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

                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;
                entity.UpdatedBy = model.UpdatedBy ?? null;
                await _genericRepository.UpdateAsync(entity);
                var afterEntity = JsonConvert.DeserializeObject<ShiftsSetupVM>(JsonConvert.SerializeObject(entity));
                await _userInfoService.ActionLogAsync("Action Taken", ActionName.DataUpdated, beforeEntity, afterEntity, entity.ShiftID, model);
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
        public async Task<ShiftsSetupVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.GetByIdAsync(id);
                if (data == null) return null;

                return new ShiftsSetupVM
                {
                    ShiftID = data.ShiftID,
                    ShiftName = data.ShiftName,
                    //OrganizationID = data.OrganizationID,
                    StartTime = data.StartTime,
                    EndTime = data.EndTime,
                    IsLateCount = data.IsLateCount,
                    IsAutomaticORManualBreakTime = data.IsAutomaticORManualBreakTime,
                    IsMealBreakCompulsaryOrComplementaryDeductWithShift = data.IsMealBreakCompulsaryOrComplementaryDeductWithShift,
                    IsAllowStartAndEndTime = data.IsAllowStartAndEndTime,
                    MealBreakStartTime = data.MealBreakStartTime,
                    MealBreakEndTime = data.MealBreakEndTime,
                    IsAllowOvertime = data.IsAllowOvertime,
                    GraceTime = data.GraceTime,
                    MinimumWorkingTime = data.MinimumWorkingTime,
                    MinimumRequiredOvertime = data.MinimumRequiredOvertime,
                    MaximumAllowedOvertime = data.MaximumAllowedOvertime,
                    MealBreakTime = data.MealBreakTime,
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
        public async Task<bool> IsNameUniqueAsync(string name)
        {
            var existingNames = await _genericRepository.FindAsync(b => b.DeletedAt == null && b.ShiftName != null);

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
        public async Task<PaginationService<Shifts, ShiftsSetupVM>.PaginationResult<ShiftsSetupVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "ShiftID", string sortOrder = "desc")
        {
            var query = _genericRepository.All().AsNoTracking().Include(x => x.Organization).Where(x => x.DeletedAt == null);

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "ShiftID" => sortOrder == "desc" ? query.OrderByDescending(x => x.ShiftID) : query.OrderBy(x => x.ShiftID),
                    "ShiftName" => sortOrder == "desc" ? query.OrderByDescending(x => x.ShiftName) : query.OrderBy(x => x.ShiftName),
                    _ => query.OrderBy(x => x.ShiftID)
                };
            }

            var result = await PaginationService<Shifts, ShiftsSetupVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.ShiftName, $"%{term}%"),
                x => new ShiftsSetupVM
                {
                    ShiftID = x.ShiftID,
                    ShiftName = x.ShiftName ?? "-",
                    OrganizationName = x.Organization != null ? x.Organization.OrganizationName ?? "-" : "-",
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


        #region 
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
