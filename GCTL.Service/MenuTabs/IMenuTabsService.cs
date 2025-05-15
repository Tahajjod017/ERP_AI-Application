using GCTL.Core.ViewModels.MasterSetup.ActionTakens;
using GCTL.Core.ViewModels.MenuTab;
using GCTL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.MenuTabs
{
    public interface IMenuTabsService
    {
        Task<List<MenuTab>> GetActiveMenusAsync();

        #region CRUD
        Task<bool> AddAsync(MenuTabVM model);
        Task<bool> UpdateAsync(MenuTabVM model);
        Task<MenuTabVM> GetByIdAsync(int id);
        //Task<PagedResult<MenuTabVM>> GetAllAsync(int pageNumber, int pageSize, string searchTerm, string sortColumn, string sortOrder);
        Task<PaginationService<MenuTab, MenuTabVM>.PaginationResult<MenuTabVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "", string sortOrder = "");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}
