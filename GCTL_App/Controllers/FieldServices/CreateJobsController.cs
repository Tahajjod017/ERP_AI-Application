using GCTL.Core.Repository;
using GCTL.Core.ViewModels.FieldServices;
using GCTL.Data.Models;
using GCTL.Service.FieldServices;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.FieldServices
{
    public class CreateJobsController : BaseController
    {
        private readonly ICreateJobService _createJobService;
        public readonly IGenericRepository<Statuses> _statusRepository;
        public readonly IGenericRepository<JobTypes> _jobTypeRepository;
        public CreateJobsController(ITranslateService translateService, IUserProfileService userProfileService, ICreateJobService createJobService, IGenericRepository<Statuses> statusRepository, IGenericRepository<JobTypes> jobTypeRepository) : base(translateService, userProfileService)
        {
            _createJobService = createJobService;
            _statusRepository = statusRepository;
            _jobTypeRepository = jobTypeRepository;
        }
        public IActionResult Index()
        {
            ViewBag.JobTypesDD = new SelectList(_jobTypeRepository.AllActive().Select(t => new {t.JobTypeID, t.JobTypeName}), "JobTypeID", "JobTypeName");
            ViewBag.StatusDD = new SelectList(_statusRepository.AllActive().Where(t => t.StatusType == "FieldService").Select(t => new {t.StatusID, t.StatusName}), "StatusID", "StatusName");
            return View();
        }

        #region Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert([FromForm] CreateJobVM createJobVM)
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
        #endregion
    }
}
