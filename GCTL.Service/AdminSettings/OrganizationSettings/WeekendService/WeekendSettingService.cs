using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AdminSettingsVM;
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
using System.Web.Mvc;

namespace GCTL.Service.AdminSettings.OrganizationSettings.WeekendService
{
    public class WeekendSettingService : AppService<WeekendSettings> , IWeekendSettingService
    {
        #region Repositories & Services
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<WeekendSettings> _genericRepository;
        private readonly IGenericRepository<Organization> _genericRepositoryOraganization;
        private readonly IGenericRepository<OrganizationBranches> genericRepositoryBranches;
        private readonly IGenericRepository<WeekendDays> genericRepositoryWeekdays;

        public WeekendSettingService(IUserInfoService userInfoService, IGenericRepository<WeekendSettings> genericRepository, IGenericRepository<Organization> genericRepositoryOraganization, IGenericRepository<OrganizationBranches> genericRepositoryBranches, IGenericRepository<WeekendDays> genericRepositoryWeekdays) : base(genericRepository)
        {
            _userInfoService = userInfoService;
            _genericRepository = genericRepository;
            _genericRepositoryOraganization = genericRepositoryOraganization;
            this.genericRepositoryBranches = genericRepositoryBranches;
            this.genericRepositoryWeekdays = genericRepositoryWeekdays;
        }
        #endregion
        #region AddAsync  
        public async Task<bool> AddAsync(WeekendSettingVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                // Step 1: Try to find an existing soft-deleted record
                var existingEntityList = await _genericRepository.FindAsync(e =>
                    e.DeletedAt != null &&
                    e.OrganizationID == model.OrganizationID &&
                    (
                        (model.OrganizationBranchID == null && e.OrganizationBranchID == null) ||
                        (model.OrganizationBranchID != null && e.OrganizationBranchID == model.OrganizationBranchID)
                    )
                );

                var existingEntity = existingEntityList.FirstOrDefault();
                WeekendSettings setting;

                if (existingEntity != null)
                {
                    // Step 2: Restore and update soft-deleted WeekendSetting
                    existingEntity.OrganizationID = model.OrganizationID;
                    existingEntity.OrganizationBranchID = model.OrganizationBranchID;
                    existingEntity.UpdatedAt = DateTime.Now;
                    existingEntity.UpdatedBy = model.UpdatedBy ?? model.CreatedBy;
                    existingEntity.CreatedAt = DateTime.Now;
                    existingEntity.CreatedBy = model.CreatedBy;
                    existingEntity.DeletedAt = null;
                    existingEntity.DeletedBy = null;

                    await _genericRepository.UpdateAsync(existingEntity);
                    setting = existingEntity;
                }
                else
                {
                    // Step 3: Insert new WeekendSetting
                    setting = new WeekendSettings
                    {
                        OrganizationID = model.OrganizationID,
                        OrganizationBranchID = model.OrganizationBranchID,
                        CreatedAt = DateTime.Now,
                        CreatedBy = model.CreatedBy,
                        // Optional: LIP = model.LIP, LMAC = model.LMAC
                    };

                    await _genericRepository.AddAsync(setting);
                }

                // Step 4: Soft-delete existing WeekendDays for this setting (if any)
                var oldWeekendDays = await genericRepositoryWeekdays.FindAsync(w =>
                    w.WeekendSettingID == setting.WeekendSettingID && w.DeletedAt == null);

                foreach (var oldDay in oldWeekendDays)
                {
                    oldDay.DeletedAt = DateTime.Now;
                    oldDay.DeletedBy = model.UpdatedBy ?? model.CreatedBy;
                    await genericRepositoryWeekdays.UpdateAsync(oldDay);
                }

                // Step 5: Insert new WeekendDays
                foreach (var weekday in model.WeekendDays)
                {
                    var wd = new WeekendDays
                    {
                        WeekendSettingID = setting.WeekendSettingID,
                        WeekdayNumber = weekday,
                        CreatedAt = DateTime.Now,
                        CreatedBy = model.CreatedBy,
                        // Optional: LIP = model.LIP, LMAC = model.LMAC
                    };

                    await genericRepositoryWeekdays.AddAsync(wd);
                }

                // Step 6: Commit Transaction
                await _genericRepository.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                throw new Exception("Failed to add weekend setting", ex);
            }
        }




