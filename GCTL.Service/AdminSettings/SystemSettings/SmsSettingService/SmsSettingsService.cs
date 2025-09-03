using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.AdminSettings.SystemSettings.ISmsSettingService;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AdminSettings.SystemSettings.SmsSettingService
{
    public class SmsSettingsService : AppService<SMSSettings>,ISmsSettingsService
    {
        #region Repositories & Services
        // Here you would typically inject your repositories or services needed for data access.
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<SMSSettings> _genericRepository;
        private readonly IGenericRepository<Organization> _organizationRepository;

        public SmsSettingsService(IUserInfoService userInfoService, IGenericRepository<SMSSettings> genericRepository, IGenericRepository<Organization> organizationRepository) : base(genericRepository)
        {
            _userInfoService = userInfoService;
            _genericRepository = genericRepository;
            _organizationRepository = organizationRepository;
        }
        #endregion
        #region AddAsync  
        public async Task<bool> AddAsync(SmsSettingsVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                // Try to find an existing soft-deleted record  
                var existingEntityList = await _genericRepository.FindAsync(e =>
                    e.DeletedAt != null &&
                    e.ServerName == model.ServerName &&
                    e.OrganizationID == model.OrganizationID
                );

                var existingEntity = existingEntityList.FirstOrDefault();

                if (existingEntity != null)
                {
                    // Update and restore  
                    existingEntity.ServerName = model.ServerName;
                    existingEntity.Gateway = model.Gateway;
                    existingEntity.API = model.API;
                    existingEntity.MobileNumber = model.MobileNumber;
                    existingEntity.PriorityIndex = model.PriorityIndex;
                    existingEntity.UserName = model.UserName;
                    existingEntity.Password = model.Password;
                    existingEntity.IsActive = model.IsActive;

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
                    var newEntity = new SMSSettings
                    {
                        OrganizationID = model.OrganizationID,
                        ServerName = model.ServerName,
                        Gateway = model.Gateway,
                        API = model.API,
                        MobileNumber = model.MobileNumber,
                        PriorityIndex = model.PriorityIndex,
                        UserName = model.UserName,
                        Password = model.Password,
                        IsActive = model.IsActive,
                        CreatedAt = DateTime.Now,
                        CreatedBy = model.CreatedBy,
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
        public async Task<bool> UpdateAsync(SmsSettingsVM model)
        {
            await _genericRepository.BeginTransactionAsync();

            try
            {
                // Find existing record (including soft-deleted ones)
                var existingList = await _genericRepository.FindAsync(e =>
                    (e.DeletedAt == null || e.DeletedAt != null) &&
                    e.SMSSettingID == model.SMSSettingID // Assuming you have a primary key in the model
                );

                var entity = existingList.FirstOrDefault();

                if (entity == null)
                {
                    throw new Exception("Email settings not found.");
                }

                // Update properties
                entity.OrganizationID = model.OrganizationID;
                entity.ServerName = model.ServerName;
                entity.Gateway = model.Gateway;
                entity.API = model.API;
                entity.MobileNumber = model.MobileNumber;
                entity.PriorityIndex = model.PriorityIndex;
                entity.UserName = model.UserName;
                entity.Password = model.Password;
                entity.IsActive = model.IsActive;

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
        public async Task<SmsSettingsVM> GetByIdAsync(int id)
        {
            // Retrieve the EmailSettings entity by ID, excluding soft-deleted records
            var entityList = await _genericRepository.FindAsync(x => x.SMSSettingID == id && x.DeletedAt == null);
            var entity = entityList.FirstOrDefault();

            if (entity == null)
                return null;

            // Map to ViewModel
            var model = new SmsSettingsVM
            {
                SMSSettingID = entity.SMSSettingID,
                OrganizationID = entity.OrganizationID,
                ServerName = entity.ServerName,
                Gateway = entity.Gateway,
                API = entity.API,
                MobileNumber = entity.MobileNumber,
                PriorityIndex = entity.PriorityIndex,
                UserName = entity.UserName,
                Password = entity.Password,
                IsActive = entity.IsActive,
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
            var existingNames = await _genericRepository.FindAsync(b => b.DeletedAt == null && b.ServerName != null);

            var nameList = existingNames.Select(b => b.ServerName);

            return !DuplicateChecker.IsDuplicate(name, nameList);
        }
        #endregion


        #region Soft Delete
        public async Task<SmsSettingsVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.SMSSettingID));
                if (data == null || data.Count == 0)
                {
                    return new SmsSettingsVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<SmsSettingsVM>>(JsonConvert.SerializeObject(data));
                var targetIds = data.Select(x => (int?)x.SMSSettingID).ToList();

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

                return new SmsSettingsVM
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

        public async Task<PaginationService<SMSSettings, SmsSettingsVM>.PaginationResult<SmsSettingsVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "EmailSettingID", string sortOrder = "desc", int? organizationID = null)
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
                    "EmailSettingID" => sortOrder == "desc" ? query.OrderByDescending(x => x.SMSSettingID) : query.OrderBy(x => x.SMSSettingID),
                    "OrganizationName" => sortOrder == "desc" ? query.OrderByDescending(x => x.Organization.OrganizationName) : query.OrderBy(x => x.Organization.OrganizationName),
                    "ServerName" => sortOrder == "desc" ? query.OrderByDescending(x => x.ServerName) : query.OrderBy(x => x.ServerName),
                    "Gateway" => sortOrder == "desc" ? query.OrderByDescending(x => x.Gateway) : query.OrderBy(x => x.Gateway),
                    "PriorityIndex" => sortOrder == "desc" ? query.OrderByDescending(x => x.PriorityIndex) : query.OrderBy(x => x.PriorityIndex),
                    "IsActive" => sortOrder == "desc" ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                    _ => query.OrderBy(x => x.SMSSettingID)
                };
            }

            // Use pagination service with projection
            var result = await PaginationService<SMSSettings, SmsSettingsVM>.GetPaginatedData(
                query,
                pageNumber,
                pageSize,
                searchTerm,
                sortColumn,
                sortOrder,
                term => x => EF.Functions.Like(x.Organization.OrganizationName, $"%{term}%") || EF.Functions.Like(x.ServerName, $"%{term}%"),
                x => new SmsSettingsVM
                {
                    SMSSettingID = x.SMSSettingID,
                    OrganizationID = x.OrganizationID,
                    OrganizationName = x.Organization != null ? x.Organization.OrganizationName ?? "-" : "-",
                    ServerName = x.ServerName ?? "-",
                    Gateway = x.Gateway,
                    API = x.API,
                    MobileNumber = x.MobileNumber,
                    PriorityIndex = x.PriorityIndex,
                    UserName = x.UserName ?? "-",
                    Password = x.Password ?? "-",
                    IsActive = x.IsActive,
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
