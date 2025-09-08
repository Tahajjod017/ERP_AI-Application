using GCTL.Service.Language;
using GCTL.Service.MenuTabs;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.MenuTab
{
    public class MasterSetupCategoryController : BaseController

    {
        private readonly IMenuTabsService _menuTabService;
        public MasterSetupCategoryController(ITranslateService translateService, IUserProfileService userProfileService, IMenuTabsService menuTabService) : base(translateService, userProfileService)
        {
            _menuTabService = menuTabService;
        }

        public IActionResult Index()
        {
            return View();
        }


        #region GetMasterSetupCategoriesAsync
        [Route("MasterSetupCategory/GetMasterSetupCategoriesAsync")]
        [HttpGet]
        public async Task<IActionResult> GetMasterSetupCategoriesAsync()
        {
            try
            {
                var data = await _menuTabService.GetMasterSetupCategoriesAsync();
                return Json(data);
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion
    }
}
