using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Core.ViewModels.HRMsettingsVM;
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

namespace GCTL.Service.HRMsettings.ProbationService
{
    public class ProbationSettingService : AppService<ProbetionPeriodSettings> , IProbationSettingService
    {
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<ProbetionPeriodSettings> _genericRepository;
        private readonly IGenericRepository<Organization> _genericRepositoryOraganization;

        public ProbationSettingService(IUserInfoService userInfoService, IGenericRepository<ProbetionPeriodSettings> genericRepository, IGenericRepository<Organization> genericRepositoryOraganization):base(genericRepository)
        {
            _userInfoService = userInfoService;
            _genericRepository = genericRepository;
            _genericRepositoryOraganization = genericRepositoryOraganization;
        }

        #region AddAsync  
        public async Task<bool> AddAsync(ProbationSettingVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                // Step 1: Find an existing soft-deleted ProbationSetting
                var existingEntityList = await _genericRepository.FindAsync(e =>
                    e.DeletedAt != null &&
                    e.OrganizationID == model.OrganizationID
                );

                var existingEntity = existingEntityList.FirstOrDefault();
                ProbetionPeriodSettings probationSetting;

                if (existingEntity != null)
                {
                    // Step 2: Restore and update soft-deleted ProbationSetting
                    existingEntity.OrganizationID = model.OrganizationID;
                    existingEntity.Period = model.Period;
                    existingEntity.PeriodType = model.PeriodType;
                    existingEntity.UpdatedAt = DateTime.Now;
                    existingEntity.UpdatedBy = model.UpdatedBy ?? model.CreatedBy;
                    existingEntity.CreatedAt = DateTime.Now;
                    existingEntity.CreatedBy = model.CreatedBy;
                    existingEntity.DeletedAt = null;
                    existingEntity.DeletedBy = null;

                    await _genericRepository.UpdateAsync(existingEntity);
                    probationSetting = existingEntity;
                }
                else
                {
                    // Step 3: Insert new ProbationSetting
                    probationSetting = new ProbetionPeriodSettings
                    {
                        OrganizationID = model.OrganizationID,
                        Period = model.Period,
                        PeriodType = model.PeriodType,
                        CreatedAt = DateTime.Now,
                        CreatedBy = model.CreatedBy
                        // Optional: Additional fields can be added here
                    };

                    await _genericRepository.AddAsync(probationSetting);
                }

                // Step 4: Commit Transaction
                await _genericRepository.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                throw new Exception("Failed to add probation setting", ex);
            }
        }
        #endregion

        #region Update
        public async Task<bool> UpdateAsync(ProbationSettingVM model)
        {
            await _genericRepository.BeginTransactionAsync();

            try
            {
                // Step 1: Find existing ProbationSetting record (including soft-deleted ones)
                var existingEntityList = await _genericRepository.FindAsync(e =>
                    (e.DeletedAt == null || e.DeletedAt != null) &&
                    e.ProbetionPeriodSettingID == model.ProbetionPeriodSettingID // Primary key for the model
                );

                var existingEntity = existingEntityList.FirstOrDefault();

                if (existingEntity == null)
                {
                    throw new Exception("Probation Setting not found.");
                }

                // Step 2: Update properties
                existingEntity.OrganizationID = model.OrganizationID;
                existingEntity.Period = model.Period;
                existingEntity.PeriodType = model.PeriodType;

                // Restore soft-deleted record (if previously deleted)
                existingEntity.DeletedAt = null;
                existingEntity.DeletedBy = null;

                existingEntity.UpdatedAt = DateTime.Now;
                existingEntity.UpdatedBy = model.UpdatedBy ?? model.CreatedBy; // Default or current user

                // Step 3: Save changes
                await _genericRepository.UpdateAsync(existingEntity);

                // Step 4: Commit transaction
                await _genericRepository.CommitTransactionAsync();

                return true;
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                // You can log the error here
                throw new Exception("Failed to update probation setting: " + ex.Message, ex);
            }
        }
        #endregion

