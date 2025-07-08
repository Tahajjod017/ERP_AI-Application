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

namespace GCTL.Service.AdminSettings.OrganizationSettings.ApprovalService
{
    public class ApprovalSettingService : AppService<ApprovalSettings>, IApprovalSettingService
    {
        #region Repositories & Services
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<ApprovalSettings> _genericRepository;
        private readonly IGenericRepository<Organization> _genericRepositoryOraganization;
        private readonly IGenericRepository<OrganizationBranches> _genericRepositoryBranches;
        private readonly IGenericRepository<ApprovalTypes> _genericRepositoryApprovalType;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _genericRepositoryEmployees;
        private readonly IGenericRepository<Designations> _genericRepositoryDesignations;
        private readonly IGenericRepository<Country> _genericRepositoryCountry;

        public ApprovalSettingService(IUserInfoService userInfoService, IGenericRepository<ApprovalSettings> genericRepository, IGenericRepository<Organization> genericRepositoryOraganization, IGenericRepository<OrganizationBranches> genericRepositoryBranches, IGenericRepository<ApprovalTypes> genericRepositoryApprovalType, IGenericRepository<Data.Models.Employees> genericRepositoryEmployees, IGenericRepository<Designations> genericRepositoryDesignations, IGenericRepository<Country> genericRepositoryCountry) : base(genericRepository)
        {
            _userInfoService = userInfoService;
            _genericRepository = genericRepository;
            _genericRepositoryOraganization = genericRepositoryOraganization;
            _genericRepositoryBranches = genericRepositoryBranches;
            _genericRepositoryApprovalType = genericRepositoryApprovalType;
            _genericRepositoryEmployees = genericRepositoryEmployees;
            _genericRepositoryDesignations = genericRepositoryDesignations;
            _genericRepositoryCountry = genericRepositoryCountry;
        }

        #endregion

        #region AddAsync  
        public async Task<bool> AddAsync(ApprovalSettingsVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                // Step 1:  find an existing soft-deleted ApprovalSetting
                var existingEntityList = await _genericRepository.FindAsync(e =>
                    e.DeletedAt != null &&
                    e.OrganizationID == model.OrganizationID
                // Additional conditions can be added here (e.g., OrganizationBranchID)
                );

                var existingEntity = existingEntityList.FirstOrDefault();
                ApprovalSettings setting;

                if (existingEntity != null)
                {
                    // Step 2: Restore and update soft-deleted ApprovalSetting
                    existingEntity.OrganizationID = model.OrganizationID;
                    existingEntity.OrganizationBranchID = model.OrganizationBranchID;
                    existingEntity.ApprovalTypeID = model.ApprovalTypeID;
                    existingEntity.StartDate = model.StartDate;
                    existingEntity.EndDate = model.EndDate;
                    existingEntity.LIP = model.LIP;
                    existingEntity.LMAC = model.LMAC;
                    existingEntity.FirstApprovalID = model.FirstApprovalID;
                    existingEntity.IsDesignationOrEmpFirstApprovalID = model.IsDesignationOrEmpFirstApprovalID =="on";
                    existingEntity.IsEnableSecondApproval = model.IsEnableSecondApproval =="on";
                    existingEntity.SecondApprovalID = model.SecondApprovalID;
                    existingEntity.IsDesignationOrEmpSecondApprovalID = model.IsDesignationOrEmpSecondApprovalID == "on";
                    existingEntity.IsEnableThirdApproval = model.IsEnableThirdApproval == "on";
                    existingEntity.ThirdApprovalID = model.ThirdApprovalID;
                    existingEntity.IsDesignationOrEmpThirdApprovalID = model.IsDesignationOrEmpThirdApprovalID == "on";
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
                    // Step 3: Insert new ApprovalSetting
                    setting = new ApprovalSettings
                    {
                        OrganizationID = model.OrganizationID,
                        OrganizationBranchID = model.OrganizationBranchID,
                        ApprovalTypeID = model.ApprovalTypeID,
                        StartDate = model.StartDate,
                        EndDate = model.EndDate,
                        LIP = model.LIP,
                        LMAC = model.LMAC,
                        FirstApprovalID = model.FirstApprovalID,
                        IsDesignationOrEmpFirstApprovalID = model.IsDesignationOrEmpFirstApprovalID == "on",
                        IsEnableSecondApproval = model.IsEnableSecondApproval == "on",
                        SecondApprovalID = model.SecondApprovalID,
                        IsDesignationOrEmpSecondApprovalID = model.IsDesignationOrEmpSecondApprovalID == "on",
                        IsEnableThirdApproval = model.IsEnableThirdApproval == "on",
                        ThirdApprovalID = model.ThirdApprovalID,
                        IsDesignationOrEmpThirdApprovalID = model.IsDesignationOrEmpThirdApprovalID == "on",
                        CreatedAt = DateTime.Now,
                        CreatedBy = model.CreatedBy
                        // Optional: Additional fields can be added here
                    };

                    await _genericRepository.AddAsync(setting);
                }

               

                // Step 6: Commit Transaction
                await _genericRepository.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                throw new Exception("Failed to add approval setting", ex);
            }
        }

