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

namespace GCTL.Service.AdminSettings.OrganizationSettings.CompanyService
{
    public class CompanySettingService :AppService<Organization>, ICompanySettingService
    {
        #region Repositories & Services
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<Organization> _genericRepository;
        private readonly IGenericRepository<TenantInfo> _tenantInfoRepository;
        private readonly IGenericRepository<Country> _genericRepositoryCountry;

        public CompanySettingService(IUserInfoService userInfoService, IGenericRepository<Organization> genericRepository, IGenericRepository<TenantInfo> tenantInfoRepository, IGenericRepository<Country> genericRepositoryCountry) : base(genericRepository)
        {
            _userInfoService = userInfoService;
            _genericRepository = genericRepository;
            _tenantInfoRepository = tenantInfoRepository;
            _genericRepositoryCountry = genericRepositoryCountry;
        }
        #endregion

        #region AddAsync  
        public async Task<bool> AddAsync(CompanySettingsVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                // Try to find an existing soft-deleted record  
                var existingEntityList = await _genericRepository.FindAsync(e =>
                    e.DeletedAt != null &&
                    e.OrganizationID == model.OrganizationID
                );

                var existingEntity = existingEntityList.FirstOrDefault();

                if (existingEntity != null)
                {
                    // Update and restore  
                    existingEntity.OrganizationName = model.OrganizationName;
                    existingEntity.EmailAddress = model.EmailAddress;
                    existingEntity.Phone = model.Phone;
                    existingEntity.WebAddress = model.WebAddress;
                    existingEntity.Fax = model.Fax;
                    existingEntity.LogoLink = model.LogoLink;
                    existingEntity.FaviconLink = model.FaviconLink;
                    existingEntity.Address = model.Address;
                    existingEntity.CountryID = model.CountryID;
                    existingEntity.Street = model.Street;
                    existingEntity.City = model.City;
                    existingEntity.PostCode = model.PostCode;

                    // Fix for CS0029: Convert Latitude and Longitude to decimal?  
                    existingEntity.Latitude = string.IsNullOrEmpty(model.Latitude) ? null : decimal.Parse(model.Latitude);
                    existingEntity.Longitude = string.IsNullOrEmpty(model.Longitude) ? null : decimal.Parse(model.Longitude);

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
                    var newEntity = new Organization
                    {
                        OrganizationID = model.OrganizationID,
                        OrganizationName = model.OrganizationName,
                        EmailAddress = model.EmailAddress,
                        Phone = model.Phone,
                        WebAddress = model.WebAddress,
                        Fax = model.Fax,
                        LogoLink = model.LogoLink,
                        FaviconLink = model.FaviconLink,
                        Address = model.Address,
                        CountryID = model.CountryID,
                        Street = model.Street,
                        City = model.City,
                        PostCode = model.PostCode,

                        // Fix for CS0029: Convert Latitude and Longitude to decimal?  
                        Latitude = string.IsNullOrEmpty(model.Latitude) ? null : decimal.Parse(model.Latitude),
                        Longitude = string.IsNullOrEmpty(model.Longitude) ? null : decimal.Parse(model.Longitude),

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
        public async Task<bool> UpdateAsync(CompanySettingsVM model)
        {
            await _genericRepository.BeginTransactionAsync();

            try
            {
                // Find existing record (including soft-deleted ones)
                var existingList = await _genericRepository.FindAsync(e =>
                    (e.DeletedAt == null || e.DeletedAt != null) &&
                    e.OrganizationID == model.OrganizationID // Assuming you have a primary key in the model
                );

                var entity = existingList.FirstOrDefault();

                if (entity == null)
                {
                    throw new Exception(" Organization not found.");
                }

                // Update properties
                entity.OrganizationID = model.OrganizationID;
                entity.OrganizationName = model.OrganizationName;
                entity.EmailAddress = model.EmailAddress;
                entity.Phone = model.Phone;
                entity.WebAddress = model.WebAddress;
                entity.Fax = model.Fax;
                entity.LogoLink = model.LogoLink;
                entity.FaviconLink = model.FaviconLink;
                entity.Address = model.Address;
                entity.CountryID = model.CountryID;
                entity.Street = model.Street;
                entity.City = model.City;
                entity.PostCode = model.PostCode;
                // Convert Latitude and Longitude to decimal? if they are not null or empty
                entity.Latitude = string.IsNullOrEmpty(model.Latitude) ? null : decimal.Parse(model.Latitude);
                entity.Longitude = string.IsNullOrEmpty(model.Longitude) ? null : decimal.Parse(model.Longitude);



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
        public async Task<CompanySettingsVM> GetByIdAsync(int id)
        {
            // Retrieve the EmailSettings entity by ID, excluding soft-deleted records
            var entityList = await _genericRepository.FindAsync(x => x.OrganizationID == id && x.DeletedAt == null);
            var entity = entityList.FirstOrDefault();

            if (entity == null)
                return null;

            // Map to ViewModel
            var model = new CompanySettingsVM
            {
                OrganizationID = entity.OrganizationID,
                OrganizationName = entity.OrganizationName,
                EmailAddress = entity.EmailAddress,
                Phone = entity.Phone,
                WebAddress = entity.WebAddress,
                Fax = entity.Fax,
                LogoLink = entity.LogoLink,
                FaviconLink = entity.FaviconLink,
                Address = entity.Address,
                CountryID = entity.CountryID,
                Street = entity.Street,
                City = entity.City,
                PostCode = entity.PostCode,
                Latitude = entity.Latitude?.ToString(),
                Longitude = entity.Longitude?.ToString(),


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
            var existingNames = await _genericRepository.FindAsync(b => b.DeletedAt == null && b.OrganizationName != null);

            var nameList = existingNames.Select(b => b.OrganizationName);

            return !DuplicateChecker.IsDuplicate(name, nameList);
        }
        #endregion


        #region Soft Delete
        public async Task<CompanySettingsVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.OrganizationID));
                if (data == null || data.Count == 0)
                {
                    return new CompanySettingsVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<CompanySettingsVM>>(JsonConvert.SerializeObject(data));
                var targetIds = data.Select(x => (int?)x.OrganizationID).ToList();

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

                return new CompanySettingsVM
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


        public async Task<PaginationService<Organization, CompanySettingsVM>.PaginationResult<CompanySettingsVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "OrganizationID", string sortOrder = "desc", int? organizationID = null)
        {
            var query = _genericRepository.All()
                .AsNoTracking()
                .Include(x => x.Country)
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
            var result = await PaginationService<Organization, CompanySettingsVM>.GetPaginatedData(
                query,
                pageNumber,
                pageSize,
                searchTerm,
                sortColumn,
                sortOrder,
                term => x => EF.Functions.Like(x.OrganizationName, $"%{term}%") ||
                EF.Functions.Like(x.EmailAddress, $"%{term}%") ||
                EF.Functions.Like(x.Phone, $"%{term}%") ||
                //EF.Functions.Like(x.WebAddress, $"%{term}%") ||
              //  EF.Functions.Like(x.Fax, $"%{term}%") ||
                EF.Functions.Like(x.Address, $"%{term}%") ||
                EF.Functions.Like(x.Country.CountryName, $"%{term}%") ,
                //  EF.Functions.Like(x.Street, $"%{term}%") ||
                //  EF.Functions.Like(x.City, $"%{term}%"),
                x => new CompanySettingsVM
                {
                    OrganizationID = x.OrganizationID,
                    OrganizationName = x.OrganizationName,
                    EmailAddress = x.EmailAddress ?? "_",
                    Phone = x.Phone ?? "_",
                    WebAddress = x.WebAddress ?? "_",
                    Fax = x.Fax ?? "_",
                    LogoLink = x.LogoLink ?? "_",
                    FaviconLink = x.FaviconLink ?? "_",
                    Address = x.Address ?? "_",
                    Street = x.Street ?? "_",
                    City = x.City ?? "_",
                    PostCode = x.PostCode ?? "_",
                    CountryID = x.CountryID,
                    CountryName = x.Country?.CountryName ?? "_",

                    // CreatedAt = x.CreatedAt,
                    CreatedBy = x.CreatedBy,
                    //UpdatedAt = x.UpdatedAt,
                    UpdatedBy = x.UpdatedBy,
                    LIP = x.LIP ?? "_",
                    LMAC = x.LMAC ?? "_"
                });

            return result;
        }




        #endregion

        #region GetCountryAsync
        public async Task<List<SelectListItem>> GetCountriesAsync()
        {
            var countries = await _genericRepositoryCountry.All()
                .Where(c => c.DeletedAt == null)
                .Select(c => new SelectListItem
                {
                    Value = c.CountryID.ToString(),
                    Text = c.CountryName
                })
                .ToListAsync();
            return countries;
        }
        #endregion

    }
}
