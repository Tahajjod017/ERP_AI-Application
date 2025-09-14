using GCTL.Core.Repository;
using GCTL.Core.ViewModels.CRM;
using GCTL.Data.Models;
using GCTL.Service.CRM.LeadCreate;
using GCTL.Service.CRM.LeadDetails;
using GCTL.Service.Language;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol.Plugins;
using OpenQA.Selenium.Support.UI;


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
        //private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        //private readonly IGenericRepository<Customers> _customersRepository;
        //private readonly IGenericRepository<Country> _countryRepository;
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
            ViewBag.LeadActivityTypes = _leadActivityTypesRepository.AllActive().Where(e => e.UseFor == null).Select(e => new { e.LeadActivityTypeID, e.LeadActivityIcon, e.LeadActivityName }).ToList();
            ViewBag.LeadActivityTypes2 = _leadActivityTypesRepository.AllActive().Where(e => e.UseFor == "special").Select(e => new { e.LeadActivityTypeID, e.LeadActivityIcon, e.LeadActivityName }).ToList();
            ViewBag.LeadStatus = new SelectList(_leadStatusesRepository.AllActive().Select(e => new { e.LeadStatusID, e.LeadStatusName }), "LeadStatusID", "LeadStatusName");
            ViewBag.LeadPriorities = new SelectList(_prioritiesRepository.AllActive().Select(e => new { e.PriorityID, e.PriorityName }), "PriorityID", "PriorityName");


            var customerObj = await (from lead in _context.Leads
                                     join cAddress in _context.CustomerAddresses
                                     on lead.CustomerID equals cAddress.CustomerAddressID
                                     join customer in _context.Customers on cAddress.CustomerID equals customer.CustomerID
                                     join address in _context.Addresses on cAddress.AddressID equals address.AddressID

                                     where lead.LeadID == id
                                     select new CustomerInfoVM
                                     {
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
                                         LeadDescription = lead.LeadDescription,
                                         AddressTypeName = cAddress.AddressType.AddressTypeName,
                                         FullAddress = address.FullAddress,
                                         Street = address.Street,
                                         City = address.City,
                                         Additionaladdress = address.Additionaladdress,
                                         State = address.State,
                                         PostalCode = address.PostalCode,
                                         Latitude = address.Latitude,
                                         Longitude = address.Longitude,
                                         Phone = address.Phone,
                                         OtherPhone = address.OtherPhone,
                                         Email = address.Email,
                                         FirstName = address.FirstName,
                                         LastName = address.LastName,
                                         isWon = lead.IsWwn ?? null,
                                         LeadOwnerId = lead.LeadOwnerID,
                                         LeadOwnerName = lead.LeadOwner.FirstName + " " + lead.LeadOwner.LastName,
                                         ServiceIds = lead.LeadServices.Where(s => s.ServiceID.HasValue).Select(s => s.ServiceID).ToList(),
                                         ClosingDate = lead.ClosingDate,

                                         // 🔥 Stats calculation for this LeadOwner
                                         SuccessPercentage = (int)Math.Round(_context.Leads
                                         .Where(x => x.LeadOwnerID == lead.LeadOwnerID && x.IsWwn == true)
                                         .Count() * 100m /
                                         (_context.Leads.Count(x => x.LeadOwnerID == lead.LeadOwnerID) == 0 ? 1 : _context.Leads.Count(x => x.LeadOwnerID == lead.LeadOwnerID))),

                                         LostPercentage = (int)Math.Round(_context.Leads
                                         .Where(x => x.LeadOwnerID == lead.LeadOwnerID && x.IsWwn == false)
                                         .Count() * 100m /
                                         (_context.Leads.Count(x => x.LeadOwnerID == lead.LeadOwnerID) == 0 ? 1 : _context.Leads.Count(x => x.LeadOwnerID == lead.LeadOwnerID))),

                                         CancelPercentage = (int)Math.Round(_context.Leads
                                         .Where(x => x.LeadOwnerID == lead.LeadOwnerID && x.IsWwn == null)
                                         .Count() * 100m /
                                         (_context.Leads.Count(x => x.LeadOwnerID == lead.LeadOwnerID) == 0 ? 1 : _context.Leads.Count(x => x.LeadOwnerID == lead.LeadOwnerID)))

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


        // update source field value 
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
            int leadDetailsTypeID = 0;
            if (!string.IsNullOrEmpty(type))
            {
                var leadDetailsTypeObj = await _leadActivityTypesRepository.FirstOrDefaultAsync(u => u.LeadActivityName == type);
                leadDetailsTypeID = leadDetailsTypeObj.LeadActivityTypeID;
            }

            const int pageSize = 10; // Number of items per page
            int skip = (page - 1) * pageSize; // Calculate how many items to skip

            // Fetch filtered and paginated data using LIKE
            var list = await _leadDetailsRepository
          .Find(u => u.LeadID == id &&
                     (leadDetailsTypeID == 0 || u.LeadActivityTypeID == leadDetailsTypeID) &&
                     (string.IsNullOrEmpty(query)
                      || EF.Functions.Like(u.ActivityDateTime.ToString(), $"%{query}%")
                      || EF.Functions.Like(u.ActivityNote, $"%{query}%")
                      || EF.Functions.Like(u.LeadActivityType.LeadActivityName, $"%{query}%")
                     )
          )
          .OrderByDescending(e => e.ActivityDateTime)   // ORDER FIRST!
          .Skip(skip)                            // THEN skip
          .Take(pageSize)                        // THEN take
          .Select(e => new
          {
              e.LeadDetailID,
              e.ActivityDateTime,
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
        public async Task<IActionResult> GetUpcomingActivityList(int id, int page)
        {

            const int pageSize = 10;
            int skip = (page - 1) * pageSize;
            // Fetch filtered and paginated data using LIKE
            var list = await _leadDetailsRepository
          .Find(u => u.LeadID == id &&
                     u.ActivityDateTime >= DateTime.UtcNow
          )
          .OrderByDescending(e => e.ActivityDateTime)   // ORDER FIRST!
          .Skip(skip)                            // THEN skip
          .Take(pageSize)                        // THEN take
          .Select(e => new
          {
              e.LeadDetailID,
              e.ActivityDateTime,
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

            const int pageSize = 10; // Number of items per page
            int skip = (page - 1) * pageSize; // Calculate how many items to skip

            // Fetch filtered and paginated data using LIKE
            var list = await _leadDetailsRepository
            .Find(u => u.LeadID == id && u.ActivityDateTime >= currentDate)
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

        // TODO: update Lead

        [HttpPost]
        public async Task<IActionResult> EditLeadData([FromBody] LeadUpdateVM leadUpdateVM)
        {
            if (ModelState.IsValid)
            {
                if (leadUpdateVM.LeadID != 0)
                {
                    var result = await _leadCreateService.EditLead(leadUpdateVM);
                    return Ok(result);
                }
            }
            var results = new ReturnView
            {
                Success = false,
                Message = "Data not inserted",
            };
            return Ok(results);

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

                var leadObj = await _leadsRepository.FirstOrDefaultAsync(u => u.LeadID == leadDetailsVM.LeadID);

                if (leadObj != null && leadObj.IsWwn != null)
                {
                    leadObj.IsWwn = null;
                    leadObj.ClosingDate = null;
                    await _leadsRepository.UpdateAsync(leadObj);
                }
                //Won / Lost special case
                var existingLeadTypeObj = await _leadActivityTypesRepository.FirstOrDefaultAsync(u => u.LeadActivityTypeID == leadDetailsVM.LeadActivityTypeID);
                if (existingLeadTypeObj.LeadActivityName == "Won" || existingLeadTypeObj.LeadActivityName == "Lost")
                {
                    var result = await _leadDetailsService.AddIsWon(new IsWonVM
                    {
                        LeadID = leadDetailsVM.LeadID.Value,
                        LeadActivityTypeID = leadDetailsVM.LeadActivityTypeID ?? 0,
                        ActivityNote = leadDetailsVM.ActivityNote,
                        CreatedBy = leadDetailsVM.CreatedBy,

                    });

                    return Ok(result);
                }

                string fileLocation = leadDetailsVM.File != null
                ? await StorePhoto(leadDetailsVM.File)
                : null;

                bool created = await _leadDetailsService.CreateLeadDeatil(leadDetailsVM, fileLocation);

                return Ok(new
                {
                    success = true,
                    message = created ? "Data added successfully" : "Failed to add lead details"
                });
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
    }
}