        #endregion

        #region Update
        public async Task<bool> UpdateAsync(ApprovalSettingsVM model)
        {

            await _genericRepository.BeginTransactionAsync();

            try
            {
                // Step 1: Find existing ApprovalSetting record (including soft-deleted ones)
                var existingEntityList = await _genericRepository.FindAsync(e =>
                    (e.DeletedAt == null || e.DeletedAt != null) &&
                    e.ApprovalSettingID == model.ApprovalSettingID //  a primary key in the model
                );

                var existingEntity = existingEntityList.FirstOrDefault();

                if (existingEntity == null)
                {
                    throw new Exception("Approval Setting not found.");
                }

                // Step 2: Update properties
                existingEntity.OrganizationID = model.OrganizationID;
                existingEntity.OrganizationBranchID = model.OrganizationBranchID;
                existingEntity.ApprovalTypeID = model.ApprovalTypeID;
                existingEntity.StartDate = model.StartDate;
                existingEntity.EndDate = model.EndDate;
                existingEntity.LIP = model.LIP;
                existingEntity.LMAC = model.LMAC;
                existingEntity.FirstApprovalID = model.FirstApprovalID;
                existingEntity.IsDesignationOrEmpFirstApprovalID = model.IsDesignationOrEmpFirstApprovalID == "on";
                existingEntity.IsEnableSecondApproval = model.IsEnableSecondApproval == "on";
                existingEntity.SecondApprovalID = model.SecondApprovalID;
                existingEntity.IsDesignationOrEmpSecondApprovalID = model.IsDesignationOrEmpSecondApprovalID == "on";
                existingEntity.IsEnableThirdApproval = model.IsEnableThirdApproval == "on";
                existingEntity.ThirdApprovalID = model.ThirdApprovalID;
                existingEntity.IsDesignationOrEmpThirdApprovalID = model.IsDesignationOrEmpThirdApprovalID == "on";

                // Restore soft-deleted record (if previously deleted)
                existingEntity.DeletedAt = null;
                existingEntity.DeletedBy = null;

                existingEntity.UpdatedAt = DateTime.Now;
                existingEntity.UpdatedBy = model.UpdatedBy ?? model.CreatedBy; // Default or current user

                // Optional: Restore other fields like LIP, LMAC if needed
                // existingEntity.LIP = model.LIP;
                // existingEntity.LMAC = model.LMAC;

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
                throw new Exception("Failed to update approval setting: " + ex.Message, ex);
            }
        }

        #endregion

