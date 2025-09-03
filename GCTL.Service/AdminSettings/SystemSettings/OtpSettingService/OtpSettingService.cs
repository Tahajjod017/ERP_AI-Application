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

namespace GCTL.Service.AdminSettings.SystemSettings.OtpSettingService
{
    public class OtpSettingService :AppService<OTPSettings> , IOtpSettingService
    {
        #region
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<OTPSettings> _genericRepository;
        private readonly IGenericRepository<Organization> _organizationRepository;

        public OtpSettingService(IUserInfoService userInfoService, IGenericRepository<OTPSettings> genericRepository, IGenericRepository<Organization> organizationRepository) : base(genericRepository)    
        {
            _userInfoService = userInfoService;
            _genericRepository = genericRepository;
            _organizationRepository = organizationRepository;
        }
        #endregion

        #region AddAsync  
        public async Task<bool> AddAsync(OtpSettingsVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                // Try to find an existing soft-deleted record  
                var existingEntityList = await _genericRepository.FindAsync(e =>
                    e.DeletedAt != null &&
                    e.OTPType == model.OTPType
                   
                );

                var existingEntity = existingEntityList.FirstOrDefault();

                if (existingEntity != null)
                {
                    // Update and restore  
                    existingEntity.OTPType = model.OTPType;
                    existingEntity.OTPDigitLimit = model.OTPDigitLimit;
                    existingEntity.OTPExpireInMin = model.OTPExpireInMin;
                   
                    existingEntity.CreatedAt = DateTime.Now;
                    existingEntity.CreatedBy = model.CreatedBy; // You can replace this with current user ID  
                    existingEntity.LIP = model.LIP;
                    existingEntity.LMAC = model.LMAC;

                    existingEntity.UpdatedAt = DateTime.Now;
                    existingEntity.UpdatedBy = model.UpdatedBy ?? null;
                    existingEntity.DeletedAt = null;

                    await _genericRepository.UpdateAsync(existingEntity);
                }
                else
                {
                    // New Insert  
                    var newEntity = new OTPSettings
                    {
                        OrganizationID = model.OrganizationID,
                        OTPType = model.OTPType,
                        OTPDigitLimit = model.OTPDigitLimit,
                        OTPExpireInMin = model.OTPExpireInMin,
                        CreatedAt = DateTime.Now,
                        CreatedBy = model.CreatedBy, // You can replace this with current user ID  
                        LIP = model.LIP,
                        LMAC = model.LMAC,
                    };
                   

                    await _genericRepository.AddAsync(newEntity);
                }

                await _genericRepository.CommitTransactionAsync();
                return true; // Ensure a return value is provided here  
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                throw new Exception(ex.Message, ex);
                //return false; // Or handle the exception as needed  
            }
        }

        #endregion


        #region Update
        public async Task<bool> UpdateAsync(OtpSettingsVM model)
        {
            await _genericRepository.BeginTransactionAsync();

            try
            {
                // Find existing record (including soft-deleted ones)
                var existingList = await _genericRepository.FindAsync(e =>
                    (e.DeletedAt == null || e.DeletedAt != null) &&
                    e.OTPSettingID == model.OTPSettingID // Assuming you have a primary key in the model
                );

                var entity = existingList.FirstOrDefault();

                if (entity == null)
                {
                    throw new Exception("Email settings not found.");
                }

                // Update properties
                entity.OrganizationID = model.OrganizationID;
                entity.OTPType = model.OTPType;
                entity.OTPDigitLimit = model.OTPDigitLimit;
                entity.OTPExpireInMin = model.OTPExpireInMin;

                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = model.UpdatedBy ?? 0; // default or current user
                entity.DeletedAt = null; // Restore if previously deleted
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;

                await _genericRepository.UpdateAsync(entity);
                await _genericRepository.CommitTransactionAsync();

                return true;
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                // You can log the error here
                throw new Exception("Failed to update email settings: " + ex.Message, ex);
            }
        }

        #endregion


