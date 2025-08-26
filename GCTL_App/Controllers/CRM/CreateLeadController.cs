using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
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
        private readonly IGenericRepository<CustomerAddresses> _customerAddressesRepository;
        private readonly IGenericRepository<Addresses> _addressesRepository;
        private readonly AppDbContext _context;
        private readonly IGenericRepository<Country> _countryRepository;

        #endregion
        public CreateLeadController(AppDbContext context, ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<Services> serviceTypeRepository, IGenericRepository<LeadSources> leadSourceTypeRepository, IGenericRepository<Customers> customersRepository, IGenericRepository<CustomerAddresses> customerAddressesRepository, IGenericRepository<GCTL.Data.Models.Employees> employeeTypeRepository, IGenericRepository<Country> countryRepository, IGenericRepository<Addresses> addressesRepository, IGenericRepository<LeadStatuses> leadStatusesTypeRepository = null, ILeadCreateService leadCreateService = null) : base(translateService, userProfileService)
        {
            _serviceTypeRepository = serviceTypeRepository;
            _leadSourceTypeRepository = leadSourceTypeRepository;
            _leadStatusesTypeRepository = leadStatusesTypeRepository;
            _customersRepository = customersRepository;
            _leadCreateService = leadCreateService;
            _customerAddressesRepository = customerAddressesRepository;
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
            var customerObj = await _customerAddressesRepository
                .FirstOrDefaultAsync(u => u.CustomerAddressID == id);

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
            var customers = await _customerAddressesRepository
                    .Find(u => u.AddressType.AddressTypeName == "billing" || u.AddressType.AddressTypeName == "company")
                    .OrderBy(n => n.Customer.FullName)
                    .Select(n => new
                    {
                        CustomerId = n.CustomerAddressID,
                        FullName = n.Customer.FullName,
                        Type = n.AddressType.AddressTypeName,
                        Phone = n.Address.Phone,
                        Email = n.Address.Email
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
        public async Task<IActionResult> getCompanyList()
        {

            var companyList = await _customerAddressesRepository.AllActive().Where(u=> u.AddressType.AddressTypeName == "company").Include(c=>c.Customer).ToListAsync();
            var companySelectList = companyList
                .Where(c => c.Customer != null) // ensure Customer exists
                .Select(c => new SelectListItem
                {
                    Value = c.CustomerAddressID.ToString(),
                    Text = c.Customer.FullName
                }).ToList();

            if (companySelectList.Any())
                return Json(companySelectList);

            return BadRequest("No companies found.");
        }

        public async Task<IActionResult> getPersonList([FromBody] string query)
        {
 
            if (string.IsNullOrWhiteSpace(query))
                return Json(new List<SelectListItem>());

            var personList = await _customerAddressesRepository
                .AllActive()
                .Where(u => u.AddressType.AddressTypeName == "billing")
                .Include(c => c.Customer)
                .Where(c => c.Customer.FullName.Contains(query)) 
                .OrderBy(c => c.Customer.FullName)               
                .Take(5)                                        
                .ToListAsync();

            var personSelectList = personList
                .Where(c => c.Customer != null)
                .Select(c => new SelectListItem
                {
                    Value = c.CustomerAddressID.ToString(),
                    Text = c.Customer.FullName
                })
                .ToList();

            if (personSelectList.Any())
                return Json(personSelectList);

            return BadRequest("No companies found.");
        }


        [HttpPost]
        public async Task<IActionResult> GetCustomerInfo([FromBody]  int id)
        {
            var customerObj = await (from add in _context.CustomerAddresses
                                     join ind in _context.Customers
                                     on add.CustomerID equals ind.CustomerID
                                     join address in _context.Addresses on add.AddressID equals address.AddressID
                                     join country in _context.Country on address.CountryID equals country.CountryID
                                     where add.CustomerAddressID == id
                                     select new
                                     {
                                         add.CustomerAddressID,
                                         add.AddressType.AddressTypeName,
                                         ind.FullName,
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
                                         address.LastName,
                                     }).FirstOrDefaultAsync();

            var customerId = await _context.CustomerAddresses.Where(x => x.CustomerAddressID == id).Select(x => x.CustomerAddressID).FirstOrDefaultAsync();
            var shippingObj = await (from add in _context.CustomerAddresses
                                     join address in _context.Addresses on add.AddressID equals address.AddressID
                                     join country in _context.Country on address.CountryID equals country.CountryID
                                     where add.AddressType.AddressTypeName == "shipping" && add.CustomerID == customerId
                                     select new
                                     {
                                         add.CustomerAddressID,
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
        public async Task<IActionResult> InsertPerson([FromBody] CustomerVM customerVM)
        {
            if (ModelState.IsValid)
            {
                if (customerVM.PrimaryID == 0)
                {
                    var result = await _leadCreateService.CreatePerson(customerVM);
                    
                    return Ok(result);
                } 
            }
            var results =  new ReturnView
            {
                Success = false,
                Message = "Data not inserted",
            };
            return Ok(results); 
        } 
        [HttpPost]
        public async Task<IActionResult> InsertCompany([FromBody] CompanyVM companyVM)
        {
            if (ModelState.IsValid)
            {
                if (companyVM.PrimaryID == 0)
                {
                    var result = await _leadCreateService.CreateCompany(companyVM);
                    
                    return Ok(result);
                } 
            }
            var results =  new ReturnView
            {
                Success = false,
                Message = "Data not inserted",
            };
            return Ok(results); 
        } 

        public async Task<IActionResult> InsertBranch([FromBody] BranchVM branchVM)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    if (branchVM.PrimaryID == 0)
                    {
                        var result = await _leadCreateService.CreateBranch(branchVM);
                        return Ok(result);
                    }
                }
                return Ok(false);
            }
            catch (Exception)
            {
                return Ok(false);
            }
            
        } 
        public async Task<IActionResult> InsertWarehouse([FromBody] WarehouseVM warehouseVM)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    if (warehouseVM.PrimaryID == 0)
                    {
                        var result = await _leadCreateService.CreateWarehouse(warehouseVM);
                        return Ok(result);
                    }
                }
                return Ok(false);
            }
            catch (Exception)
            {
                return Ok(false);
            }
            
        } 

        [HttpPost]
        public async Task<IActionResult> InsertShippingAddress([FromBody] ShippingVM shippingVM)
        {
            if (ModelState.IsValid)
            {
                if (shippingVM.PrimaryID == 0)
                {
                    var result = await _leadCreateService.CreateShippingAddress(shippingVM);
                    
                    return Ok(result);
                } 
            }
            return Ok(false); 
        }
        [HttpPost]
        public async Task<IActionResult> CreateLeadData([FromBody] LeadsVM leadsVM)
        {
            if (ModelState.IsValid)
            {
                if (leadsVM.CustomerId != 0)
                {
                    var result = await _leadCreateService.CreateLead(leadsVM);
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

    }
}
