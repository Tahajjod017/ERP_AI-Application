using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Core.ViewModels.ElementPermission;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.ElementPermission
{
    public class ElementPermissionService : IElementPermissionService
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IGenericRepository<RoleElementPermissions> _genericRepository;

        // Constructor with dependency injection
        public ElementPermissionService(AppDbContext dbContext, UserManager<ApplicationUser> userManager, IGenericRepository<RoleElementPermissions> genericRepository) 
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _genericRepository = genericRepository;
        }

        // Check if the user's role has permission to access the element
        public async Task<bool> HasPermissionForElementAsync(string userId, int pageId, string elementKey)
        {
            // Get the user and their roles
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;  // If user not found, deny permission
            }

            // Get roles of the user
            var roleNames = await _userManager.GetRolesAsync(user);
            // Fetch the matching RoleIds from AspNetRoles
            var roleIds = await _dbContext.Roles
                .Where(r => roleNames.Contains(r.Name))
                .Select(r => r.Id)
                .ToListAsync();


            // Check if any of the user's roles has the specific permission for the element
            var hasPermission = await _dbContext.RoleElementPermissions
                .AnyAsync(p => roleIds.Contains(p.RoleId) && p.PageId == pageId && p.ElementKey == elementKey);

            return hasPermission;
        }
        #region
        public async Task<PaginationService<RoleElementPermissions, ElementPermissionVM>.PaginationResult<ElementPermissionVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "OrganizationID", string sortOrder = "desc", int? organizationID = null)
        {
            var query = _genericRepository.All()
                .AsNoTracking()
                .Include(x => x.Role) // Include related  entity
                .Include(x => x.Page); // Include related  entity
                
                //.Where(x => x.DeletedAt == null);

            // Filter by organization if provided
            //if (organizationID.HasValue && organizationID.Value > 0)
            //{
            //    query = query.Where(x => x.OrganizationID == organizationID.Value);
            //}

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
            var result = await PaginationService<RoleElementPermissions, ElementPermissionVM>.GetPaginatedData(
                query,
                pageNumber,
                pageSize,
                searchTerm,
                sortColumn,
                sortOrder,
                term => x => EF.Functions.Like(x.Page.Name, $"%{term}%") || EF.Functions.Like(x.ElementKey, $"%{term}%"),
                x => new ElementPermissionVM
                {
                    Id = x.Id,
                    PageId = x.PageId,
                    ElementKeys = x.ElementKey,
                    RoleId = x.RoleId,
                    RoleName = x.Role.Name,
                    PageName = x.Page.Name,



                });

            return result;
        }
        #endregion

        #region SoftDeleteAsyncd
        public async Task<ElementPermissionVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.Id));
                if (data == null || data.Count == 0)
                {
                    return new ElementPermissionVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<ElementPermissionVM>>(JsonConvert.SerializeObject(data));
                var targetIds = data.Select(x => (int?)x.Id).ToList();

                foreach (var item in data)
                {
                   // item.DeletedAt = DateTime.Now;
                   // item.DeletedBy = requestVM.DeletedBy;
                    //item.LIP = requestVM.LIP;
                    //item.LMAC = requestVM.LMAC;
                }

                await _genericRepository.UpdateRangeAsync(data);

                //await _userInfoService.ActionLogDeleteAsync("Holiday", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);

                await _genericRepository.CommitTransactionAsync();

                return new ElementPermissionVM
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
    }
}
