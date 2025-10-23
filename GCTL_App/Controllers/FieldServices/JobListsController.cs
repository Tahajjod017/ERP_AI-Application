using GCTL.Service.FieldServices;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.FieldServices
{
    public class JobListsController : BaseController
    {
        private readonly ICreateJobService _jobService;
        public JobListsController(ITranslateService translateService, IUserProfileService userProfileService, ICreateJobService jobService) : base(translateService, userProfileService)
        {
            _jobService = jobService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetJobList(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "CreateJobID", string sortOrder = "asc") 
        {
            try
            {
                var organizationID = await GetCurrentOrganizationIdAsync() ?? 0;
                var result = await _jobService.GetAllAsync(organizationID, pageNumber, pageSize, searchTerm, sortColumn, sortOrder);
                return Ok(result);
            }
            catch (Exception) { return Ok(); }
        }
    }
}
