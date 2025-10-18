using GCTL.Core.ViewModels.FieldServices;
using GCTL.Service.FieldServices;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace GCTL_App.Controllers.FieldServices
{
    public class CreateJobsController : BaseController
    {
        private readonly ICreateJobService _createJobService;
        public CreateJobsController(ITranslateService translateService, IUserProfileService userProfileService, ICreateJobService createJobService) : base(translateService, userProfileService)
        {
            _createJobService = createJobService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateJob([FromForm] CreateJobVM createJobVM)
        {
            try
            {
                if (createJobVM.CreateJobID == 0) {
                    var result = await _createJobService.AddAsync(createJobVM);
                    return Ok(result);
                }
                return Ok(new { Success = false, Message = "Id is not valid" });
            } catch(Exception ex)
            {
                return Ok(new { Success = false, Message = ex });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateJob([FromForm] CreateJobVM createJobVM)
        {
            try
            {
                if (createJobVM.CreateJobID > 0) {
                    var result = await _createJobService.UpdateAsync(createJobVM);
                    return Ok(result);
                }
                return Ok(new { Success = false, Message = "Id is not valid" });
            } catch(Exception ex)
            {
                return Ok(new { Success = false, Message = ex });
            }
        }
    }
}
