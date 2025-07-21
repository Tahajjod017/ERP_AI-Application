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

        #endregion

        #region
        public async Task<WeekendSettingVM> GetByIdAsync(int id)
        {
            // Step 1: Retrieve WeekendSetting (including soft-deleted records)
            var entityList = await _genericRepository.FindAsync(x =>
                x.WeekendSettingID == id);  // No check for DeletedAt, assuming you want both active and soft-deleted ones

            var entity = entityList.FirstOrDefault();

            if (entity == null)
                return null;

            // Step 2: Retrieve related WeekendDays (only non-deleted ones)
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
                WeekendDays = weekendDayIds,  // List of weekend day IDs

                CreatedBy = entity.CreatedBy,
                UpdatedBy = entity.UpdatedBy

                // Optionally map other fields if needed (LIP, LMAC, etc.)
            };

            return model;
        }

        #endregion


        #region IsNameUniqueAsync
        public async Task<bool> IsNameUniqueAsync(int organizationId, int organizationBranchId)
        {
            
            var existingList = await _genericRepository.FindAsync(b =>
                                    b.DeletedAt == null &&   
                                    b.WeekendSettingID != null);

           
            var nameList = existingList.Select(b => new
            {
                b.OrganizationID,
                b.OrganizationBranchID,
            }).ToList();

            
            return !nameList.Any(x => x.OrganizationID == organizationId &&  x.OrganizationBranchID == organizationBranchId);

        }

        #endregion


        #region Soft Delete

        public async Task<WeekendSettingVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                // Step 1: Find WeekendSettings to soft delete
                var settings = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.WeekendSettingID));
                if (settings == null || settings.Count == 0)
                {
                    return new WeekendSettingVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                // Step 2: Find all related WeekendDays for these settings
                var settingIds = settings.Select(s => s.WeekendSettingID).ToList();
                var weekendDays = await genericRepositoryWeekdays.FindAsync(wd => settingIds.Contains(wd.WeekendSettingID.Value) && wd.DeletedAt == null);

                // Step 3: Mark all as soft deleted
                var now = DateTime.Now;
                foreach (var setting in settings)
                {
                    setting.DeletedAt = now;
                    setting.DeletedBy = requestVM.DeletedBy;
                    // setting.LIP = requestVM.LIP;
                    // setting.LMAC = requestVM.LMAC;
                }
                foreach (var day in weekendDays)
                {
                    day.DeletedAt = now;
                    day.DeletedBy = requestVM.DeletedBy;
                    // day.LIP = requestVM.LIP;
                    // day.LMAC = requestVM.LMAC;
                }

                // Step 4: Update both tables
                await _genericRepository.UpdateRangeAsync(settings);
                await genericRepositoryWeekdays.UpdateRangeAsync(weekendDays);

                // Step 5: Commit Transaction
                await _genericRepository.CommitTransactionAsync();

                return new WeekendSettingVM
                {
                    Message = $"{settings.Count} WeekendSetting(s) and {weekendDays.Count} related WeekendDay(s) soft deleted successfully."
                };
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                throw new Exception("Error occurred during the deletion of data.", ex);
            }
        }
        #endregion
        #region
        public async Task<PaginationService<WeekendSettings, WeekendSettingVM>.PaginationResult<WeekendSettingVM>> GetAllAsync(
                    int pageNumber = 1,
                    int pageSize = 5,
                    string searchTerm = "",
                    string sortColumn = "OrganizationID",
                    string sortOrder = "desc",
                    int? organizationID = null)
        {
            var query = _genericRepository.All()
                .AsNoTracking()
                .Include(x => x.Organization)
                .Include(x => x.OrganizationBranch)
                .Include(x => x.WeekendDays)
                .Where(x => x.DeletedAt == null);

            if (organizationID.HasValue && organizationID.Value > 0)
            {
                query = query.Where(x => x.Organization.OrganizationID == organizationID.Value);
            }

            var result = await PaginationService<WeekendSettings, WeekendSettingVM>.GetPaginatedData(
                query,
                pageNumber,
                pageSize,
                searchTerm,
                sortColumn,
                sortOrder,
                term => x => EF.Functions.Like(x.Organization.OrganizationName, $"%{term}%")
                          || EF.Functions.Like(x.OrganizationBranch.OrganizationBranchName, $"%{term}%"),
                x => new WeekendSettingVM
                {
                    WeekendDayID = x.WeekendDays?.Select(wd => wd.WeekendDayID).FirstOrDefault(),
                    WeekendSettingID = x.WeekendSettingID,
                    OrganizationID = x.Organization?.OrganizationID,
                    OrganizationBranchID = x.OrganizationBranch?.OrganizationBranchID,
                    OrganizationName = x.Organization != null ? x.Organization?.OrganizationName : "_",
                    OrganizationBranchName = x.OrganizationBranch != null ? x.OrganizationBranch?.OrganizationBranchName : "_",

                    // Aggregate weekday names into a single string
                    WeekendTitle = string.Join(", ", x.WeekendDays
                        .Select(wd => GetDayName(wd.WeekdayNumber))
                        .Distinct()),  // Optionally use Distinct to avoid duplicates

                    CreatedBy = x.CreatedBy,
                    UpdatedBy = x.UpdatedBy
                });

            return result;
        }

        private static string GetDayName(int? weekdayNumber)
        {
            // Adjust the weekday mapping if your data does not match the DayOfWeek enum (e.g., 1 for Monday, etc.)
            return Enum.GetName(typeof(DayOfWeek), (DayOfWeek)(weekdayNumber ?? 0)) ?? $"Day {weekdayNumber}";
        }


        #endregion

        #region GetOrganizationsAsync and byId
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
