using GCTL.Core.ViewModels.MasterSetup.Genders;
using GCTL.Core.ViewModels.MasterSetup.ServiceType;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.Gender;
using GCTL.Service.MasterSetup.ServiceType;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.MasterSetup
{
    public class ServiceTypeController : BaseController
    {
        #region Services & Repositories
        private readonly IServiceTypeService _serviceTypeService;
        private readonly ITranslateService _translationService;
        public ServiceTypeController(ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {
        }
        #endregion
        public IActionResult Index()
        {
            var vm = new GCTL_App.ViewModels.MasterSetup.ServiceType.ServicTypePageVM();
            return View(vm);
        }

        #region create
        [HttpPost]
        public async Task<IActionResult> Create(ServiceTypeVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var uniqueName = await _serviceTypeService.IsNameUniqueAsync(model.ServiceTypeName);
                    if (!uniqueName)
                    {
                        return Json(new { isSuccess = false, message = "This name already exists!" });
                    }
                    await _serviceTypeService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.ServiceTypeID });
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
    }
}