        #region Soft Delete
        public async Task<ProbationSettingVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                // Step 1: Find records to soft delete
                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.ProbetionPeriodSettingID));

                if (data == null || data.Count == 0)
                {
                    return new ProbationSettingVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                // Step 2: Serialize before state for logging purposes
               // var beforeEntity = JsonConvert.DeserializeObject<List<ProbationSettingVM>>(JsonConvert.SerializeObject(data));
                var targetIds = data.Select(x => (int?)x.ProbetionPeriodSettingID).ToList();

                // Step 3: Apply soft delete to each record
                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.DeletedBy = requestVM.DeletedBy;
                    // Optionally, you can add LIP/LMAC if needed: item.LIP = requestVM.LIP;
                    // item.LMAC = requestVM.LMAC;
                }

                // Step 4: Update the records with soft delete
                await _genericRepository.UpdateRangeAsync(data);

                // Step 5: Log the delete action for auditing
               // await _userInfoService.ActionLogDeleteAsync("ProbationSetting", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);

                // Step 6: Commit the transaction
                await _genericRepository.CommitTransactionAsync();

                return new ProbationSettingVM
                {
                    Message = $"{data.Count} data(s) soft deleted successfully."
                };
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                throw new Exception("Error occurred during the soft deletion of data.", ex);
            }
        }
        #endregion

        #region Get
        public async Task<ProbationSettingVM> GetByIdAsync(int id)
        {
            // Retrieve the ProbationSetting entity by ID, excluding soft-deleted records
            var entityList = await _genericRepository.FindAsync(x => x.ProbetionPeriodSettingID == id && x.DeletedAt == null);
            var entity = entityList.FirstOrDefault();

            if (entity == null)
                return null;

            // Map to ViewModel
            var model = new ProbationSettingVM
            {
                ProbetionPeriodSettingID = entity.ProbetionPeriodSettingID,
                OrganizationID = entity.OrganizationID,
                Period = entity.Period,
                PeriodType = entity.PeriodType,
                CreatedBy = entity.CreatedBy,
                UpdatedBy = entity.UpdatedBy
                // You can add additional properties (LIP, LMAC) if required
            };

            return model;
        }
        #endregion


        #region GetAll
        public async Task<PaginationService<ProbetionPeriodSettings, ProbationSettingVM>.PaginationResult<ProbationSettingVM>> GetAllAsync(
            int pageNumber = 1,
            int pageSize = 5,
            string searchTerm = "",
            string sortColumn = "OrganizationID",
            string sortOrder = "desc",
            int? organizationID = null)
        {
            var query = _genericRepository.All()
                .AsNoTracking()
                .Include(x => x.Organization) // Include related Organization entity
                .Where(x => x.DeletedAt == null); // Filter out soft-deleted records

            // Filter by organization if provided
            if (organizationID.HasValue && organizationID.Value > 0)
            {
                query = query.Where(x => x.OrganizationID == organizationID.Value);
            }

            // Sorting logic
            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "ProbetionPeriodSettingID" => sortOrder == "desc" ? query.OrderByDescending(x => x.ProbetionPeriodSettingID) : query.OrderBy(x => x.ProbetionPeriodSettingID),
                    "OrganizationID" => sortOrder == "desc" ? query.OrderByDescending(x => x.OrganizationID) : query.OrderBy(x => x.OrganizationID),
                    "Period" => sortOrder == "desc" ? query.OrderByDescending(x => x.Period) : query.OrderBy(x => x.Period),
                    "PeriodType" => sortOrder == "desc" ? query.OrderByDescending(x => x.PeriodType) : query.OrderBy(x => x.PeriodType),
                    // Add more cases for other fields as needed
                    _ => query.OrderBy(x => x.OrganizationID)
                };
            }

            // Use pagination service with projection
            var result = await PaginationService<ProbetionPeriodSettings, ProbationSettingVM>.GetPaginatedData(
                query,
                pageNumber,
                pageSize,
                searchTerm,
                sortColumn,
                sortOrder,
                term => x => EF.Functions.Like(x.Organization.OrganizationName, $"%{term}%") ||
                             EF.Functions.Like(x.Period.ToString(), $"%{term}%") ||
                             EF.Functions.Like(x.PeriodType, $"%{term}%"),
                x => new ProbationSettingVM
                {
                    ProbetionPeriodSettingID = x.ProbetionPeriodSettingID,
                    OrganizationID = x.OrganizationID,
                    OrganizationName = x.Organization?.OrganizationName ?? "_",
                    Period = x.Period,
                    PeriodType = x.PeriodType,
                    CreatedBy = x.CreatedBy,
                    UpdatedBy = x.UpdatedBy,
                    // Include additional fields if necessary
                });

            return result;
        }
        #endregion

        #region IsNameUniqueAsync
        public async Task<bool> IsNameUniqueAsync(string name)
        {
            var existingNames = await _genericRepository.FindAsync(b => b.DeletedAt == null && b.Organization.OrganizationName != null);

            var nameList = existingNames.Select(b => b.Organization.OrganizationName);

            return !DuplicateChecker.IsDuplicate(name, nameList);
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


    }
}
