using GCTL.Core.Repository;
using GCTL.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.MenuTab;
using GCTL.Service.Pagination;

namespace GCTL.Service.MenuTabs
{
    public class MenuTabsService : IMenuTabsService
    {
        #region Repositories
        private readonly AppDbContext _context;
        private readonly IGenericRepository<MenuTab> _genericRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MenuTabsService(AppDbContext context, IHttpContextAccessor httpContextAccessor, IGenericRepository<MenuTab> genericRepository)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _genericRepository = genericRepository;
        }
        #endregion


        #region GetActiveMenusAsync
        public async Task<List<MenuTab>> GetActiveMenusAsync()
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return new List<MenuTab>();

            var roleId = await _context.UserRoles.Where(ur => ur.UserId == userId).Select(ur => ur.RoleId).FirstOrDefaultAsync();

            if (roleId == null) return new List<MenuTab>();

            var viewPermissionId = await _context.Permissions.Where(p => p.Name == "View").Select(p => p.Id).FirstOrDefaultAsync();

            if (viewPermissionId == 0) return new List<MenuTab>();

            var menus = await _context.MenuTab.Where(m =>m.IsActive == true &&  // ✅ Only active menus for left menu and others inactive menu is for l4 and l5
                _context.RoleModulePermissions
                .Any(rmp => rmp.RoleId == roleId && rmp.MenuTabId == m.MenuTabId && rmp.PermissionId == viewPermissionId && rmp.IsGranted)).OrderBy(m => m.OrderBy).ToListAsync();

            //await _hubContext.Clients.All.SendAsync("ReceiveMenuUpdate");
            return menus;
        }
        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(MenuTabVM model)
        {
            await _context.Database.BeginTransactionAsync();
            try
            {
                MenuTab entity = new MenuTab();
                entity.Title = model.Title;
                entity.Type = model.Type;
                entity.ParentId = model.ParentId;
                entity.OrderBy = model.OrderBy ?? 0;
                entity.ControllerName = model.ControllerName;
                entity.Icon = model.Icon;
                entity.IsActive = model.IsActive;

                await _context.MenuTab.AddAsync(entity);
                await _context.SaveChangesAsync();

                await _context.Database.CommitTransactionAsync();

                return true;
            }
            catch (Exception ex)
            {
                await _context.Database.RollbackTransactionAsync();
                throw new Exception(ex.Message, ex);
            }
        }
        #endregion


        #region Update
        public async Task<bool> UpdateAsync(MenuTabVM model)
        {
            await _context.Database.BeginTransactionAsync();
            try
            {
                var entity = await _context.MenuTab.FindAsync(model.MenuTabId);
                if (entity == null)
                {
                    return false;
                }

                entity.Title = model.Title;
                entity.Type = model.Type;
                entity.ParentId = model.ParentId;
                entity.OrderBy = model.OrderBy ?? 0;
                entity.ControllerName = model.ControllerName;
                entity.Icon = model.Icon;
                entity.IsActive = model.IsActive;

                _context.MenuTab.Update(entity);
                await _context.SaveChangesAsync();

                await _context.Database.CommitTransactionAsync();

                return true;
            }
            catch (Exception ex)
            {
                await _context.Database.RollbackTransactionAsync();
                throw new Exception(ex.Message, ex);
            }
        }
        #endregion


        #region Get
        public async Task<MenuTabVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _context.MenuTab.FindAsync(id);
                if (data == null) return null;

                return new MenuTabVM
                {
                    MenuTabId = data.MenuTabId,
                    Title = data.Title,
                    Type = data.Type,
                    ParentId = data.ParentId,
                    OrderBy = data.OrderBy,
                    ControllerName = data.ControllerName,
                    ViewName = data.ViewName,
                    Icon = data.Icon,
                    IsActive = data.IsActive,
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        #endregion


        #region IsNameUniqueAsync
        public async Task<bool> IsNameUniqueAsync(string name)
        {
            var existingName = await _context.MenuTab.FirstOrDefaultAsync(b => b.ControllerName == name);
            return existingName == null;
        }
        #endregion


        #region GetAllAsync
        public async Task<PaginationService<MenuTab, MenuTabVM>.PaginationResult<MenuTabVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "", string sortOrder = "")
        {
            var query = _genericRepository.All()
                .OrderBy(x => x.Type == "Primary" ? 0 :
                x.Type == "Secondary" ? 1 :
                x.Type == "Tertiary" ? 2 : 3);

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "MenuTabId" => sortOrder == "desc" ? query.OrderByDescending(x => x.MenuTabId) : query.OrderBy(x => x.MenuTabId),
                    "Title" => sortOrder == "desc" ? query.OrderByDescending(x => x.Title) : query.OrderBy(x => x.Title),
                    "Type" => sortOrder == "desc" ? query.OrderByDescending(x => x.Type) : query.OrderBy(x => x.Type),
                    "ParentId" => sortOrder == "desc" ? query.OrderByDescending(x => x.ParentId) : query.OrderBy(x => x.ParentId),
                    "OrderBy" => sortOrder == "desc" ? query.OrderByDescending(x => x.OrderBy) : query.OrderBy(x => x.OrderBy),
                    "ControllerName" => sortOrder == "desc" ? query.OrderByDescending(x => x.ControllerName) : query.OrderBy(x => x.ControllerName),
                    "ViewName" => sortOrder == "desc" ? query.OrderByDescending(x => x.ViewName) : query.OrderBy(x => x.ViewName),
                    "Icon" => sortOrder == "desc" ? query.OrderByDescending(x => x.Icon) : query.OrderBy(x => x.Icon),
                    "IsActive" => sortOrder == "desc" ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                    _ => query.OrderBy(x => x.MenuTabId)
                };
            }


            var result = await PaginationService<MenuTab, MenuTabVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.Title, $"%{term}%") 
                || EF.Functions.Like(x.Type, $"%{term}%") 
                || EF.Functions.Like(x.ControllerName, $"%{term}%") 
                || EF.Functions.Like(x.Icon, $"%{term}%")
                || EF.Functions.Like(x.IsActive.ToString(), $"%{term}%"),
                x => new MenuTabVM
                {
                    MenuTabId = x.MenuTabId,
                    Title = x.Title ?? "-",
                    Type = x.Type ?? "-",
                    ParentId = x.ParentId,
                    OrderBy = x.OrderBy,
                    ControllerName = x.ControllerName ?? "-",
                    ViewName = x.ViewName ?? "-",
                    Icon = x.Icon ?? "-",
                    IsActive = x.IsActive
                });

            return result;
        }
        #endregion
    }
}
