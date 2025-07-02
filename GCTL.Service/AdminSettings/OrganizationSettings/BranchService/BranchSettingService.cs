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

namespace GCTL.Service.AdminSettings.OrganizationSettings.BranchService
{
    public class BranchSettingService:AppService<OrganizationBranches>, IBranchSettingService
    {
        #region Repositories & Services
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<OrganizationBranches> _genericRepository;
        private readonly IGenericRepository<Organization> _genericRepositoryOraganization;

        public BranchSettingService(IUserInfoService userInfoService, IGenericRepository<OrganizationBranches> genericRepository, IGenericRepository<Organization> genericRepositoryOraganization) : base(genericRepository)
        {
            _userInfoService = userInfoService;
            _genericRepository = genericRepository;
            _genericRepositoryOraganization = genericRepositoryOraganization;
        }
        #endregion

        #region AddAsync  
        public async Task<bool> AddAsync(BranchSettingsVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                // Try to find an existing soft-deleted record  
                var existingEntityList = await _genericRepository.FindAsync(e =>
                    e.DeletedAt != null &&
                    e.OrganizationBranchID == model.OrganizationBranchID
                );

                var existingEntity = existingEntityList.FirstOrDefault();

                if (existingEntity != null)
                {
                    // Update and restore
                    existingEntity.OrganizationID = model.OrganizationID;
                    existingEntity.OrganizationBranchName = model.OrganizationBranchName;
                    existingEntity.EmailAddress = model.EmailAddress;
                    existingEntity.Phone = model.Phone;
                    existingEntity.WebAddress = model.WebAddress;
                    existingEntity.Fax = model.Fax;
                    existingEntity.Address = model.Address;
                    existingEntity.CountryID = model.CountryID;
                    existingEntity.Street = model.Street;
                    existingEntity.City = model.City;
                    existingEntity.PostCode = model.PostCode;


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
                    var newEntity = new OrganizationBranches
                    {
                        OrganizationID = model.OrganizationID,
                        OrganizationBranchName = model.OrganizationBranchName,
                        EmailAddress = model.EmailAddress,
                        Phone = model.Phone,
                        WebAddress = model.WebAddress,
                        Fax = model.Fax,
                      
                        Address = model.Address,
                        CountryID = model.CountryID,
                        Street = model.Street,
                        City = model.City,
                        PostCode = model.PostCode,


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
        public async Task<bool> UpdateAsync(BranchSettingsVM model)
        {
            await _genericRepository.BeginTransactionAsync();

            try
            {
                // Find existing record (including soft-deleted ones)
                var existingList = await _genericRepository.FindAsync(e =>
                    (e.DeletedAt == null || e.DeletedAt != null) &&
                    e.OrganizationBranchID == model.OrganizationBranchID // Assuming you have a primary key in the model
                );

                var entity = existingList.FirstOrDefault();

                if (entity == null)
                {
                    throw new Exception(" Organization not found.");
                }

                // Update properties
                entity.OrganizationID = model.OrganizationID;
                entity.OrganizationBranchName = model.OrganizationBranchName;
                entity.EmailAddress = model.EmailAddress;
                entity.Phone = model.Phone;
                entity.WebAddress = model.WebAddress;
                entity.Fax = model.Fax;
                
                entity.Address = model.Address;
                entity.CountryID = model.CountryID;
                entity.Street = model.Street;
                entity.City = model.City;
                entity.PostCode = model.PostCode;



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
                throw new Exception("Failed to update Organization: " + ex.Message, ex);
            }
        }

        #endregion


        #region Get
        public async Task<BranchSettingsVM> GetByIdAsync(int id)
        {
            // Retrieve the EmailSettings entity by ID, excluding soft-deleted records
            var entityList = await _genericRepository.FindAsync(x => x.OrganizationBranchID == id && x.DeletedAt == null);
            var entity = entityList.FirstOrDefault();

            if (entity == null)
                return null;

            // Map to ViewModel
            var model = new BranchSettingsVM
            {
                OrganizationID = entity.OrganizationID,
                OrganizationBranchName = entity.OrganizationBranchName,
                EmailAddress = entity.EmailAddress,
                Phone = entity.Phone,
                WebAddress = entity.WebAddress,
                Fax = entity.Fax,
               
                Address = entity.Address,
                CountryID = entity.CountryID,
                Street = entity.Street,
                City = entity.City,
                PostCode = entity.PostCode,


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
            var existingNames = await _genericRepository.FindAsync(b => b.DeletedAt == null && b.OrganizationBranchName != null);

            var nameList = existingNames.Select(b => b.OrganizationBranchName);

            return !DuplicateChecker.IsDuplicate(name, nameList);
        }
        #endregion


        #region Soft Delete
        public async Task<BranchSettingsVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.OrganizationBranchID));
                if (data == null || data.Count == 0)
                {
                    return new BranchSettingsVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<BranchSettingsVM>>(JsonConvert.SerializeObject(data));
                var targetIds = data.Select(x => (int?)x.OrganizationBranchID).ToList();

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

                return new BranchSettingsVM
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

        public async Task<PaginationService<OrganizationBranches, BranchSettingsVM>.PaginationResult<BranchSettingsVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "OrganizationID", string sortOrder = "desc", int? organizationID = null)
        {
            var query = _genericRepository.All()
                .AsNoTracking()
                .Include(x => x.Organization) // Include related Organization entity
                .Where(x => x.DeletedAt == null);

            // Filter by organization if provided
            if (organizationID.HasValue && organizationID.Value > 0)
            {
                query = query.Where(x => x.OrganizationID == organizationID.Value);
            }

            // Sorting logic
            //if (!string.IsNullOrEmpty(sortColumn))
            //{
            //    query = sortColumn switch
            //    {
            //        "EmailSettingID" => sortOrder == "desc" ? query.OrderByDescending(x => x.OrganizationID) : query.OrderBy(x => x.OrganizationID),
            //        "OrganizationName" => sortOrder == "desc" ? query.OrderByDescending(x => x.OrganizationName) : query.OrderBy(x => x.OrganizationName),
            //        "ServerName" => sortOrder == "desc" ? query.OrderByDescending(x => x.ServerName) : query.OrderBy(x => x.ServerName),
            //        "PortNumber" => sortOrder == "desc" ? query.OrderByDescending(x => x.PortNumber) : query.OrderBy(x => x.PortNumber),
            //        "PriorityIndex" => sortOrder == "desc" ? query.OrderByDescending(x => x.PriorityIndex) : query.OrderBy(x => x.PriorityIndex),
            //        "IsActive" => sortOrder == "desc" ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
            //        _ => query.OrderBy(x => x.OrganizationID)
            //    };
            //}

            // Use pagination service with projection
            var result = await PaginationService<OrganizationBranches, BranchSettingsVM>.GetPaginatedData(
                query,
                pageNumber,
                pageSize,
                searchTerm,
                sortColumn,
                sortOrder,
                term => x => EF.Functions.Like(x.OrganizationBranchName, $"%{term}%") || EF.Functions.Like(x.City, $"%{term}%"),
                x => new BranchSettingsVM
                {
                    OrganizationID = x.OrganizationID,
                    OrganizationBranchName = x.OrganizationBranchName,
                    EmailAddress = x.EmailAddress,
                    Phone = x.Phone,
                    WebAddress = x.WebAddress,
                    Fax = x.Fax,
                   
                    Address = x.Address,
                    Street = x.Street,
                    City = x.City,
                    PostCode = x.PostCode,

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
