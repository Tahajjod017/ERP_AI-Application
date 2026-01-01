using GCTL.Service.FieldServices;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using GCTL_App.ViewModels.FieldServiceOne;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.FieldServices
{
    public class JobDetailsController : BaseController
    {
        private readonly ICreateJobService _jobService;

        public JobDetailsController(
            ITranslateService translateService,
            IUserProfileService userProfileService,
            ICreateJobService jobService)
            : base(translateService, userProfileService)
        {
            _jobService = jobService;
        }

        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Get job information by ID
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetJobInfo(int jobId)
        {
            try
            {
                var organizationID = await GetCurrentOrganizationIdAsync() ?? 0;

                if (organizationID == 0)
                {
                    return BadRequest(new { success = false, message = "Organization not found" });
                }

                var result = await _jobService.GetByIdAsync(organizationID, jobId);

                if (result == null || result.JobID == 0)
                {
                    return NotFound(new { success = false, message = "Job not found" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving job information" });
            }
        }

        /// <summary>
        /// Save general job activity (for timeline steps)
        /// </summary>
        //[HttpPost]
        //public async Task<IActionResult> SaveActivity([FromBody] SaveActivityRequest request)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return BadRequest(new { success = false, message = "Invalid request data" });
        //        }

        //        var organizationID = await GetCurrentOrganizationIdAsync() ?? 0;
        //        var currentUserId = await GetCurrentUserIdAsync();
        //        var ip = GetUserIP();
        //        var mac = GetUserMAC();

        //        var result = await _jobService.SaveActivityAsync(
        //            request,
        //            organizationID,
        //            currentUserId,
        //            ip,
        //            mac
        //        );

        //        if (result.Success)
        //        {
        //            return Ok(result);
        //        }

        //        return BadRequest(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new
        //        {
        //            success = false,
        //            message = "Error saving activity"
        //        });
        //    }
        //}

        /// <summary>
        /// Save job team activity (Start/Push/Pause)
        /// </summary>
        [HttpPost]
        // Remove ValidateAntiForgeryToken for now to test
        public async Task<IActionResult> SaveJobTeamActivity([FromBody] SaveJobTeamActivityRequest request)
        {
            try
            {
                Console.WriteLine("=== SaveJobTeamActivity Called ===");
                Console.WriteLine($"JobID: {request?.JobID}");
                Console.WriteLine($"ActivityType: {request?.ActivityType}");

                if (request == null)
                {
                    Console.WriteLine("ERROR: Request is null");
                    return BadRequest(new { success = false, message = "Request is null" });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    Console.WriteLine($"ERROR: ModelState invalid - {string.Join(", ", errors)}");
                    return BadRequest(new { success = false, message = "Invalid request data", errors });
                }

                var organizationID = await GetCurrentOrganizationIdAsync() ?? 0;
                var currentUserId = await GetCurrentEmployeeIdAsync() ?? 0;

                Console.WriteLine($"OrganizationID: {organizationID}");
                Console.WriteLine($"CurrentUserID: {currentUserId}");

                if (organizationID == 0)
                {
                    return BadRequest(new { success = false, message = "Organization not found" });
                }

                var result = await _jobService.SaveJobTeamActivityAsync(
                    request,
                    organizationID,
                    currentUserId
                );

                Console.WriteLine($"Service Result - Success: {result.Success}, Message: {result.Message}");

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] SaveJobTeamActivity Exception: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }
    }
}