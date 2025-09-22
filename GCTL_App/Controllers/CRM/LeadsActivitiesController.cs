using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.MasterSetup.LeadStatuses;
using GCTL.Data.Models;
using GCTL.Service.CRM.LeadsActivities;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuGet.Protocol;

namespace GCTL_App.Controllers.CRM
{
    public class LeadsActivitiesController : BaseController
    {
        private readonly ILeadsActivityService _activityService;
        public readonly IGenericRepository<LeadStatuses> _leadStatusesRepository;
        private readonly IGenericRepository<AddressTypes> _addressTypeService;

        public LeadsActivitiesController(ITranslateService translateService, IUserProfileService userProfileService, ILeadsActivityService activityService, IGenericRepository<LeadStatuses> leadStatusesRepository, IGenericRepository<Services> servicesRepository, IGenericRepository<AddressTypes> addressTypeService) : base(translateService, userProfileService)
        {
            _activityService = activityService;
            _leadStatusesRepository = leadStatusesRepository;
            _addressTypeService = addressTypeService;
        }

        public IActionResult Index()
        {
            ViewBag.LeadStatus2 = new SelectList(_leadStatusesRepository.AllActive().Select(e => new { e.LeadStatusID, e.LeadStatusName }), "LeadStatusID", "LeadStatusName");
            ViewBag.ServiceTypeDD = new SelectList(_addressTypeService.AllActive().Where(u => u.AddressTypeName == "billing" || u.AddressTypeName == "company").Select(e => new { e.AddressTypeID, e.AddressTypeName }), "AddressTypeID", "AddressTypeName");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetUpcomingActivities([FromForm] UpcomingActivityVM model)
        {
       

            int page = model.PageNumber ?? 1;
            int itemPagePage = model.ItemPerPage ?? 10;
            string search = model.Search ?? string.Empty;
            string sort = model.SortColumn ?? "ActivityDateTime";
            string direction = model.SortDirection ?? "asc";
            string dateTime = model.DateRange ?? string.Empty;

            if (!model.CreatedBy.HasValue)
                return Ok(new ReturnDataView<LeadDetailsDTO>
                {
                    success = false,
                    message = "User ID not provided",
                    data = [],
                    totalItem = 0,
                    totalNowItem = 0,
                    totalSearchItem = 0
                });

            var result = await _activityService.GetUpcomingActivityList(page, itemPagePage, search, sort, direction, model.DateRange, model.CreatedBy, model.CustomerTypeID, model.LeadStatusID);
            return Ok(result);
        }

        //=======================
        // generatePDF
        //=======================
        [HttpPost]
        public async Task<IActionResult> GeneratePDF()
        {
            var pdfBytes = await _activityService.GeneratePDF();
            return File(pdfBytes, "application/pdf", "report.pdf");
        }
    }
}
