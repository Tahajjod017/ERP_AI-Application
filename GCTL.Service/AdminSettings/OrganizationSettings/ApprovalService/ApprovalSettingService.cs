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
        private readonly IGenericRepository<ApprovalDesignation> _genericRepositoryApprovalDesignations;
        private readonly IGenericRepository<EmployeeOfficeInfo> _genericRepositoryEmployeeOfficeInfo;
        private readonly IGenericRepository<ApprovalDesignation> _genericRepositoryApprovalDesignation;

        public ApprovalSettingService(IUserInfoService userInfoService, IGenericRepository<ApprovalSettings> genericRepository, IGenericRepository<Organization> genericRepositoryOraganization, IGenericRepository<OrganizationBranches> genericRepositoryBranches, IGenericRepository<ApprovalTypes> genericRepositoryApprovalType, IGenericRepository<Data.Models.Employees> genericRepositoryEmployees, IGenericRepository<Designations> genericRepositoryDesignations, IGenericRepository<Country> genericRepositoryCountry, IGenericRepository<ApprovalDesignation> genericRepositoryApprovalDesignations, IGenericRepository<EmployeeOfficeInfo> genericRepositoryEmployeeOfficeInfo, IGenericRepository<ApprovalDesignation> genericRepositoryApprovalDesignation) : base(genericRepository)
        {
            _userInfoService = userInfoService;
            _genericRepository = genericRepository;
            _genericRepositoryOraganization = genericRepositoryOraganization;
            _genericRepositoryBranches = genericRepositoryBranches;
            _genericRepositoryApprovalType = genericRepositoryApprovalType;
            _genericRepositoryEmployees = genericRepositoryEmployees;
            _genericRepositoryDesignations = genericRepositoryDesignations;
            _genericRepositoryCountry = genericRepositoryCountry;
            _genericRepositoryApprovalDesignations = genericRepositoryApprovalDesignations;
            _genericRepositoryEmployeeOfficeInfo = genericRepositoryEmployeeOfficeInfo;
            _genericRepositoryApprovalDesignation = genericRepositoryApprovalDesignation;
        }
        #endregion

        #region AddAsync

        // Helper method for parsing approval IDs with "_ad" suffix
        private static (int? id, bool isDesignation) ParseApprovalId(string input)
        {
            if (string.IsNullOrEmpty(input))
                return (null, false);

            bool isDesignation = input.EndsWith("_ad");
            string idPart = isDesignation ? input.Substring(0, input.Length - 3) : input;

            if (int.TryParse(idPart, out int id))
            {
                return (id, isDesignation);
            }
            return (null, false);
        }

        public async Task<bool> AddAsync(ApprovalSettingsVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                // Step 1: find an existing soft-deleted ApprovalSetting
                var existingEntityList = await _genericRepository.FindAsync(e =>
                    e.DeletedAt != null &&
                    e.OrganizationID == model.OrganizationID
                // Additional conditions can be added here (e.g., OrganizationBranchID)
                );

                var existingEntity = existingEntityList.FirstOrDefault();
                ApprovalSettings setting;

                // Parse approval IDs and designation flags using the helper
                var (firstApprovalId, isFirstDesignation) = ParseApprovalId(model.FirstApprovalID);
                var (secondApprovalId, isSecondDesignation) = ParseApprovalId(model.SecondApprovalID);
                var (thirdApprovalId, isThirdDesignation) = ParseApprovalId(model.ThirdApprovalID);
                var (selfApprovalId, isSelfDesignation) = ParseApprovalId(model.SelfExceptionApprovalID);


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

                    existingEntity.FirstApprovalID = firstApprovalId;
                    existingEntity.IsDesignationOrEmpFirstApprovalID = isFirstDesignation;
                    existingEntity.IsEnableSecondApproval = model.IsEnableSecondApproval == "on";
                    existingEntity.SecondApprovalID = secondApprovalId;
                    existingEntity.IsDesignationOrEmpSecondApprovalID = isSecondDesignation;
                    existingEntity.IsEnableThirdApproval = model.IsEnableThirdApproval == "on";
                    existingEntity.ThirdApprovalID = thirdApprovalId;
                    existingEntity.IsDesignationOrEmpThirdApprovalID = isThirdDesignation;
                    existingEntity.AllowSelfApproval = model.AllowSelfApproval == "on";
                    existingEntity.SelfExceptionApprovalID = isSelfDesignation ?  null: selfApprovalId;

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

                        FirstApprovalID = firstApprovalId,
                        IsDesignationOrEmpFirstApprovalID = isFirstDesignation,
                        IsEnableSecondApproval = model.IsEnableSecondApproval == "on",
                        SecondApprovalID = secondApprovalId,
                        IsDesignationOrEmpSecondApprovalID = isSecondDesignation,
                        IsEnableThirdApproval = model.IsEnableThirdApproval == "on",
                        ThirdApprovalID = thirdApprovalId,
                        IsDesignationOrEmpThirdApprovalID = isThirdDesignation,

                        AllowSelfApproval = model.AllowSelfApproval == "on",
                        SelfExceptionApprovalID = isSelfDesignation ?  null : selfApprovalId,

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
                // Step 1: Retrieve the existing entity by its ID
                var existingEntityList = await _genericRepository.FindAsync(e =>
                    e.ApprovalSettingID == model.ApprovalSettingID &&
                    (e.DeletedAt == null || e.DeletedAt != null)  // Ensure you consider both active and soft-deleted records
                );

                var existingEntity = existingEntityList.FirstOrDefault();

                if (existingEntity == null)
                {
                    throw new Exception("Approval Setting not found.");
                }

                // Step 2: Parse approval IDs and designation flags
                var (firstApprovalId, isFirstDesignation) = ParseApprovalId(model.FirstApprovalID);
                var (secondApprovalId, isSecondDesignation) = ParseApprovalId(model.SecondApprovalID);
                var (thirdApprovalId, isThirdDesignation) = ParseApprovalId(model.ThirdApprovalID);
                var (selfApprovalId, isSelfDesignation) = ParseApprovalId(model.SelfExceptionApprovalID);

                // Step 3: Update properties of the existing entity
                existingEntity.OrganizationID = model.OrganizationID;
                existingEntity.OrganizationBranchID = model.OrganizationBranchID;
                existingEntity.ApprovalTypeID = model.ApprovalTypeID;
                existingEntity.StartDate = model.StartDate;
                existingEntity.EndDate = model.EndDate;
                existingEntity.LIP = model.LIP;
                existingEntity.LMAC = model.LMAC;

                existingEntity.FirstApprovalID = firstApprovalId;
                existingEntity.IsDesignationOrEmpFirstApprovalID = isFirstDesignation;
                existingEntity.IsEnableSecondApproval = model.IsEnableSecondApproval == "on";
                existingEntity.SecondApprovalID = secondApprovalId;
                existingEntity.IsDesignationOrEmpSecondApprovalID = isSecondDesignation;
                existingEntity.IsEnableThirdApproval = model.IsEnableThirdApproval == "on";
                existingEntity.ThirdApprovalID = thirdApprovalId;
                existingEntity.IsDesignationOrEmpThirdApprovalID = isThirdDesignation;
                existingEntity.AllowSelfApproval = model.AllowSelfApproval == "on";
                existingEntity.SelfExceptionApprovalID = isSelfDesignation ? null : selfApprovalId;

                existingEntity.UpdatedAt = DateTime.Now;
                existingEntity.UpdatedBy = model.UpdatedBy ?? model.CreatedBy; // Ensure it has a valid user who updated the record

                // Optional: You can restore other fields like LIP, LMAC if needed

                // Step 4: Save changes to the repository
                await _genericRepository.UpdateAsync(existingEntity);

                // Step 5: Commit the transaction
                await _genericRepository.CommitTransactionAsync();

                return true;
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
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
        #region Get by id
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
                FirstApprovalID = entity.FirstApprovalID?.ToString(), // Fix for CS0029: Convert int? to string  
                IsDesignationOrEmpFirstApprovalID = entity.IsDesignationOrEmpFirstApprovalID ? "on" : null,
                IsEnableSecondApproval = entity.IsEnableSecondApproval ? "on" : null,
                IsEnableThirdApproval = entity.IsEnableThirdApproval ? "on" : null,
                IsDesignationOrEmpSecondApprovalID = entity.IsDesignationOrEmpSecondApprovalID ? "on" : null,
                IsDesignationOrEmpThirdApprovalID = entity.IsDesignationOrEmpThirdApprovalID ? "on" : null,
                SecondApprovalID = entity.SecondApprovalID?.ToString(), // Fix for CS0029: Convert int? to string  
                ThirdApprovalID = entity.ThirdApprovalID?.ToString(), // Fix for CS0029: Convert int? to string  
                SelfExceptionApprovalID = entity.SelfExceptionApprovalID?.ToString(), // Fix for CS0029: Convert int? to string
                AllowSelfApproval = entity.AllowSelfApproval == true ? "on" : null,
                CreatedBy = entity.CreatedBy,
                UpdatedBy = entity.UpdatedBy
                // You can add additional properties (LIP, LMAC) if required  
            };
            if (entity.IsDesignationOrEmpFirstApprovalID)
                model.FirstApprovalID += "_ad";
            if (entity.IsDesignationOrEmpSecondApprovalID)
                model.SecondApprovalID += "_ad";
            if (entity.IsDesignationOrEmpThirdApprovalID)
                model.ThirdApprovalID += "_ad";

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
            var apprvDesignation = _genericRepositoryApprovalDesignation.All()
                .Where(x => x.DeletedAt == null)
                .Select(x => new { x.ApprovalDesignationID, x.ApprovalDesignationName });
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
                    FirstApprovalID = x.FirstApprovalID?.ToString(),
                    IsDesignationOrEmpFirstApprovalID = x.IsDesignationOrEmpFirstApprovalID ? "on" : null,
                    //IsDesignationOrEmpFirstApprovalID = x.IsDesignationOrEmpFirstApprovalID,
                    // Conditional logic for FirstApprovalName based on IsDesignationOrEmpFirstApprovalID
                    FirstApprovalName = x.FirstApprovalID.HasValue
                                        ? (x.IsDesignationOrEmpFirstApprovalID
                                            ? _genericRepositoryApprovalDesignation.AllActive().FirstOrDefault(d => d.ApprovalDesignationID == x.FirstApprovalID)?.ApprovalDesignationName ?? "_"
                                            : _genericRepositoryEmployees.All().FirstOrDefault(e => e.EmployeeID == x.FirstApprovalID)?.FirstName + " " + _genericRepositoryEmployees.All().FirstOrDefault(e => e.EmployeeID == x.FirstApprovalID)?.LastName)
                                        : "_",
                    //IsEnableSecondApproval = x.IsEnableSecondApproval,
                    SecondApprovalID = x.SecondApprovalID?.ToString(),
                    IsDesignationOrEmpSecondApprovalID = x.IsDesignationOrEmpSecondApprovalID ? "on" : null,
                    SecondApprovalName = x.SecondApprovalID.HasValue
                         ? (x.IsDesignationOrEmpSecondApprovalID
                                            ? _genericRepositoryApprovalDesignation.All().FirstOrDefault(d => d.ApprovalDesignationID == x.FirstApprovalID)?.ApprovalDesignationName ?? "_"
                                            : _genericRepositoryEmployees.All().FirstOrDefault(e => e.EmployeeID == x.SecondApprovalID)?.FirstName + " " + _genericRepositoryEmployees.All().FirstOrDefault(e => e.EmployeeID == x.SecondApprovalID)?.LastName)
                                        : "_",
                    //IsDesignationOrEmpSecondApprovalID = x.IsDesignationOrEmpSecondApprovalID,
                    // IsEnableThirdApproval = x.IsEnableThirdApproval,
                    ThirdApprovalID = x.ThirdApprovalID?.ToString(),
                    IsDesignationOrEmpThirdApprovalID = x.IsDesignationOrEmpThirdApprovalID ? "on" : null,
                    ThirdApprovalName = x.ThirdApprovalID.HasValue
                       ? (x.IsDesignationOrEmpThirdApprovalID
                                            ? _genericRepositoryApprovalDesignation.All().FirstOrDefault(d => d.ApprovalDesignationID == x.FirstApprovalID)?.ApprovalDesignationName ?? "_"
                                            : _genericRepositoryEmployees.All().FirstOrDefault(e => e.EmployeeID == x.ThirdApprovalID)?.FirstName + " " + _genericRepositoryEmployees.All().FirstOrDefault(e => e.EmployeeID == x.ThirdApprovalID)?.LastName)
                                        : "_",
                    SelfExceptionApprovalID = x.SelfExceptionApprovalID?.ToString(),
                    AllowSelfApproval = x.AllowSelfApproval == true ? "on" : null,
                    SelfExceptionApprovalName = x.SelfExceptionApprovalID.HasValue
                                            ? (x.AllowSelfApproval == false
                                                ? (_genericRepositoryEmployees.All().FirstOrDefault(e => e.EmployeeID == x.SelfExceptionApprovalID)?.FirstName + " " + _genericRepositoryEmployees.All().FirstOrDefault(e => e.EmployeeID == x.SelfExceptionApprovalID)?.LastName) ?? "-"
                                                : "-")
                                            : "Yes",


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
            // Set first item as selected by default if any exist
            if (organizations.Any())
            {
                organizations[0].Selected = true;
            }



            return organizations;
        }
        public async Task<List<SelectListItem>> GetOrganizationsAsync2()
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

        #region GetEmployeeWithApprovalDesignationAsync
        public async Task<List<SelectListItem>> GetEmployeeWithApprovalDesignationAsync(int organizationId)
        {
            // Fetch Approval Designations
            var approvalDesignations = await _genericRepositoryApprovalDesignations.All()
                .Where(ad => ad.DeletedAt == null)
                .Select(ad => new SelectListItem
                {
                    Value = ad.ApprovalDesignationID.ToString() + "_ad",
                    Text = ad.ApprovalDesignationName
                })
                .ToListAsync();

            // Fetch Employees with Designation and Ranking, filtered by organizationId and ordered by rank 1 to 5
            var employeeWithDesignation = await _genericRepositoryEmployeeOfficeInfo.All()
                .Where(eoi => eoi.DeletedAt == null)
                .Join(
                    _genericRepositoryEmployees.All().Where(emp => emp.DeletedAt == null),
                    eoi => eoi.EmployeeID,
                    emp => emp.EmployeeID,
                    (eoi, emp) => new { eoi, emp }
                )
                .Join(
                    _genericRepositoryDesignations.All().Where(
                        d => d.DeletedAt == null &&
                             d.Ranking >= 1 &&
                             d.Ranking <= 5 &&
                             d.OrganizationID == organizationId
                    ),
                    combined => combined.eoi.DesignationID,
                    d => d.DesignationID,
                    (combined, d) => new { combined, d }
                )
                .OrderBy(x => x.d.Ranking) // Order employees by their designation rank
                .Select(x => new SelectListItem
                {
                    Value = x.combined.eoi.EmployeeID.ToString(),
                    Text = $"{x.combined.emp.FirstName} {x.combined.emp.LastName} | {x.d.DesignationName}"
                })
                .ToListAsync();

            // Combine both lists
            var combinedList = new List<SelectListItem>();
            combinedList.AddRange(approvalDesignations);
            combinedList.AddRange(employeeWithDesignation);

            return combinedList;
        }

        #endregion



    }
}
