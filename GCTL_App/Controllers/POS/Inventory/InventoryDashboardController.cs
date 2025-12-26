using GCTL.Service.Language;
using GCTL.Service.POS.Inventory;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.POS.Inventory
{
    public class InventoryDashboardController : BaseController
    {
        private readonly IInventoryService _inventoryService;

        public InventoryDashboardController(ITranslateService translateService, IUserProfileService userProfileService, IInventoryService inventoryService) : base(translateService, userProfileService)
        {
            _inventoryService = inventoryService;
        }

        public async Task<IActionResult> Index()
        {
            int? orgID = await GetCurrentOrganizationIdAsync();
            var dashboardData = await _inventoryService.GetDashboardDataAsync(orgID);
            return View(dashboardData);
        }
    }
}
