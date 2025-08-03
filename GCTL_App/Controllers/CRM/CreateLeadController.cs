using GCTL.Core.Repository;
using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.MasterSetup.ServiceType;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;


namespace GCTL_App.Controllers.CRM
{
    public class CreateLeadController : BaseController
    {
        #region CTOR
        private readonly IGenericRepository<Services> _serviceTypeRepository;
        private readonly IGenericRepository<LeadSources> _leadSourceTypeRepository;
        private readonly IGenericRepository<LeadStatuses> _leadStatusesTypeRepository;
        #endregion
        public CreateLeadController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<Services> serviceTypeRepository, IGenericRepository<LeadSources> leadSourceTypeRepository, IGenericRepository<LeadStatuses> leadStatusesTypeRepository = null) : base(translateService, userProfileService)
        {
            _serviceTypeRepository = serviceTypeRepository;
            _leadSourceTypeRepository = leadSourceTypeRepository;
            _leadStatusesTypeRepository = leadStatusesTypeRepository;
        }

        public async Task<IActionResult> index()
        {
            SetSmartPageCode(600100);

            ViewBag.ServiceDD = new SelectList(_serviceTypeRepository.AllActive().Select(e => new { e.ServiceID, e.ServiceName }), "ServiceID", "ServiceName");
            ViewBag.LeadSourceDD = new SelectList(_leadSourceTypeRepository.AllActive().Select(e => new { e.LeadSourceID, e.LeadSourceName }), "LeadSourceID", "LeadSourceName");
            ViewBag.LeadStatusDD = new SelectList(_leadStatusesTypeRepository.AllActive().Select(e => new { e.LeadStatusID, e.LeadStatusName }), "LeadStatusID", "LeadStatusName");
            

            return View();
        }


        [HttpGet]
        public IActionResult GetNationalities()
        {
            var nationalities = _serviceTypeRepository.AllActive()
                .OrderBy(n => n.ServiceName)
                .Select(n => n.ServiceName)
                .ToList();

            return Json(nationalities);
        }

    }
}
