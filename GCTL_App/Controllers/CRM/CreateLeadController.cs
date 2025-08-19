using GCTL.Core.Repository;
using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.MasterSetup.ServiceType;
using GCTL.Data.Models;
using GCTL.Service.CRM.LeadCreate;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using SkiaSharp;


namespace GCTL_App.Controllers.CRM
{
    public class CreateLeadController : BaseController
    {
        #region CTOR
        private readonly IGenericRepository<Services> _serviceTypeRepository;
        private readonly IGenericRepository<LeadSources> _leadSourceTypeRepository;
        private readonly IGenericRepository<LeadStatuses> _leadStatusesTypeRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeTypeRepository;
        private readonly IGenericRepository<Customers> _customersRepository;
        private readonly ILeadCreateService _leadCreateService;
        private readonly IGenericRepository<IndividualAddresses> _individualAddressesRepository;
        private readonly IGenericRepository<Addresses> _addressesRepository;
        private readonly AppDbContext _context;
        private readonly IGenericRepository<Country> _countryRepository;

        #endregion
        public CreateLeadController(AppDbContext context, ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<Services> serviceTypeRepository, IGenericRepository<LeadSources> leadSourceTypeRepository, IGenericRepository<Customers> customersRepository, IGenericRepository<IndividualAddresses> individualAddressesRepository, IGenericRepository<GCTL.Data.Models.Employees> employeeTypeRepository, IGenericRepository<Country> countryRepository, IGenericRepository<Addresses> addressesRepository, IGenericRepository<LeadStatuses> leadStatusesTypeRepository = null, ILeadCreateService leadCreateService = null) : base(translateService, userProfileService)
        {
            _serviceTypeRepository = serviceTypeRepository;
            _leadSourceTypeRepository = leadSourceTypeRepository;
            _leadStatusesTypeRepository = leadStatusesTypeRepository;
            _customersRepository = customersRepository;
            _leadCreateService = leadCreateService;
            _individualAddressesRepository = individualAddressesRepository;
            _employeeTypeRepository = employeeTypeRepository;
            _countryRepository = countryRepository;
            _addressesRepository = addressesRepository;
            _context = context;
        }

        public async Task<IActionResult> index()
        {
            SetSmartPageCode(600100);

            ViewBag.ServiceDD = new SelectList(_serviceTypeRepository.AllActive().Select(e => new { e.ServiceID, e.ServiceName }), "ServiceID", "ServiceName");
            ViewBag.LeadSourceDD = new SelectList(_leadSourceTypeRepository.AllActive().Select(e => new { e.LeadSourceID, e.LeadSourceName }), "LeadSourceID", "LeadSourceName");
            ViewBag.LeadStatusDD = new SelectList(_leadStatusesTypeRepository.AllActive().Select(e => new { e.LeadStatusID, e.LeadStatusName }), "LeadStatusID", "LeadStatusName");
            ViewBag.EmployeeDD = new SelectList(_employeeTypeRepository.AllActive().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }), "EmployeeID", "FullName");
            ViewBag.CountryDD = new SelectList(_countryRepository.AllActive().Select(e => new { e.CountryID, e.CountryName }), "CountryID", "CountryName");


