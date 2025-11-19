using GCTL.Service.FieldServices;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.FieldServices
{
    public class JobDetailsController : BaseController
    {
        private readonly ICreateJobService _jobService;
        public JobDetailsController(ITranslateService translateService, IUserProfileService userProfileService, ICreateJobService jobService) : base(translateService, userProfileService)
        {
            _jobService = jobService;
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> GetJobInfo(int jobId)
        {
            try
            {
                var organizationID = await GetCurrentOrganizationIdAsync() ?? 0;
                var result = await _jobService.GetByIdAsync(organizationID, jobId);
                return Ok(result);
            }
            catch (Exception) { return Ok(); }
        }
    }
}