        #region Soft Delete
        public async Task<ApprovalSettingsVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                // Step 1: Find records to soft delete
                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.ApprovalSettingID));

                if (data == null || data.Count == 0)
                {
                    return new ApprovalSettingsVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                // Step 2: Serialize before state for logging purposes
                //var beforeEntity = JsonConvert.DeserializeObject<List<ApprovalSettingsVM>>(JsonConvert.SerializeObject(data));
                var targetIds = data.Select(x => (int?)x.ApprovalSettingID).ToList();

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
               // await _userInfoService.ActionLogDeleteAsync("ApprovalSetting", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);

                // Step 6: Commit the transaction
                await _genericRepository.CommitTransactionAsync();

                return new ApprovalSettingsVM
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
        public async Task<ApprovalSettingsVM> GetByIdAsync(int id)
        {
            // Retrieve the ApprovalSetting entity by ID, excluding soft-deleted records
            var entityList = await _genericRepository.FindAsync(x => x.ApprovalSettingID == id && x.DeletedAt == null);
            var entity = entityList.FirstOrDefault();

            if (entity == null)
                return null;

            // Map to ViewModel
            var model = new ApprovalSettingsVM
            {
                ApprovalSettingID = entity.ApprovalSettingID,
                OrganizationID = entity.OrganizationID,
                OrganizationBranchID = entity.OrganizationBranchID,
                ApprovalTypeID = entity.ApprovalTypeID,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                FirstApprovalID = entity.FirstApprovalID,

                // Fix for CS0029: Cannot implicitly convert type 'bool' to 'string'  
                // The issue is that `IsDesignationOrEmpFirstApprovalID` is a `bool` in the `ApprovalSettings` entity,  
                // but the `ApprovalSettingsVM` expects it to be a `string`.  
                // To fix this, we need to convert the `bool` to a `string` representation (e.g., "on" or "off").  

                IsDesignationOrEmpFirstApprovalID = entity.IsDesignationOrEmpFirstApprovalID ? "on" : null,
                //IsDesignationOrEmpFirstApprovalID = entity.IsDesignationOrEmpFirstApprovalID,
                //IsEnableSecondApproval = entity.IsEnableSecondApproval,
                SecondApprovalID = entity.SecondApprovalID,
               // IsDesignationOrEmpSecondApprovalID = entity.IsDesignationOrEmpSecondApprovalID,
                //IsEnableThirdApproval = entity.IsEnableThirdApproval,
                ThirdApprovalID = entity.ThirdApprovalID,
               // IsDesignationOrEmpThirdApprovalID = entity.IsDesignationOrEmpThirdApprovalID,
                CreatedBy = entity.CreatedBy,
                UpdatedBy = entity.UpdatedBy
                // You can add additional properties (LIP, LMAC) if required
            };

            return model;
        }
        #endregion


        #region GetAll
        public async Task<PaginationService<ApprovalSettings, ApprovalSettingsVM>.PaginationResult<ApprovalSettingsVM>> GetAllAsync(
            int pageNumber = 1,
            int pageSize = 5,
            string searchTerm = "",
            string sortColumn = "OrganizationID",
            string sortOrder = "desc",
            int? organizationID = null)
        {
            var query = _genericRepository.All()
                .AsNoTracking()
                .Include(x => x.Organization) // Include  Organization entity
                .Include(x => x.ApprovalType) // Include  ApprovalType entity
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
                    "ApprovalSettingID" => sortOrder == "desc" ? query.OrderByDescending(x => x.ApprovalSettingID) : query.OrderBy(x => x.ApprovalSettingID),
                    "OrganizationID" => sortOrder == "desc" ? query.OrderByDescending(x => x.OrganizationID) : query.OrderBy(x => x.OrganizationID),
                    "OrganizationBranchID" => sortOrder == "desc" ? query.OrderByDescending(x => x.OrganizationBranchID) : query.OrderBy(x => x.OrganizationBranchID),
                    "ApprovalTypeID" => sortOrder == "desc" ? query.OrderByDescending(x => x.ApprovalTypeID) : query.OrderBy(x => x.ApprovalTypeID),
                    "FirstApprovalID" => sortOrder == "desc" ? query.OrderByDescending(x => x.FirstApprovalID) : query.OrderBy(x => x.FirstApprovalID),
                    "IsDesignationOrEmpFirstApprovalID" => sortOrder == "desc" ? query.OrderByDescending(x => x.IsDesignationOrEmpFirstApprovalID) : query.OrderBy(x => x.IsDesignationOrEmpFirstApprovalID),
                    "IsEnableSecondApproval" => sortOrder == "desc" ? query.OrderByDescending(x => x.IsEnableSecondApproval) : query.OrderBy(x => x.IsEnableSecondApproval),
                    "SecondApprovalID" => sortOrder == "desc" ? query.OrderByDescending(x => x.SecondApprovalID) : query.OrderBy(x => x.SecondApprovalID),
                    "IsDesignationOrEmpSecondApprovalID" => sortOrder == "desc" ? query.OrderByDescending(x => x.IsDesignationOrEmpSecondApprovalID) : query.OrderBy(x => x.IsDesignationOrEmpSecondApprovalID),
                    "IsEnableThirdApproval" => sortOrder == "desc" ? query.OrderByDescending(x => x.IsEnableThirdApproval) : query.OrderBy(x => x.IsEnableThirdApproval),
                    "ThirdApprovalID" => sortOrder == "desc" ? query.OrderByDescending(x => x.ThirdApprovalID) : query.OrderBy(x => x.ThirdApprovalID),
                    "IsDesignationOrEmpThirdApprovalID" => sortOrder == "desc" ? query.OrderByDescending(x => x.IsDesignationOrEmpThirdApprovalID) : query.OrderBy(x => x.IsDesignationOrEmpThirdApprovalID),


                    // Add more cases for other fields as needed
                    _ => query.OrderBy(x => x.OrganizationID)
                };
            }

            // Use pagination service with projection
            var result = await PaginationService<ApprovalSettings, ApprovalSettingsVM>.GetPaginatedData(
                query,
                pageNumber,
                pageSize,
                searchTerm,
                sortColumn,
                sortOrder,
                term => x => EF.Functions.Like(x.Organization.OrganizationName, $"%{term}%") ||
                             EF.Functions.Like(x.FirstApprovalID.ToString(), $"%{term}%") ||
                             EF.Functions.Like(x.SecondApprovalID.ToString(), $"%{term}%"),
                x => new ApprovalSettingsVM
                {
                    ApprovalSettingID = x.ApprovalSettingID,
                    OrganizationID = x.OrganizationID,
                    OrganizationBranchID = x.OrganizationBranchID,
                    OrganizationName = x.Organization?.OrganizationName ?? "_",
                    ApprovalTypeID = x.ApprovalTypeID,
                    ApprovalTypeName = x.ApprovalType.ApprovalTypeName ?? "_",
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    FirstApprovalID = x.FirstApprovalID,
                    //IsDesignationOrEmpFirstApprovalID = x.IsDesignationOrEmpFirstApprovalID,
                    // Conditional logic for FirstApprovalName based on IsDesignationOrEmpFirstApprovalID
                    FirstApprovalName = x.FirstApprovalID.HasValue
                        ? (x.IsDesignationOrEmpFirstApprovalID
                            ? _genericRepositoryEmployees.All().FirstOrDefault(e => e.EmployeeID == x.FirstApprovalID)?.FirstName + " " + _genericRepositoryEmployees.All().FirstOrDefault(e => e.EmployeeID == x.FirstApprovalID)?.LastName
                            : _genericRepositoryDesignations.All().FirstOrDefault(d => d.DesignationID == x.FirstApprovalID)?.DesignationName ?? "_")
                        : "_",
                    //IsEnableSecondApproval = x.IsEnableSecondApproval,
                    SecondApprovalID = x.SecondApprovalID,
                    SecondApprovalName = x.SecondApprovalID.HasValue
                         ? (x.IsDesignationOrEmpSecondApprovalID
                             ? _genericRepositoryEmployees.All().FirstOrDefault(e => e.EmployeeID == x.SecondApprovalID)?.FirstName + " " + _genericRepositoryEmployees.All().FirstOrDefault(e => e.EmployeeID == x.SecondApprovalID)?.LastName
                             : _genericRepositoryDesignations.All().FirstOrDefault(d => d.DesignationID == x.SecondApprovalID)?.DesignationName ?? "_")
                         : "_",
                    //IsDesignationOrEmpSecondApprovalID = x.IsDesignationOrEmpSecondApprovalID,
                    // IsEnableThirdApproval = x.IsEnableThirdApproval,
                    ThirdApprovalID = x.ThirdApprovalID,
                    ThirdApprovalName = x.ThirdApprovalID.HasValue
                        ? (x.IsDesignationOrEmpThirdApprovalID
                            ? _genericRepositoryEmployees.All().FirstOrDefault(e => e.EmployeeID == x.ThirdApprovalID)?.FirstName + " " + _genericRepositoryEmployees.All().FirstOrDefault(e => e.EmployeeID == x.ThirdApprovalID)?.LastName
                            : _genericRepositoryDesignations.All().FirstOrDefault(d => d.DesignationID == x.ThirdApprovalID)?.DesignationName ?? "_")
                        : "_",
                    // IsDesignationOrEmpThirdApprovalID = x.IsDesignationOrEmpThirdApprovalID,
                    CreatedBy = x.CreatedBy,
                    UpdatedBy = x.UpdatedBy,
                    // Include additional fields if necessary (e.g., LIP, LMAC)
                });

            return result;
        }
        #endregion

        #region Is UniqueName
        public async Task<bool> IsNameUniqueAsync(int approvalTypeId, int organizationId)
        {
            // Step 1: Find existing ApprovalSettings with no soft delete
            var existingApprovalSettings = await _genericRepository.FindAsync(b => b.DeletedAt == null && b.ApprovalSettingID != null);

            // Step 2: Extract the relevant properties (ApprovalTypeID and OrganizationID) for comparison
            var nameList = existingApprovalSettings.Select(b => new
            {
                b.ApprovalTypeID,
                b.OrganizationID
            }).ToList();

            // Step 3: Check if the combination of the provided ApprovalTypeID and OrganizationID exists in the list
            return !nameList.Any(x => x.ApprovalTypeID == approvalTypeId && x.OrganizationID == organizationId);
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

        #region GetApprovalTypesAsync
        // Optional: Status list from a Status table or enum
        public async Task<List<SelectListItem>> GetApprovalTypesAsync()
        {
            var approvalTypes = await _genericRepositoryApprovalType
                .All() // Assuming `Status` is the entity mapped to [Statuses]
                .Where(s => s.DeletedAt == null)
                .Select(s => new SelectListItem
                {
                    Value = s.ApprovalTypeID.ToString(),
                    Text = s.ApprovalTypeName.ToString()
                })
                .ToListAsync();

            return approvalTypes;
        }
        #endregion

        #region GetEmployeeAsync
        public async Task<List<SelectListItem>> GetEmployeeAsync()
        {
            var employees = await _genericRepositoryEmployees.All()
                .Where(e => e.DeletedAt == null)
                .Select(e => new SelectListItem
                {
                    Value = e.EmployeeID.ToString(),
                    Text = e.FirstName +" "+e.LastName
                })
                .ToListAsync();
            return employees;
        }
        #endregion

        #region GetDesignationAsync
        public async Task<List<SelectListItem>> GetDesignationAsync()
        {
            var designations = await _genericRepositoryDesignations.All()
                .Where(d => d.DeletedAt == null)
                .Select(d => new SelectListItem
                {
                    Value = d.DesignationID.ToString(),
                    Text = d.DesignationName 
                })
                .ToListAsync();
            return designations;
        }
        #endregion

      
    }
}