        #endregion


        #region Update
        public async Task<bool> UpdateAsync(WeekendSettingVM model)
        {
            await _genericRepository.BeginTransactionAsync();

            try
            {
                // Step 1: Find existing record (including soft-deleted ones)
                var existingList = await _genericRepository.FindAsync(e =>
                    (e.DeletedAt == null || e.DeletedAt != null) &&
                    e.WeekendSettingID == model.WeekendSettingID
                );

                var entity = existingList.FirstOrDefault();

                if (entity == null)
                {
                    throw new Exception("Weekend setting not found.");
                }

                // Step 2: Update fields
                entity.OrganizationID = model.OrganizationID;
                entity.OrganizationBranchID = model.OrganizationBranchID;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = model.UpdatedBy ?? model.CreatedBy;
                entity.DeletedAt = null; // Restore if soft-deleted
                entity.DeletedBy = null;
                //entity.LIP = model.LIP;
                //entity.LMAC = model.LMAC;

                await _genericRepository.UpdateAsync(entity);

                // Step 3: Soft-delete old weekend days
                var oldDays = await genericRepositoryWeekdays.FindAsync(d =>
                    d.WeekendSettingID == entity.WeekendSettingID &&
                    d.DeletedAt == null);

                foreach (var old in oldDays)
                {
                    old.DeletedAt = DateTime.Now;
                    old.DeletedBy = model.UpdatedBy ?? model.CreatedBy;
                    await genericRepositoryWeekdays.UpdateAsync(old);
                }

                // Step 4: Add new weekend days
                foreach (var day in model.WeekendDays)
                {
                    var newDay = new WeekendDays
                    {
                        WeekendSettingID = entity.WeekendSettingID,
                        WeekdayNumber = day,
                        CreatedAt = DateTime.Now,
                        CreatedBy = model.UpdatedBy ?? model.CreatedBy,
                        //LIP = model.LIP,
                        //LMAC = model.LMAC
                    };

                    await genericRepositoryWeekdays.AddAsync(newDay);
                }

                // Step 5: Commit
                await _genericRepository.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                throw new Exception("Failed to update weekend setting: " + ex.Message, ex);
            }
        }


        public async Task<WeekendSettingVM> GetByIdAsync(int id)
        {
            // Step 1: Retrieve WeekendSetting (excluding soft-deleted)  
            var entityList = await _genericRepository.FindAsync(x =>
                x.WeekendSettingID == id && x.DeletedAt == null);

            var entity = entityList.FirstOrDefault();

            if (entity == null)
                return null;

            // Step 2: Retrieve related WeekendDays  
            var weekdayEntities = await genericRepositoryWeekdays.FindAsync(d =>
                d.WeekendSettingID == entity.WeekendSettingID && d.DeletedAt == null);

            var weekendDayIds = weekdayEntities
                .Select(d => d.WeekdayNumber)
                .Where(d => d.HasValue) // Ensure only non-null values are selected  
                .Select(d => d.Value) // Convert nullable int to int  
                .ToList();

            // Step 3: Map to ViewModel  
            var model = new WeekendSettingVM
            {
                WeekendSettingID = entity.WeekendSettingID,
                OrganizationID = entity.OrganizationID,
                OrganizationBranchID = entity.OrganizationBranchID,
                WeekendDays = weekendDayIds,

                CreatedBy = entity.CreatedBy,
                UpdatedBy = entity.UpdatedBy,
                //CreatedAt = entity.CreatedAt,  
                //UpdatedAt = entity.UpdatedAt,  
                //LIP = entity.LIP,  
                //LMAC = entity.LMAC  
            };

            return model;
        }


        #endregion


        #region IsNameUniqueAsync
        public async Task<bool> IsNameUniqueAsync(string name)
        {
            // This is only meaningful if you later add a WeekendTitle or similar
            var existingList = await _genericRepository.FindAsync(b =>
                b.DeletedAt == null && b.WeekendSettingID != null);

            var nameList = existingList.Select(b => b.WeekendSettingID.ToString()); // placeholder

            return !DuplicateChecker.IsDuplicate(name, nameList);
        }

        #endregion