        #region Get
        public async Task<OtpSettingsVM> GetByIdAsync(int id)
        {
            // Retrieve the EmailSettings entity by ID, excluding soft-deleted records
            var entityList = await _genericRepository.FindAsync(x => x.OTPSettingID == id && x.DeletedAt == null);
            var entity = entityList.FirstOrDefault();

            if (entity == null)
                return null;

            // Map to ViewModel
            var model = new OtpSettingsVM
            {
                OTPSettingID = entity.OTPSettingID,
                OrganizationID = entity.OrganizationID,
                OTPType = entity.OTPType,
                OTPDigitLimit = entity.OTPDigitLimit,
                OTPExpireInMin = entity.OTPExpireInMin,

                //CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                //UpdatedAt = entity.UpdatedAt,
                UpdatedBy = entity.UpdatedBy,
                LIP = entity.LIP,
                LMAC = entity.LMAC
            };

            return model;
        }

        #endregion


        #region IsNameUniqueAsync
        public async Task<bool> IsNameUniqueAsync(string name)
        {
            var existingNames = await _genericRepository.FindAsync(b => b.DeletedAt == null && b.OTPType != null);

            var nameList = existingNames.Select(b => b.OTPType);

            return !DuplicateChecker.IsDuplicate(name, nameList);
        }
        #endregion


        #region Soft Delete
        public async Task<OtpSettingsVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.OTPSettingID));
                if (data == null || data.Count == 0)
                {
                    return new OtpSettingsVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<OtpSettingsVM>>(JsonConvert.SerializeObject(data));
                var targetIds = data.Select(x => (int?)x.OTPSettingID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.DeletedBy = requestVM.DeletedBy;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                }

                await _genericRepository.UpdateRangeAsync(data);

                await _userInfoService.ActionLogDeleteAsync("Blood Group", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);

                await _genericRepository.CommitTransactionAsync();

                return new OtpSettingsVM
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

        public async Task<PaginationService<OTPSettings, OtpSettingsVM>.PaginationResult<OtpSettingsVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "OTPSettingID", string sortOrder = "desc", int? organizationID = null)
        {
            var query = _genericRepository.All()
                .AsNoTracking()
                .Include(x => x.Organization)
                .Where(x => x.DeletedAt == null);

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
                    "OTPSettingID" => sortOrder == "desc" ? query.OrderByDescending(x => x.OTPSettingID) : query.OrderBy(x => x.OTPSettingID),
                    "OrganizationName" => sortOrder == "desc" ? query.OrderByDescending(x => x.Organization.OrganizationName) : query.OrderBy(x => x.Organization.OrganizationName),
                    "OTPType" => sortOrder == "desc" ? query.OrderByDescending(x => x.OTPType) : query.OrderBy(x => x.OTPType),
                    "OTPDigitLimit" => sortOrder == "desc" ? query.OrderByDescending(x => x.OTPDigitLimit) : query.OrderBy(x => x.OTPDigitLimit),
                    "OTPExpireInMin" => sortOrder == "desc" ? query.OrderByDescending(x => x.OTPExpireInMin) : query.OrderBy(x => x.OTPExpireInMin),
                   
                    _ => query.OrderBy(x => x.OTPSettingID)
                };
            }

            // Use pagination service with projection
            var result = await PaginationService<OTPSettings, OtpSettingsVM>.GetPaginatedData(
                query,
                pageNumber,
                pageSize,
                searchTerm,
                sortColumn,
                sortOrder,
                term => x => EF.Functions.Like(x.Organization.OrganizationName, $"%{term}%") || EF.Functions.Like(x.OTPType, $"%{term}%"),
                x => new OtpSettingsVM
                {
                    OTPSettingID = x.OTPSettingID,
                    OrganizationID = x.OrganizationID,
                    OrganizationName = x.Organization != null ? x.Organization.OrganizationName ?? "-" : "-",
                    OTPType = x.OTPType,
                    OTPDigitLimit = x.OTPDigitLimit,
                    OTPExpireInMin = x.OTPExpireInMin,

                    // CreatedAt = x.CreatedAt,
                    CreatedBy = x.CreatedBy,
                    //UpdatedAt = x.UpdatedAt,
                    UpdatedBy = x.UpdatedBy,
                    LIP = x.LIP,
                    LMAC = x.LMAC
                });

            return result;
        }
        #endregion
    }
}
