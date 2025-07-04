using GCTL.Core.ViewModels.HRMsettingsVM;
using GCTL.Service.HRMsettings.ProbationService;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GCTL_App.Controllers.HRMsettings
{
    public class ProbationPeriodSettingController : BaseController
    {
        private readonly IProbationSettingService _probationSettingService;
        public ProbationPeriodSettingController(ITranslateService translateService, IUserProfileService userProfileService, IProbationSettingService probationSettingService) : base(translateService, userProfileService)
        {
            _probationSettingService = probationSettingService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Organizations = await _probationSettingService.GetOrganizationsAsync();
            return View();
        }
        public async Task<IActionResult> GetProbationPeriods(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "ProbationID", string sortOrder = "desc")
        {
            var result = await _probationSettingService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);
            return Json(result);
        }
        [HttpPost]
        public async Task<IActionResult> Create(ProbationSettingVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //var uniqueName = await _probationSettingService.IsNameUniqueAsync(model.OrganizationName);
                    //if (!uniqueName)
                    //{
                    //    return Json(new { isSuccess = false, message = "This name already exists!" });
                    //}
                    await _probationSettingService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.Period });
                }
                var errorMessage = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
                return Json(new { isSuccess = false, message = errorMessage ?? "Something went wrong." });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
    }
}
