using GCTL.Core.Repository;
using GCTL.Core.ViewModels.CRM;
using GCTL.Data.Models;
using GCTL.Service.CRM.LeadCreate;
using GCTL.Service.CRM.LeadDetail;
using GCTL.Service.Language;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers.CRM
{
    [Authorize]
    public class LeadDetailsController : BaseController
    {
        private readonly ILeadCreateService _leadCreateService;
        private readonly AppDbContext _context;
        private readonly IGenericRepository<LeadSources> _leadSourceTypeRepository;
        private readonly IGenericRepository<LeadActivityTypes> _leadActivityTypesRepository;
        private readonly IGenericRepository<LeadDetails> _leadDetailsRepository;
        private readonly IGenericRepository<LeadStatuses> _leadStatusesRepository;
        private readonly IGenericRepository<Priorities> _prioritiesRepository;
        private readonly IGenericRepository<Leads> _leadsRepository;
        private readonly ILeadDetailsService _leadDetailsService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly IGenericRepository<Services> _serviceTypeRepository;
        public LeadDetailsController(IWebHostEnvironment webHostEnvironment, IGenericRepository<LeadDetails> leadDetailsRepository, IGenericRepository<LeadActivityTypes> leadActivityTypesRepository, ILeadDetailsService leadDetailsService, IGenericRepository<LeadSources> leadSourceTypeRepository, AppDbContext context, ILeadCreateService leadCreateService, ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<LeadStatuses> leadStatusesRepository, IGenericRepository<Priorities> prioritiesRepository, IGenericRepository<Services> serviceTypeRepository, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository, IGenericRepository<Customers> customersRepository, IGenericRepository<Country> countryRepository, IGenericRepository<Leads> leadsRepository) : base(translateService, userProfileService)
        {
            _leadCreateService = leadCreateService;
            _leadSourceTypeRepository = leadSourceTypeRepository;
            _leadDetailsService = leadDetailsService;
            _leadActivityTypesRepository = leadActivityTypesRepository;
            _leadDetailsRepository = leadDetailsRepository;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
            _leadStatusesRepository = leadStatusesRepository;
            _prioritiesRepository = prioritiesRepository;
            _serviceTypeRepository = serviceTypeRepository;
            _leadsRepository = leadsRepository;
        }

        public async Task<IActionResult> Index(int? id)
        {

            await _leadDetailsService.CreateLeadActivateTypes();


            ViewBag.ServiceDD = new SelectList(_serviceTypeRepository.AllActive().Select(e => new { e.ServiceID, e.ServiceName }), "ServiceID", "ServiceName");

            ViewBag.LeadSourceDD = new SelectList(_leadSourceTypeRepository.AllActive().Select(e => new { e.LeadSourceID, e.LeadSourceName }), "LeadSourceID", "LeadSourceName");
            ViewBag.LeadActivityTypes = _leadActivityTypesRepository.AllActive().Where(e => e.UseFor == "General").Select(e => new { e.LeadActivityTypeID, e.LeadActivityIcon, e.LeadActivityName }).ToList();
            ViewBag.LeadActivityTypes2 = _leadActivityTypesRepository.AllActive().Where(e => e.UseFor == "Won" || e.UseFor == "Lost").Select(e => new { e.LeadActivityTypeID, e.LeadActivityIcon, e.LeadActivityName, e.UseFor }).ToList();
            ViewBag.LeadStatus = new SelectList(_leadStatusesRepository.AllActive().Where(u => u.IsSpecial != true).Select(e => new { e.LeadStatusID, e.LeadStatusName }), "LeadStatusID", "LeadStatusName");
            ViewBag.LeadPriorities = new SelectList(_prioritiesRepository.AllActive().Select(e => new { e.PriorityID, e.PriorityName }), "PriorityID", "PriorityName");


            var customerObj = await (from lead in _context.Leads
                                     join cAddress in _context.CustomerAddresses
                                     on lead.CustomerID equals cAddress.CustomerID
                                     join customer in _context.Customers on cAddress.CustomerID equals customer.CustomerID
                                     join address in _context.Addresses on cAddress.AddressID equals address.AddressID

                                     where lead.LeadID == id
                                     select new CustomerInfoVM
                                     {
                                         CustomerId = lead.CustomerID,
                                         BranchId = lead.CompanyBranchID,
                                         FullName = customer.FullName,
                                         LeadName = lead.LeadName,
                                         LeadID = lead.LeadID,
                                         LeadSourceID = lead.LeadSourceID ?? 0,
                                         LeadStatusID = lead.LeadStatusID ?? 0,
                                         PriorityID = lead.PriorityID ?? 0,
                                         Created = lead.CreatedAt,
                                         ApproximateDealValue = lead.ApproximateDealValue ?? 0m,
                                         Priority = lead.Priority.PriorityName,
                                         Probability = (int)(lead.ProbabilityPercentage ?? 0),
                                         Phone = address.Phone,
                                         Email = address.Email,
                                         LeadOwnerId = lead.LeadOwnerID,
                                         LeadOwnerName = lead.LeadOwner.FirstName + " " + lead.LeadOwner.LastName,
                                         Services = lead.LeadServices.Where(s => s.ServiceID.HasValue).Select(x => new SelectListItem
                                         {
                                             Value = x.ServiceID.ToString(),
                                             Text = x.Service.ServiceName,
                                         }).ToList(),
                                     }).FirstOrDefaultAsync();
            if (customerObj != null)
            {
                return View(customerObj);
            }
            else
            {
                CustomerInfoVM obj = new();
                return View(obj);
            }
        }

        public async Task<string> StorePhoto(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "media/leads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"); // e.g., 20250903154530123
            var random = new Random().Next(100, 999); // 3-digit random number
            var extension = Path.GetExtension(file.FileName); // Keep original extension
            var uniqueFileName = $"{timestamp}_{random}{extension}";

            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/media/leads/{uniqueFileName}";
        }


        [HttpPost]
        public async Task<IActionResult> UpdateLeadValue([FromForm] DetailsLeadUpdateVM detailsLeadUpdateVM)
        {
            var fieldNames = new[] { "source", "priority", "stage", "probability" };
            if (fieldNames.Contains(detailsLeadUpdateVM.FieldName))
            {
                var result = await _leadDetailsService.UpdateLeadFieldValue(detailsLeadUpdateVM);
                return Ok(result);
            }
            return Ok(false);
        }

        // geting all acitity list

        [HttpGet]
        public async Task<IActionResult> getActivityList(int id, string query, int page, string type)
        {
            var list = await _leadDetailsService.ActivityList(id, query, page, type);

            return Ok(list);
        }
        [HttpGet]
        public async Task<IActionResult> GetUpcomingActivityList(int id, int page)
        {

            const int pageSize = 10;
            int skip = (page - 1) * pageSize;
            // Fetch filtered and paginated data using LIKE
            var list = await _leadDetailsRepository
          .AllActive().Where(u => u.LeadID == id &&
                     u.ActivityDateTime >= DateTime.UtcNow.AddSeconds(11)
          )
          .OrderByDescending(e => e.ActivityDateTime)   // ORDER FIRST!
          .Skip(skip)                            // THEN skip
          .Take(pageSize)                        // THEN take
          .Select(e => new
          {
              e.LeadDetailID,
              e.ActivityDateTime,
              e.PhoneNumber,
              e.EmailAddress,
              e.ActivityNote,
              e.FileLink,
              e.LeadActivityType.LeadActivityName,
              e.LeadActivityType.LeadActivityIcon,
              CreatedByName = e.CreatedByNavigation != null
                              ? $"{e.CreatedByNavigation.FirstName} {e.CreatedByNavigation.LastName}"
                              : null
          })
          .ToListAsync();

            return Ok(list);
        }

        [HttpGet]
        public async Task<IActionResult> getUpcommingList(int id, int page)
        {
            var currentDate = DateTime.UtcNow;

            const int pageSize = 10; 
            int skip = (page - 1) * pageSize; 

            var list = await _leadDetailsRepository
            .AllActive().Where(u => u.LeadID == id && u.ActivityDateTime >= currentDate)
            .Skip(skip)
            .OrderByDescending(e => e.CreatedAt)
            .Take(pageSize).Select(e => new
            {
                e.ActivityDateTime,
                e.ActivityNote,
                e.LeadActivityType.LeadActivityName,
                e.LeadActivityType.LeadActivityIcon,
                CreatedByName = e.CreatedByNavigation != null ? $"{e.CreatedByNavigation.FirstName} {e.CreatedByNavigation.LastName}" : null
            })
            .ToListAsync();
            return Ok(list);
        }


        // ===================
        // new lead details
        // =======================
        [Permission("Create", "LeadDetails")]
        [HttpPost]
        public async Task<IActionResult> SaveLeadActivity([FromForm] LeadDetailsVM leadDetailsVM)
        {
            try
            {
                if (leadDetailsVM == null)
                    return BadRequest(new { success = false, message = "Invalid data" });

                if (leadDetailsVM.LeadID == null || leadDetailsVM.LeadID == 0)
                    return BadRequest(new { success = false, message = "LeadID is required" });

                //var leadObj = await _leadsRepository.FirstOrDefaultAsync(u => u.LeadID == leadDetailsVM.LeadID);
                //Won / Lost special case
                var existingLeadTypeObj = await _leadActivityTypesRepository.FirstOrDefaultAsync(u => u.LeadActivityTypeID == leadDetailsVM.LeadActivityTypeID);
                if (existingLeadTypeObj.UseFor == "Won" || existingLeadTypeObj.UseFor == "Lost")
                {
                    var result = await _leadDetailsService.AddIsWon(new IsWonVM
                    {
                        LeadID = leadDetailsVM.LeadID.Value,
                        LeadActivityTypeID = leadDetailsVM.LeadActivityTypeID ?? 0,
                        ActivityNote = leadDetailsVM.ActivityNote,
                        CreatedBy = leadDetailsVM.CreatedBy,
                        DeletedBy = leadDetailsVM.DeletedBy,
                    });

                    return Ok(result);
                }

                string fileLocation = leadDetailsVM.File != null
                ? await StorePhoto(leadDetailsVM.File)
                : null;

                var result2 = await _leadDetailsService.CreateLeadDeatil(leadDetailsVM, fileLocation);

                return Ok(result2);
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    success = false,
                    message =  "Something goes to wrong"
                });
            }

        }

        //==============================
        // restore lead details activity
        //==============================
        [HttpPost]
        public async Task<IActionResult> RestoreLead([FromForm]  int id)
         {
            var restult = await _leadDetailsService.RestoreLead(id);
            return Ok(restult);
        }

        #region Get GetContactPerson List
        [HttpGet]
        public async Task<IActionResult> GetContactNumberList(int leadId, string search = "", int page = 1, int pageSize = 20)
        {
            var result = await _leadCreateService.GetContactPersonNumberAsync(
               leadId, search, page, pageSize, await GetCurrentOrganizationIdAsync() ?? 0
            );
            var hasMore = (page * pageSize) < result.totalItem;
            var formatted = new
            {
                items = result.data.Select(c => new
                {
                    value = c.Name ?? "",
                    label = $"{c.Name ?? ""}",
                    group = ""
                }),
                hasMore
            };

            return Json(formatted);
        }
        #endregion
        #region Get GetContactPerson List
        [HttpGet]
        public async Task<IActionResult> GetContactEmailList(int leadId, string search = "", int page = 1, int pageSize = 20)
        {
            var result = await _leadCreateService.GetContactPersonEmailAsync(
               leadId, search, page, pageSize, await GetCurrentOrganizationIdAsync() ?? 0
            );
            var hasMore = (page * pageSize) < result.totalItem;
            var formatted = new
            {
                items = result.data.Select(c => new
                {
                    value = c.Name ?? "",
                    label = $"{c.Name ?? ""}",
                    group = ""
                }),
                hasMore
            };

            return Json(formatted);
        }
        #endregion
    }
}
