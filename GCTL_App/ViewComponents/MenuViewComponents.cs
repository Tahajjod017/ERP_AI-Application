using GCTL.Data.Models;
using GCTL.Service.MenuTabs;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.ViewComponents
{
    public class MenuViewComponents : ViewComponent
    {
        private readonly IMenuTabsService _menuTabsService;

        public MenuViewComponents(IMenuTabsService menuTabsService)
        {
            _menuTabsService = menuTabsService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var menus = await _menuTabsService.GetActiveMenusAsync();
            if (menus == null)
            {
                menus = new List<MenuTab>();
            }

            // Build the hierarchical menu using InverseParent
            var menuHierarchy = BuildMenuHierarchy(menus);
            return View(menuHierarchy);  // Pass the hierarchy to the view
        }

        private List<MenuTab> BuildMenuHierarchy(List<MenuTab> menus)
        {
            // Select only parent menus (where ParentId is null)
            return menus.Where(m => m.ParentId == null).OrderBy(m => m.OrderBy)  // Optional: order menus
                .Select(m => new MenuTab
                {
                    MenuTabId = m.MenuTabId,
                    Title = m.Title,
                    ControllerName = m.ControllerName,
                    Icon = m.Icon,
                    IsActive = m.IsActive,
                    InverseParent = GetChildMenus(menus, m.MenuTabId)  // Get child menus
                }).ToList();
        }

        private List<MenuTab> GetChildMenus(List<MenuTab> menus, int parentId)
        {
            // Recursively get child menus based on ParentId
            return menus.Where(m => m.ParentId == parentId).OrderBy(m => m.OrderBy)
                .Select(m => new MenuTab
                {
                    MenuTabId = m.MenuTabId,
                    Title = m.Title,
                    ControllerName = m.ControllerName,
                    Icon = m.Icon,
                    IsActive = m.IsActive,
                    InverseParent = GetChildMenus(menus, m.MenuTabId)  // Recursive call for deeper levels
                }).ToList();
        }
    }
}