            return View();
        }

       

        [HttpGet]
        private async Task<bool> IsUniqueAsync(string queryText, string type, int id)
        {
            if (string.IsNullOrEmpty(queryText) || string.IsNullOrEmpty(type))
                return false;

            int? addressId = 0;
            var customerObj = await _individualAddressesRepository
                .FirstOrDefaultAsync(u => u.IndividualAddressID == id);

            if (customerObj != null)
                addressId = customerObj.AddressID;

            if (type == "phone")
            {
                var queryResult = await _addressesRepository
                    .FindAsync(u => (u.Phone == queryText || u.OtherPhone == queryText) && u.AddressID != addressId);
                return queryResult.Count == 0;
            }
            else if (type == "email")
            {
                var queryResult = await _addressesRepository
                    .FindAsync(u => u.Email == queryText && u.AddressID != addressId);
                return queryResult.Count == 0;
            }

            return false;
        }


        public async Task<IActionResult> UniquenessCheck(string queryText, string type, int id)
        {
            var isUnique = await IsUniqueAsync(queryText, type, id);
            return Json(new { unique = isUnique });
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerList()
        {
            var customers = await _individualAddressesRepository
                    .Find(u => u.AddressType.AddressTypeName == "billing")
                    .OrderBy(n => n.Individual.FirstName + n.Individual.LastName)
                    .Select(n => new
                    {
                        CustomerId = n.IndividualAddressID,
                        FullName = n.Individual.FirstName + " " + n.Individual.LastName,
                        Type = n.AddressType.AddressTypeName,
                        Phone = n.Address.Phone
                    })
                    .ToListAsync();

            return Json(customers);
        }

        [HttpGet]
        public async Task<IActionResult> addCountry(string countryName)
        {
            var countryObj = await _countryRepository.AllActive().Where(e => e.CountryName.Trim().ToLower() == countryName.Trim().ToLower()).FirstOrDefaultAsync();
            if (countryObj == null)
            {
                countryObj = new Country()
                {
                    CountryName = countryName
                };
                await _countryRepository.AddAsync(countryObj);
            }

            return Json(new {countryId = countryObj.CountryID , countryName= countryObj.CountryName});
        }
        [HttpGet]
        public async Task<IActionResult> getCountry(string countryName)
        {
            var countryList = await _countryRepository.GetAllAsync();
            var countrySelectList = countryList.Select(c => new SelectListItem
            {
                Value = c.CountryID.ToString(),
                Text = c.CountryName
            }).ToList();
            return Json(countrySelectList);
        }
        [HttpPost]
        public async Task<IActionResult> GetCustomerInfo([FromBody]  int id)
        {
            var customerObj = await (from add in _context.IndividualAddresses
                                     join ind in _context.Individuals
                                     on add.IndividualID equals ind.IndividualID
                                     join address in _context.Addresses on add.AddressID equals address.AddressID
                                     join country in _context.Country on address.CountryID equals country.CountryID
                                     where add.IndividualAddressID == id
                                     select new
                                     {
                                         add.IndividualAddressID,
                                         add.AddressType.AddressTypeName,
                                         ind.FirstName,
                                         ind.LastName,
                                         address.FullAddress,
                                         address.Street,
                                         address.City,
                                         address.Additionaladdress,
                                         address.State,
                                         address.PostalCode,
                                         country.CountryName,
                                         country.CountryCode,
                                         address.Latitude,
                                         address.Longitude,
                                         address.Phone,
                                         address.OtherPhone,
                                         address.Email
                                     }).FirstOrDefaultAsync();

            var individualId = await _context.IndividualAddresses.Where(x => x.IndividualAddressID == id).Select(x => x.IndividualID).FirstOrDefaultAsync();
            var shippingObj = await (from add in _context.IndividualAddresses
                                     join address in _context.Addresses on add.AddressID equals address.AddressID
                                     join country in _context.Country on address.CountryID equals country.CountryID
                                     where add.AddressType.AddressTypeName == "shipping" && add.IndividualID == individualId
                                     select new
                                     {
                                         add.IndividualAddressID,
                                         address.FullAddress,
                                         address.Street,
                                         address.City,
                                         address.Additionaladdress,
                                         address.State,
                                         address.PostalCode,
                                         country.CountryName,
                                         country.CountryCode,
                                         address.Latitude,
                                         address.Longitude,
                                         address.Phone,
                                         address.OtherPhone,
                                         address.Email,
                                         address.FirstName,
                                         address.LastName
                                     }).FirstOrDefaultAsync();

            return Json(new { customer = customerObj, shipping = shippingObj });
        }


        [HttpPost]
        public async Task<IActionResult> upsertPerson([FromBody] CustomerVM customerVM)
        {
            if (ModelState.IsValid)
            {
                if (customerVM.Customers[0].PrimaryID == 0)
                {
                    var result = await _leadCreateService.CreateLead(customerVM);
                    


                    return Json(new { success = true, message = "Saved successfully", result= result });
                } 
            }
            return Json(new { MessageContent = "Error" });

        } 
        public async Task<IActionResult> CreateLead([FromBody] LeadsVM leadsVM)
        {
            if (ModelState.IsValid && leadsVM != null)
            {
                var isUnique = await IsUniqueAsync(
                    leadsVM.Customers[0].Phone,
                    "phone",
                    leadsVM.Customers[0].PrimaryID
                );
                if (leadsVM.Customers[0].PrimaryID != 0 && isUnique)
                {

                    var result = await _leadCreateService.UpdateLead(leadsVM);
                    return Ok(result);
                }
            }
            return Json(new { MessageContent = "Error" });

        }

    }
}
