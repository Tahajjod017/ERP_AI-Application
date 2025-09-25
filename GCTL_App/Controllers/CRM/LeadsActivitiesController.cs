using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.MasterSetup.LeadStatuses;
using GCTL.Data.Models;
using GCTL.Service.CRM;
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
        private readonly IGenericRepository<LeadActivityTypes> _leadActivityTypeService;


        public LeadsActivitiesController(ITranslateService translateService, IUserProfileService userProfileService, ILeadsActivityService activityService, IGenericRepository<LeadStatuses> leadStatusesRepository, IGenericRepository<Services> servicesRepository, IGenericRepository<AddressTypes> addressTypeService, IGenericRepository<LeadActivityTypes> leadActivityTypeService) : base(translateService, userProfileService)
        {
            _activityService = activityService;
            _leadStatusesRepository = leadStatusesRepository;
            _addressTypeService = addressTypeService;
            _leadActivityTypeService = leadActivityTypeService;
        }

        public IActionResult Index()
        {

            ViewBag.LeadStatus2 = new SelectList(_leadStatusesRepository.AllActive().Select(e => new { e.LeadStatusID, e.LeadStatusName }), "LeadStatusID", "LeadStatusName");
            ViewBag.ServiceTypeDD = new SelectList(_addressTypeService.AllActive().Where(u => u.AddressTypeName == "individual" || u.AddressTypeName == "company").Select(e => new { e.AddressTypeID, e.AddressTypeName }), "AddressTypeID", "AddressTypeName");
            ViewBag.ActivityTypeDD = new SelectList(_leadActivityTypeService.AllActive().Select(e => new { e.LeadActivityTypeID, e.LeadActivityName }), "LeadActivityTypeID", "LeadActivityName");
            return View();
        }
        public async Task<bool> SendEmailWithPdf(byte[] pdfBytes)
        {
            try
            {
                var emailService = new EmailService1();

                string receiverEmail = "debanjandevelopment@gmail.com";
                string subject = "Hello from Gmail SMTP";
                string body = "<h2>This email is sent using EmailService with PDF attachment!</h2>";

                // Pass PDF as attachment
                await emailService.SendEmailAsync(receiverEmail, subject, body, pdfBytes, "report.pdf");

                Console.WriteLine("✅ Email sent successfully with PDF!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Email sending failed: {ex.Message}");
                return false;
            }
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

            var result = await _activityService.GetUpcomingActivityList(page, itemPagePage, search, sort, direction, model.DateRange, model.CreatedBy, model.CustomerTypeID, model.LeadStatusID, model.ActivityTypeID);
            return Ok(result);
        }

        //=======================
        // generatePDF
        //=======================
        [HttpPost]
        public async Task<IActionResult> GeneratePDF()
        {

            var pdfBytes = await _activityService.GeneratePDF();
            await SendEmailWithPdf(pdfBytes);
            return File(pdfBytes, "application/pdf", "report.pdf");
        }
    }
}
