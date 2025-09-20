using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.CRM;
using GCTL.Service.CRM.LeadsActivities;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.CRM
{
    public class LeadsActivitiesController : BaseController
    {
        private readonly ILeadsActivityService _activityService;

        public LeadsActivitiesController(ITranslateService translateService, IUserProfileService userProfileService, ILeadsActivityService activityService) : base(translateService, userProfileService)
        {
            _activityService = activityService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetUpcomingActivities([FromForm] UpcomingActivityVM model)
        {
       

            int page = model.pageNumber ?? 1;
            int itemPagePage = model.itemPerPage ?? 10;
            string search = model.search ?? string.Empty;
            string sort = model.sortColumn ?? "ActivityDateTime";
            string direction = model.sortDirection ?? "asc";
            string dateTime = model.dateRange ?? string.Empty;

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

            var result = await _activityService.GetUpcomingActivityList(page, itemPagePage, search, sort, direction, model.dateRange, model.CreatedBy);
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
