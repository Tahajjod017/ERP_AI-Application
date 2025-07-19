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

namespace GCTL.Service.AdminSettings.OrganizationSettings.HolidayService
{
    public class HolidaySettingService :AppService<Holidays>, IHolidaySettingService
    {
        #region Repositories & Services
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<Holidays> _genericRepository;
        private readonly IGenericRepository<Organization> _genericRepositoryOraganization;
        private readonly IGenericRepository<Statuses> genericRepositoryStatus;

        public HolidaySettingService(IUserInfoService userInfoService, IGenericRepository<Holidays> genericRepository, IGenericRepository<Organization> genericRepositoryOraganization, IGenericRepository<Statuses> genericRepositoryStatus) : base(genericRepository)
        {
            _userInfoService = userInfoService;
            _genericRepository = genericRepository;
            _genericRepositoryOraganization = genericRepositoryOraganization;
            this.genericRepositoryStatus = genericRepositoryStatus;
        }


        #endregion
        #region AddAsync  
        public async Task<bool> AddAsync(HolidayViewModel model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                // Try to find an existing soft-deleted record  
                var existingEntityList = await _genericRepository.FindAsync(e =>
                    e.DeletedAt != null &&
                    e.HolidayID == model.HolidayID
                );

                var existingEntity = existingEntityList.FirstOrDefault();

                if (existingEntity != null)
                {
                    // Update and restore
                    existingEntity.OrganizationID = model.OrganizationID;
                   existingEntity.OrganizationBranchID = model.OrganizationBranchID;
                    existingEntity.HolidayTitle = model.HolidayTitle;
                    existingEntity.HolidayDescription = model.HolidayDescription;

                    // Fix for CS0029: Convert string to DateTime? using DateTime.TryParse or DateTime.ParseExact if format is known.

                    existingEntity.StartDate = DateTime.TryParse(model.StartDate, out var parsedStartDate) ? parsedStartDate : null;
                    existingEntity.EndDate = DateTime.TryParse(model.EndDate, out var parsedEndDate) ? parsedEndDate : null;
                    //existingEntity.StartDate = model.StartDate;
                    //existingEntity.EndDate = model.EndDate;
                    existingEntity.TotalDays = model.TotalDays;
                    existingEntity.StatusID = model.StatusID;


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
                    var newEntity = new Holidays
                    {
                        OrganizationID = model.OrganizationID,
                        OrganizationBranchID = model.OrganizationBranchID,
                        HolidayTitle = model.HolidayTitle,
                        HolidayDescription = model.HolidayDescription,
                        StartDate = DateTime.TryParse(model.StartDate, out var parsedStartDate) ? parsedStartDate : null,
                        EndDate = DateTime.TryParse(model.EndDate, out var parsedEndDate) ? parsedEndDate : null,
                        TotalDays = model.TotalDays,
                        StatusID = model.StatusID,


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
        public async Task<bool> UpdateAsync(HolidayViewModel model)
        {
            await _genericRepository.BeginTransactionAsync();

            try
            {
                // Find existing record (including soft-deleted ones)
                var existingList = await _genericRepository.FindAsync(e =>
                    (e.DeletedAt == null || e.DeletedAt != null) &&
                    e.HolidayID == model.HolidayID // Assuming you have a primary key in the model
                );

                var entity = existingList.FirstOrDefault();

                if (entity == null)
                {
                    throw new Exception(" Holiday not found.");
                }

                // Update properties
                entity.OrganizationID = model.OrganizationID;
                entity.OrganizationBranchID = model.OrganizationBranchID;
                entity.HolidayTitle = model.HolidayTitle;
                entity.HolidayDescription = model.HolidayDescription;
                entity.StartDate = DateTime.TryParse(model.StartDate, out var parsedStartDate) ? parsedStartDate : null;
                entity.EndDate = DateTime.TryParse(model.EndDate, out var parsedEndDate) ? parsedEndDate : null;
                entity.TotalDays = model.TotalDays;
                entity.StatusID = model.StatusID;




                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = model.UpdatedBy ?? 0; // default or current user
                entity.DeletedAt = null; // Restore if previously deleted
                //entity.LIP = model.LIP;
                //entity.LMAC = model.LMAC;

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

        public async Task<HolidayViewModel> GetByIdAsync(int id)
        {
            // Retrieve the EmailSettings entity by ID, excluding soft-deleted records  
            var entityList = await _genericRepository.FindAsync(x => x.HolidayID == id && x.DeletedAt == null);
            var entity = entityList.FirstOrDefault();

            if (entity == null)
                return null;

            // Map to ViewModel  
            var model = new HolidayViewModel
            {
                OrganizationID = entity.OrganizationID,
                OrganizationBranchID = entity.OrganizationBranchID,
                HolidayTitle = entity.HolidayTitle,
                HolidayDescription = entity.HolidayDescription,
                StartDate = entity.StartDate.HasValue ? entity.StartDate.Value.ToString("yyyy-MM-dd") : null,
                EndDate = entity.EndDate.HasValue ? entity.EndDate.Value.ToString("yyyy-MM-dd") : null,
                TotalDays = entity.TotalDays,
                StatusID = entity.StatusID,
                CreatedBy = entity.CreatedBy,
                UpdatedBy = entity.UpdatedBy
            };

            return model;
        }

        #endregion


        #region IsNameUniqueAsync
        public async Task<bool> IsNameUniqueAsync(string name)
        {
            var existingNames = await _genericRepository.FindAsync(b => b.DeletedAt == null && b.HolidayID != null);

            var nameList = existingNames.Select(b => b.HolidayTitle);

            return !DuplicateChecker.IsDuplicate(name, nameList);
        }
        #endregion


        #region Soft Delete
        public async Task<HolidayViewModel> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.HolidayID));
                if (data == null || data.Count == 0)
                {
                    return new HolidayViewModel
                    {
                        Message = "No data found to soft delete."
                    };
                }

                //var beforeEntity = JsonConvert.DeserializeObject<List<HolidayViewModel>>(JsonConvert.SerializeObject(data));
                var targetIds = data.Select(x => (int?)x.HolidayID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.DeletedBy = requestVM.DeletedBy;
                    //item.LIP = requestVM.LIP;
                    //item.LMAC = requestVM.LMAC;
                }

                await _genericRepository.UpdateRangeAsync(data);

                //await _userInfoService.ActionLogDeleteAsync("Holiday", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);

                await _genericRepository.CommitTransactionAsync();

                return new HolidayViewModel
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

        #region Data Table List
        public async Task<PaginationService<Holidays, HolidayViewModel>.PaginationResult<HolidayViewModel>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "OrganizationID", string sortOrder = "desc", int? organizationID = null)
        {
            var query = _genericRepository.All()
                .AsNoTracking()
                .Include(x => x.Organization) // Include related Organization entity
                .Include(x => x.OrganizationBranch) // Include related OrganizationBranch entity
                .Include(x => x.Status) // Include related Status entity
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
            var result = await PaginationService<Holidays, HolidayViewModel>.GetPaginatedData(
                query,
                pageNumber,
                pageSize,
                searchTerm,
                sortColumn,
                sortOrder,
                term => x => EF.Functions.Like(x.HolidayTitle, $"%{term}%") || EF.Functions.Like(x.Status.StatusName, $"%{term}%"),
                x => new HolidayViewModel
                {
                    OrganizationID = x.OrganizationID,
                    OrganizationBranchID = x.OrganizationBranchID,
                    HolidayID = x.HolidayID,
                    HolidayTitle = x.HolidayTitle??"_",
                    HolidayDescription = x.HolidayDescription??"_",
                    StartDate = x.StartDate.HasValue ? x.StartDate.Value.ToString("yyyy-MM-dd") : "_",
                    EndDate = x.EndDate.HasValue ? x.EndDate.Value.ToString("yyyy-MM-dd") : "_",
                    TotalDays = x.TotalDays,
                    StatusName = !string.IsNullOrEmpty(x.Status?.StatusName) ? x.Status.StatusName : "_",


                    // CreatedAt = x.CreatedAt,
                    CreatedBy = x.CreatedBy,
                    //UpdatedAt = x.UpdatedAt,
                    UpdatedBy = x.UpdatedBy,
                    //LIP = x.LIP,
                    //LMAC = x.LMAC
                });

            return result;
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

        #region GetHolidayStatusesAsync
        // Optional: Status list from a Status table or enum
        public async Task<List<SelectListItem>> GetHolidayStatusesAsync()
        {
            var holidayStatuses = await genericRepositoryStatus
                .All() // Assuming `Status` is the entity mapped to [Statuses]
                .Where(s => s.DeletedAt == null && s.StatusType == "Active/Inactive")
                .Select(s => new SelectListItem
                {
                    Value = s.StatusID.ToString(),
                    Text = s.StatusName
                })
                .ToListAsync();

            return holidayStatuses;
        }
        #endregion

    }
}
