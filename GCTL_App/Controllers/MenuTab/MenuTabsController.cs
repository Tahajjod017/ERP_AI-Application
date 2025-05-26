using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MenuTab;
using GCTL.Service.Language;
using GCTL.Service.MenuTabs;
using GCTL.Service.RolePermissions;
using GCTL_App.ViewModels.MenuTab;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.MenuTab
{
    public class MenuTabsController : BaseController
    {
        #region Services & Repositories
        private readonly IMenuTabsService _menuTabService;
        private readonly IGenericRepository<GCTL.Data.Models.MenuTab> _menuTabRepository;

        public MenuTabsController(IMenuTabsService menuTabsService, IGenericRepository<GCTL.Data.Models.MenuTab> menuTabRepository, ITranslateService translateService) : base(translateService)
        {
            _menuTabService = menuTabsService;
            _menuTabRepository = menuTabRepository;
        }
        #endregion


        #region Index
        //[Permission("View", " MenuTabs")]
        public IActionResult Index()
        {
            var menuTabs = _menuTabRepository.All()
                .Where(x => /* any filters if needed */ true)
                .Select(x => new MenuTabVM
                {
                    MenuTabId = x.MenuTabId,
                    Title = x.Title,
                    Type = x.Type,
                    ParentId = x.ParentId,
                    OrderBy = x.OrderBy,
                    ControllerName = x.ControllerName,
                    ViewName = x.ViewName,
                    Icon = x.Icon,
                    IsActive = x.IsActive
                }).ToList();

            MenuTabPageVM model = new MenuTabPageVM()
            {
                Setup = new MenuTabVM(),
                List = menuTabs
            };

            return View(model);
        }
        #endregion


        #region Create
        [HttpPost]
        public async Task<IActionResult> Create(MenuTabVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if(model.ControllerName != null)
                    {
                        var uniqueName = await _menuTabService.IsNameUniqueAsync(model.ControllerName);
                        if (!uniqueName)
                        {
                            return Json(new { isSuccess = false, message = "This name already exists!" });
                        }
                    }

                    await _menuTabService.AddAsync(model);
                    return RedirectToAction(nameof(Index));
                }
                var errorMessage = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;

                return Json(new { isSuccess = false, message = errorMessage ?? "Something went wrong." });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        #endregion


        #region Update
        public async Task<IActionResult> Update(MenuTabVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _menuTabService.UpdateAsync(model);
                    return RedirectToAction(nameof(Index));
                }
                var errorMessage = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
                return Json(new { isSuccess = false, message = errorMessage ?? "Something went wrong." });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        #endregion


        #region CheckNameUnique
        [HttpPost]
        public async Task<IActionResult> CheckNameUnique(string name)
        {
            try
            {
                bool isUnique = await _menuTabService.IsNameUniqueAsync(name);
                if (!isUnique)
                {
                    return Json("This name already exists.");
                }
                return Json(true);
            }
            catch (Exception ex)
            {
                return Json("Error occurred: " + ex.Message);
            }
        }
        #endregion


        #region GetById
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _menuTabService.GetByIdAsync(id);
                if (result == null)
                {
                    return Json(new { isSuccess = false, message = "No data found!" });
                }

                return Json(new { isSuccess = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        #endregion


        #region GetAll
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "", string sortOrder = "")
        {
            var result = await _menuTabService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion
    }
}
