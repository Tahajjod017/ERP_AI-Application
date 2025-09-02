using GCTL.Core.Repository;
using GCTL.Core.ViewModels.CRM;
using GCTL.Data.Models;
using GCTL.Service.CRM.LeadCreate;
using GCTL.Service.CRM.LeadDetails;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.JsonPatch.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;


namespace GCTL_App.Controllers.CRM
{
    public class LeadDetailsController : BaseController
    {
        private readonly ILeadCreateService _leadCreateService;
        private readonly AppDbContext _context;
        private readonly IGenericRepository<LeadSources> _leadSourceTypeRepository;
        private readonly IGenericRepository<LeadActivityTypes> _leadActivityTypesRepository;
        private readonly IGenericRepository<LeadDetails> _leadDetailsRepository;
        private readonly ILeadDetailsService _leadDetailsService;
        public LeadDetailsController(IGenericRepository<LeadDetails> leadDetailsRepository, IGenericRepository<LeadActivityTypes> leadActivityTypesRepository, ILeadDetailsService leadDetailsService, IGenericRepository<LeadSources> leadSourceTypeRepository, AppDbContext context, ILeadCreateService leadCreateService, ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {
            _leadCreateService = leadCreateService;
            _leadSourceTypeRepository = leadSourceTypeRepository;
            _leadDetailsService = leadDetailsService;
            _leadActivityTypesRepository = leadActivityTypesRepository;
            _leadDetailsRepository = leadDetailsRepository;
            _context = context;
        }

        public async Task<IActionResult> Index(int? id)
        {

            await _leadDetailsService.CreateLeadActivateTypes();


            ViewBag.LeadSourceDD = new SelectList(_leadSourceTypeRepository.AllActive().Select(e => new { e.LeadSourceID, e.LeadSourceName }), "LeadSourceID", "LeadSourceName");
            ViewBag.LeadActivityTypes = _leadActivityTypesRepository.AllActive().Select(e => new { e.LeadActivityTypeID, e.LeadActivityIcon, e.LeadActivityName }).ToList();
            
            var customerObj = await(from lead in _context.Leads
                                    join cAddress in _context.CustomerAddresses
                                    on lead.CustomerID equals cAddress.CustomerAddressID
                                    join customer in _context.Customers on cAddress.CustomerID equals customer.CustomerID
                                    join address in _context.Addresses on cAddress.AddressID equals address.AddressID
                                    //join country in _context.Country on address.CountryID equals country.CountryID into countryGroup
                                    //from country in countryGroup.DefaultIfEmpty()
                                    where lead.LeadID == id
                                    select new CustomerInfoVM
                                    {
                                        FullName = customer.FullName,
                                        LeadName = lead.LeadName,
                                        LeadID = lead.LeadID,
                                        Created = lead.CreatedAt,
                                        Probability =  lead.ProbabilityPercentage,
                                        AddressTypeName = cAddress.AddressType.AddressTypeName,
                                        FullAddress = address.FullAddress,
                                        Street = address.Street,
                                        City = address.City,
                                        Additionaladdress = address.Additionaladdress,
                                        State = address.State,
                                        PostalCode = address.PostalCode,
                                        //CountryID = country != null ? country.CountryID : 0,
                                        //CountryCode = country != null ? country.CountryCode : null,
                                        Latitude = address.Latitude,
                                        Longitude = address.Longitude,
                                        Phone = address.Phone,
                                        OtherPhone = address.OtherPhone,
                                        Email = address.Email,
                                        FirstName = address.FirstName,
                                        LastName = address.LastName

                                    }).FirstOrDefaultAsync();
            if (customerObj != null)
            {
                return View(customerObj);
            } else
            {
                CustomerInfoVM obj = new ();
                return View(obj);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CeateLeadDetail([FromBody] LeadDetailsVM leadDetailsVM)
        {
            try
            {

                if (leadDetailsVM.LeadID != 0)
                {
                    bool result = await _leadDetailsService.CreateLeadDeatil(leadDetailsVM);
                    return Ok(new { result = result, message = "Data added successfully" });
                }
            }
            catch (Exception)
            {
                return Ok(new { result = false, message = "Failed to add lead details. Please check the input data." });

            }
            return Ok(new { result = false, message = "Failed to add lead details. Please check the input data." });
        }


        [HttpGet]
        public async Task<IActionResult> getActivityInfo([FromBody] LeadDetailsVM leadDetailsVM)
        {
            try
            {

                if (leadDetailsVM.LeadID != 0)
                {
                    bool result = await _leadDetailsService.CreateLeadDeatil(leadDetailsVM);
                    return Ok(new { result = result, message = "Data added successfully" });
                }
            }
            catch (Exception)
            {
                return Ok(new { result = false, message = "Failed to add lead details. Please check the input data." });

            }
            return Ok(new { result = false, message = "Failed to add lead details. Please check the input data." });
        }


        // geting all acitity list

        [HttpGet]
        public async Task<IActionResult> getActivityList(int id, string query, int page, string type)
        {
            const int pageSize = 10; // Number of items per page
            int skip = (page - 1) * pageSize; // Calculate how many items to skip

            // Fetch filtered and paginated data using LIKE
            var list = await _leadDetailsRepository
            .Find(u => u.LeadID == id && (string.IsNullOrEmpty(query)
                || EF.Functions.Like(u.ActivityDateTime.ToString(), $"%{query}%")
                || EF.Functions.Like(u.ActivityNote, $"%{query}%")
                || EF.Functions.Like(u.LeadActivityType.LeadActivityName, $"%{query}%")
                ))
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
    }
}
