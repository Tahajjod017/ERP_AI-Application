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

namespace GCTL.Service.AdminSettings.OrganizationSettings.DepartmentService
{
    public class DepartmentSettingService : AppService<Departments>, IDepartmentSettingService
    {
        #region Repositories & Services
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<Departments> _genericRepository;
        private readonly IGenericRepository<Organization> _genericRepositoryOraganization;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> genericRepositoryEmployees;

        public DepartmentSettingService(IUserInfoService userInfoService, IGenericRepository<Departments> genericRepository, IGenericRepository<Organization> genericRepositoryOraganization, IGenericRepository<Data.Models.Employees> genericRepositoryEmployees) : base(genericRepository)
        {
            _userInfoService = userInfoService;
            _genericRepository = genericRepository;
            _genericRepositoryOraganization = genericRepositoryOraganization;
            this.genericRepositoryEmployees = genericRepositoryEmployees;
        }
        #endregion
        #region AddAsync  
        public async Task<bool> AddAsync(DepartmentSettingsVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                // Try to find an existing soft-deleted record  
                var existingEntityList = await _genericRepository.FindAsync(e =>
                    e.DeletedAt != null &&
                    e.DepartmentID == model.DepartmentID
                );

                var existingEntity = existingEntityList.FirstOrDefault();

                if (existingEntity != null)
                {
                    // Update and restore
                    existingEntity.OrganizationID = model.OrganizationID;
                    existingEntity.DepartmentName = model.DepartmentName;
                    existingEntity.DepartmentHeadEmpID = model.DepartmentHeadEmpID;




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
                    var newEntity = new Departments
                    {
                        OrganizationID = model.OrganizationID,
                        DepartmentName = model.DepartmentName,
                        DepartmentHeadEmpID = model.DepartmentHeadEmpID,
                        //IsDepartmentHead = model.IsDepartmentHead,




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
        public async Task<bool> UpdateAsync(DepartmentSettingsVM model)
        {
            await _genericRepository.BeginTransactionAsync();

            try
            {
                // Find existing record (including soft-deleted ones)
                var existingList = await _genericRepository.FindAsync(e =>
                    (e.DeletedAt == null || e.DeletedAt != null) &&
                    e.DepartmentID == model.DepartmentID // Assuming you have a primary key in the model
                );

                var entity = existingList.FirstOrDefault();

                if (entity == null)
                {
                    throw new Exception(" Department not found.");
                }

                // Update properties

                entity.OrganizationID = model.OrganizationID;
                entity.DepartmentName = model.DepartmentName;
                entity.DepartmentHeadEmpID = model.DepartmentHeadEmpID;
                //entity.IsDepartmentHead = model.IsDepartmentHead;





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
        public async Task<DepartmentSettingsVM> GetByIdAsync(int id)
        {
            // Retrieve the EmailSettings entity by ID, excluding soft-deleted records
            var entityList = await _genericRepository.FindAsync(x => x.DepartmentID == id && x.DeletedAt == null);
            var entity = entityList.FirstOrDefault();

            if (entity == null)
                return null;

            // Map to ViewModel
            var model = new DepartmentSettingsVM
            {
                DepartmentID = entity.DepartmentID,
                OrganizationID = entity.OrganizationID,
                DepartmentName = entity.DepartmentName,
                DepartmentHeadEmpID = entity.DepartmentHeadEmpID,
                //IsDepartmentHead = entity.IsDepartmentHead,




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
            var existingNames = await _genericRepository.FindAsync(b => b.DeletedAt == null && b.DepartmentName != null);

            var nameList = existingNames.Select(b => b.DepartmentName);

            return !DuplicateChecker.IsDuplicate(name, nameList);
        }
        #endregion


        #region Soft Delete
        public async Task<DepartmentSettingsVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.DepartmentID));
                if (data == null || data.Count == 0)
                {
                    return new DepartmentSettingsVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

               // var beforeEntity = JsonConvert.DeserializeObject<List<DepartmentSettingsVM>>(JsonConvert.SerializeObject(data));
                var targetIds = data.Select(x => (int?)x.DepartmentID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.DeletedBy = requestVM.DeletedBy;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                }

                await _genericRepository.UpdateRangeAsync(data);

                //await _userInfoService.ActionLogDeleteAsync("Blood Group", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);

                await _genericRepository.CommitTransactionAsync();

                return new DepartmentSettingsVM
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

        #region Table
        public async Task<PaginationService<Departments, DepartmentSettingsVM>.PaginationResult<DepartmentSettingsVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "OrganizationID", string sortOrder = "desc", int? organizationID = null)
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
            var result = await PaginationService<Departments, DepartmentSettingsVM>.GetPaginatedData(
                query,
                pageNumber,
                pageSize,
                searchTerm,
                sortColumn,
                sortOrder,
                term => x => EF.Functions.Like(x.DepartmentName, $"%{term}%") || EF.Functions.Like(x.Organization.OrganizationName, $"%{term}%"),
                x => new DepartmentSettingsVM
                {
                    DepartmentID = x.DepartmentID,
                    OrganizationID = x.OrganizationID,
                    DepartmentName = x.DepartmentName,
                    OrganizationName = x.Organization?.OrganizationName ?? "_", 
                    //IsDepartmentHead = x.IsDepartmentHead,
                    DepartmentHeadEmpID = x.DepartmentHeadEmpID,
                    HeadEmployeeName = x.DepartmentHeadEmpID.HasValue
                        ? genericRepositoryEmployees.All()
                            .Where(emp => emp.EmployeeID == x.DepartmentHeadEmpID)
                            .Select(emp => emp.FirstName +" "+emp.LastName)
                            .FirstOrDefault()
                        : null,


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
        public async Task<List<SelectListItem>> GetEmployeeCodeAsync()
        {
            var employeeCodeWithName = await genericRepositoryEmployees.All()
                .Where(e => e.DeletedAt == null)
                .Select(e => new SelectListItem
                {
                    Value = e.EmployeeID.ToString(),
                    Text = e.FirstName + " " + e.LastName + "|" + e.EmployeeCode
                }).ToListAsync();

            return employeeCodeWithName;
        }
        #endregion
    }
}
