using GCTL.Core.Repository;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.FieldServices
{
    public class EmployeeAdvancedController : BaseController
    {
        private readonly IGenericRepository<JobTypes> _jobTypeRepository;
        public EmployeeAdvancedController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<JobTypes> jobTypeRepository) : base(translateService, userProfileService)
        {
            _jobTypeRepository = jobTypeRepository;
        }

        public IActionResult Index()
        {
            ViewBag.JobTypesDD = new SelectList(_jobTypeRepository.AllActive().Select(t => new { t.JobTypeID, t.JobTypeName }), "JobTypeID", "JobTypeName");

            return View();
        }
    }
}