        #region Soft Delete
        public async Task<WeekendSettingVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.WeekendSettingID));
                if (data == null || data.Count == 0)
                {
                    return new WeekendSettingVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                //var beforeEntity = JsonConvert.DeserializeObject<List<WeekendSettingVM>>(JsonConvert.SerializeObject(data));
                var targetIds = data.Select(x => (int?)x.WeekendSettingID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.DeletedBy = requestVM.DeletedBy;
                    //item.LIP = requestVM.LIP;
                    //item.LMAC = requestVM.LMAC;
                }

                await _genericRepository.UpdateRangeAsync(data);

               // await _userInfoService.ActionLogDeleteAsync("WeekendSettings", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);

                await _genericRepository.CommitTransactionAsync();

                return new WeekendSettingVM
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


        public async Task<PaginationService<WeekendDays, WeekendSettingVM>.PaginationResult<WeekendSettingVM>> GetAllAsync(
                             int pageNumber = 1,
                             int pageSize = 5,
                             string searchTerm = "",
                             string sortColumn = "OrganizationID",
                             string sortOrder = "desc",
                             int? organizationID = null)
        {
            var query = genericRepositoryWeekdays.All()
                .AsNoTracking()
                .Include(x => x.WeekendSetting)
                .ThenInclude(x => x.Organization)
                .Include(x => x.WeekendSetting)
                .ThenInclude(x => x.OrganizationBranch)
                .Where(x => x.DeletedAt == null);

            if (organizationID.HasValue && organizationID.Value > 0)
            {
                query = query.Where(x => x.WeekendSetting.Organization.OrganizationID == organizationID.Value);
            }

            var result = await PaginationService<WeekendDays, WeekendSettingVM>.GetPaginatedData(
                query,
                pageNumber,
                pageSize,
                searchTerm,
                sortColumn,
                sortOrder,
                term => x => EF.Functions.Like(x.WeekendSetting.Organization.OrganizationName, $"%{term}%")
                          || EF.Functions.Like(x.WeekendSetting.OrganizationBranch.OrganizationBranchName, $"%{term}%"),
                x => new WeekendSettingVM
                {
                    WeekendDayID = x.WeekendDayID,
                    WeekendSettingID = x.WeekendSetting.WeekendSettingID,
                    OrganizationID = x.WeekendSetting.Organization.OrganizationID,
                    OrganizationBranchID = x.WeekendSetting.OrganizationBranch.OrganizationBranchID,
                    OrganizationName = x.WeekendSetting.Organization != null ? x.WeekendSetting.Organization.OrganizationName : "_",
                    OrganizationBranchName = x.WeekendSetting.OrganizationBranch != null ? x.WeekendSetting.OrganizationBranch.OrganizationBranchName : "_",

                    // Map weekday numbers to names
                    WeekendTitle = x.WeekdayNumber.HasValue ? GetDayName(x.WeekdayNumber) : "_",

                    CreatedBy = x.CreatedBy,
                    UpdatedBy = x.UpdatedBy
                });

            return result;
        }
        private static string GetDayName(int? weekdayNumber)
        {
            return Enum.GetName(typeof(DayOfWeek), weekdayNumber)
                   ?? $"Day {weekdayNumber}";
        }

        #endregion

        #region GetOrganizationsAsync
        public async Task<List<SelectListItem>> GetOrganizationsAsync()
        {
            var organizations = await _genericRepositoryOraganization.All()
                .Where(o => o.DeletedAt == null)
                .Select(o => new SelectListItem
                {
                    Value = o.OrganizationID.ToString(),
                    Text = o.OrganizationName
                })
                .ToListAsync();

            return organizations;
        }
        #endregion

        #region GetBranchesByOrganizationAsync
        // Optional: Status list from a Status table or enum
        public async Task<List<SelectListItem>> GetBranchesByOrganizationIdAsync(int organizationId)
        {
            var branches = await genericRepositoryBranches.All()
                .Where(b => b.OrganizationID == organizationId && b.DeletedAt == null)
                .OrderBy(b => b.OrganizationBranchName)
                .ToListAsync();

            return branches.Select(b => new SelectListItem
            {
                Value = b.OrganizationBranchID.ToString(),
                Text = b.OrganizationBranchName
            }).ToList();
        }

        #endregion
    }
}
