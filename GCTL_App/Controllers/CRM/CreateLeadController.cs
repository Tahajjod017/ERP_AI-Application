using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.CRM;
using GCTL.Data.Models;
using GCTL.Service.CRM.LeadCreate;
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
    public class CreateLeadController : BaseController
    {
        #region Repositories & Services
        private readonly IGenericRepository<Services> _serviceTypeRepository;
        private readonly IGenericRepository<LeadSources> _leadSourceTypeRepository;
        private readonly IGenericRepository<LeadStatuses> _leadStatusesTypeRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<Customers> _customersRepository;
        private readonly ILeadCreateService _leadCreateService;
        private readonly IGenericRepository<CustomerAddresses> _customerAddressesRepository;
        private readonly IGenericRepository<Addresses> _addressesRepository;
        private readonly AppDbContext _context;
        private readonly IGenericRepository<Country> _countryRepository;
        private readonly IGenericRepository<Priorities> _prioritiesRepository;


        public CreateLeadController(AppDbContext context, ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<Services> serviceTypeRepository, IGenericRepository<LeadSources> leadSourceTypeRepository, IGenericRepository<Customers> customersRepository, IGenericRepository<CustomerAddresses> customerAddressesRepository, IGenericRepository<GCTL.Data.Models.Employees> employeeTypeRepository, IGenericRepository<Country> countryRepository, IGenericRepository<Addresses> addressesRepository, IGenericRepository<LeadStatuses> leadStatusesTypeRepository = null, ILeadCreateService leadCreateService = null, IGenericRepository<Priorities> prioritiesRepository = null) : base(translateService, userProfileService)
        {
            _serviceTypeRepository = serviceTypeRepository;
            _leadSourceTypeRepository = leadSourceTypeRepository;
            _leadStatusesTypeRepository = leadStatusesTypeRepository;
            _customersRepository = customersRepository;
            _leadCreateService = leadCreateService;
            _customerAddressesRepository = customerAddressesRepository;
            _employeeRepository = employeeTypeRepository;
            _countryRepository = countryRepository;
            _addressesRepository = addressesRepository;
            _context = context;
            _prioritiesRepository = prioritiesRepository;
        }
        #endregion


        #region Index
        public async Task<IActionResult> index()
        {
            SetSmartPageCode(600100);
            ViewBag.CountryDD = new SelectList(_countryRepository.AllActive().Select(e => new { e.CountryID, e.CountryName }), "CountryID", "CountryName");
            return View();
        }
        #endregion

        //#region IsUniqueAsync
        //[HttpGet]
        //private async Task<bool> IsUniqueAsync(string queryText, string type, int id)
        //{
        //    if (string.IsNullOrEmpty(queryText) || string.IsNullOrEmpty(type))
        //        return false;

        //    int? addressId = 0;
        //    var customerObj = await _customerAddressesRepository
        //        .FirstOrDefaultAsync(u => u.CustomerAddressID == id);

        //    if (customerObj != null)
        //        addressId = customerObj.AddressID;

        //    if (type == "phone")
        //    {
        //        var queryResult = await _customerAddressesRepository
        //            .FindAsync(u => (u.Address.Phone == queryText || u.Address.OtherPhone == queryText) && (u.AddressType.AddressTypeName == "billing" || u.AddressType.AddressTypeName == "company") && u.AddressID != addressId);


        //        //var queryResult = await _addressesRepository
        //        //    .FindAsync(u => (u.Phone == queryText || u.OtherPhone == queryText) && u.AddressID != addressId);


        //        return queryResult.Count == 0;
        //    }
        //    else if (type == "email")
        //    {
        //        var queryResult = await _customerAddressesRepository
        //           .FindAsync(u => u.Address.Email == queryText  && (u.AddressType.AddressTypeName == "billing" || u.AddressType.AddressTypeName == "company") && u.AddressID != addressId);
        //        return queryResult.Count == 0;
        //    }

        //    return false;
        //}
        //#endregion

        //#region UniquenessCheck
        //public async Task<IActionResult> UniquenessCheck(string queryText, string type, int id)
        //{
        //    var isUnique = await IsUniqueAsync(queryText, type, id);
        //    return Json(new { unique = isUnique });
        //}
        //#endregion

        //#region GetCustomerList
        //[HttpGet]
        //public async Task<IActionResult> GetCustomerList()
        //{
        //    var customers = await _customerAddressesRepository
        //            .Find(u => u.AddressType.AddressTypeName == "individual" || u.AddressType.AddressTypeName == "company")
        //            .OrderBy(n => n.Customer.FullName)
        //            .Select(n => new
        //            {
        //                CustomerId = n.CustomerAddressID,
        //                FullName = n.Customer.FullName,
        //                Type = n.AddressType.AddressTypeName,
        //                Phone = n.Address.Phone,
        //                Email = n.Address.Email
        //            })
        //            .ToListAsync();

        //    return Json(customers);
        //}
        //#endregion

        //#region addCountry
        //[HttpGet]
        //public async Task<IActionResult> addCountry(string countryName)
        //{
        //    var countryObj = await _countryRepository.AllActive().Where(e => e.CountryName.Trim().ToLower() == countryName.Trim().ToLower()).FirstOrDefaultAsync();
        //    if (countryObj == null)
        //    {
        //        countryObj = new Country()
        //        {
        //            CountryName = countryName
        //        };
        //        await _countryRepository.AddAsync(countryObj);
        //    }

        //    return Json(new {countryId = countryObj.CountryID , countryName= countryObj.CountryName});
        //}
        //#endregion


        //#region getCountry
        //[HttpGet]
        //public async Task<IActionResult> getCountry(string countryName)
        //{
        //    var countryList = await _countryRepository.GetAllAsync();
        //    var countrySelectList = countryList.Select(c => new 
        //    {
        //        id = c.CountryID,
        //        name = c.CountryName
        //    }).ToList();
        //    return Json(countrySelectList);
        //}
        //#endregion


        //#region getCompanyList
        //[HttpPost]
        //public async Task<IActionResult> getCompanyList()
        //{

        //    var companyList = await _customerAddressesRepository.AllActive().Where(u=> u.AddressType.AddressTypeName == "company").Include(c=>c.Customer).ToListAsync();
        //    var companySelectList = companyList
        //        .Where(c => c.Customer != null) // ensure Customer exists
        //        .Select(c => new SelectListItem
        //        {
        //            Value = c.CustomerAddressID.ToString(),
        //            Text = c.Customer.FullName
        //        }).ToList();

        //    if (companySelectList.Any())
        //        return Json(companySelectList);

        //    return BadRequest("No companies found.");
        //}
        //#endregion


        //#region GetPersonList
        //[HttpPost]
        //public async Task<IActionResult> GetPersonList([FromBody] string query)
        //{
        //    if (string.IsNullOrWhiteSpace(query))
        //        return Json(new List<SelectListItem>());

        //    query = query.Trim();

        //    var results = await _customerAddressesRepository
        //        .AllActive()
        //        .Where(u => u.AddressType.AddressTypeName == "billing")
        //        .AsNoTracking()
        //        .Select(c => new
        //        {
        //            Id = c.CustomerAddressID,
        //            Name = c.Customer.FullName,
        //            Email = c.Address.Email,
        //            Phone = c.Address.Phone
        //        })
        //        .Where(x => x.Name != null && EF.Functions.Like(x.Name, $"%{query}%"))
        //        .OrderByDescending(x => x.Name == query)                   // exact match first
        //        .ThenByDescending(x => EF.Functions.Like(x.Name, $"{query}%")) // then starts with
        //        .ThenBy(x => x.Name)                                      // then alphabetical
        //        .Take(5)
        //        .ToListAsync();

        //    return Json(results);
        //}
        //#endregion

        //#region getAllCustomerList
        //[HttpPost]
        //public async Task<IActionResult> getAllCustomerList([FromBody] string query)
        //{
        //    if (string.IsNullOrWhiteSpace(query))
        //        return Json(new List<SelectListItem>());

        //    query = query.Trim();

        //    var results = await _customerAddressesRepository
        //        .AllActive()
        //        .Where(u => u.AddressType.AddressTypeName == "company" | u.AddressType.AddressTypeName == "billing")
        //        .AsNoTracking()
        //        .Select(c => new
        //        {
        //            Id = c.CustomerAddressID,
        //            Name = c.Customer.FullName,
        //            Email = c.Address.Email,
        //            Phone = c.Address.Phone
        //        })
        //        .Where(x =>
        //            (x.Name != null && EF.Functions.Like(x.Name, $"%{query}%")) ||
        //            (x.Email != null && EF.Functions.Like(x.Email, $"%{query}%")) ||
        //            (x.Phone != null && EF.Functions.Like(x.Phone, $"%{query}%"))
        //        )
        //        .OrderByDescending(x => x.Name == query)                   // exact match first
        //        .ThenByDescending(x => EF.Functions.Like(x.Name, $"{query}%")) // then starts with
        //        .ThenBy(x => x.Name)                                      // then alphabetical
        //        .Take(5)
        //        .ToListAsync();

        //    return Json(results);
        //}
        //#endregion

        //#region getCompnayList
        //[HttpPost]
        //public async Task<IActionResult> getCompnayList([FromBody] string query)
        //{
        //    if (string.IsNullOrWhiteSpace(query))
        //        return Json(new List<SelectListItem>());

        //    query = query.Trim();

        //    var results = await _customerAddressesRepository
        //        .AllActive()
        //        .Where(u => u.AddressType.AddressTypeName == "company")
        //        .AsNoTracking()
        //        .Select(c => new
        //        {
        //            Id = c.CustomerAddressID,
        //            Name = c.Customer.FullName,
        //            Email = c.Address.Email,
        //            Phone = c.Address.Phone
        //        })
        //        .Where(x => x.Name != null && EF.Functions.Like(x.Name, $"%{query}%"))
        //        .OrderByDescending(x => x.Name == query)                   // exact match first
        //        .ThenByDescending(x => EF.Functions.Like(x.Name, $"{query}%")) // then starts with
        //        .ThenBy(x => x.Name)                                      // then alphabetical
        //        .Take(5)
        //        .ToListAsync();

        //    return Json(results);
        //}
        //#endregion

        //#region GetCustomerInfo
        //[HttpPost]
        //public async Task<IActionResult> GetCustomerInfo([FromBody]  int id)
        //{
        //    var customerObj = await (from add in _context.CustomerAddresses
        //                             join ind in _context.Customers
        //                             on add.CustomerID equals ind.CustomerID
        //                             join address in _context.Addresses on add.AddressID equals address.AddressID
        //                             join country in _context.Country on address.CountryID equals country.CountryID into countryGroup
        //                             from country in countryGroup.DefaultIfEmpty()
        //                             where add.CustomerAddressID == id
        //                             select new
        //                             {
        //                                 FullName = ind.FullName,
        //                                 CustomerAddressID = add.CustomerID,
        //                                 AddressTypeName = add.AddressType.AddressTypeName,
        //                                 FullAddress = address.FullAddress,
        //                                 Street = address.Street,
        //                                 City = address.City,
        //                                 Additionaladdress = address.Additionaladdress,
        //                                 State = address.State,
        //                                 PostalCode = address.PostalCode,
        //                                 CountryID = country != null ? country.CountryID : 0,
        //                                 CountryCode = country != null ? country.CountryCode : null,
        //                                 Latitude = address.Latitude,
        //                                 Longitude = address.Longitude,
        //                                 Phone = address.Phone,
        //                                 OtherPhone = address.OtherPhone,
        //                                 Email = address.Email,
        //                                 FirstName = address.FirstName,
        //                                 LastName = address.LastName
        //                             }).FirstOrDefaultAsync();

        //    return Json(new { customer = customerObj });
        //}
        //#endregion

        //#region InsertPerson
        //[Permission("Create", "CreateLead")]
        //[HttpPost]
        //public async Task<IActionResult> InsertPerson([FromBody] CustomerVM customerVM)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        if (customerVM.PrimaryID == 0)
        //        {
        //            var result = await _leadCreateService.CreatePerson(customerVM);

        //            return Ok(result);
        //        } 
        //    }
        //    var results =  new ReturnView
        //    {
        //        Success = false,
        //        Message = "Data not inserted",
        //    };
        //    return Ok(results); 
        //}
        //#endregion

        //#region InsertCompany
        //[HttpPost]
        //public async Task<IActionResult> InsertCompany([FromBody] CompanyVM companyVM)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        if (companyVM.PrimaryID == 0)
        //        {
        //            var result = await _leadCreateService.CreateCompany(companyVM);

        //            return Ok(result);
        //        } 
        //    }
        //    var results =  new ReturnView
        //    {
        //        Success = false,
        //        Message = "Data not inserted",
        //    };
        //    return Ok(results); 
        //}
        //#endregion

        //#region InsertBranch
        //public async Task<IActionResult> InsertBranch([FromBody] BranchVM branchVM)
        //{
        //    try
        //    {
        //        if (ModelState.IsValid)
        //        {

        //            if (branchVM.PrimaryID == 0)
        //            {
        //                var result = await _leadCreateService.CreateBranch(branchVM);
        //                return Ok(result);
        //            }
        //        }
        //        return Ok(false);
        //    }
        //    catch (Exception)
        //    {
        //        return Ok(false);
        //    }

        //}
        //#endregion

        //#region InsertWarehouse
        //public async Task<IActionResult> InsertWarehouse([FromBody] WarehouseVM warehouseVM)
        //{
        //    try
        //    {
        //        if (ModelState.IsValid)
        //        {

        //            if (warehouseVM.PrimaryID == 0)
        //            {
        //                var result = await _leadCreateService.CreateWarehouse(warehouseVM);
        //                return Ok(result);
        //            }
        //        }
        //        return Ok(false);
        //    }
        //    catch (Exception)
        //    {
        //        return Ok(false);
        //    }

        //}
        //#endregion

        //#region InsertShippingAddress
        //[HttpPost]
        //public async Task<IActionResult> InsertShippingAddress([FromBody] ShippingVM shippingVM)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        if (shippingVM.PrimaryID == 0)
        //        {
        //            var result = await _leadCreateService.CreateShippingAddress(shippingVM);

        //            return Ok(result);
        //        } 
        //    }
        //    return Ok(false); 
        //}
        //#endregion

        #region CreateLeadData
        [Permission("Create", "CreateLead")]
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
        #endregion

        #region GetEmployeeList
        [HttpGet]
        public async Task<IActionResult> GetEmployeeList(string query, int page)
        {
            const int pageSize = 10; // Number of items per page
            int skip = (page - 1) * pageSize; // Calculate how many items to skip

            // Fetch filtered and paginated data using LIKE
            var list = await _employeeRepository
            .Find(u => string.IsNullOrEmpty(query)
                || EF.Functions.Like(u.FirstName.ToString(), $"%{query}%")
                || EF.Functions.Like(u.LastName, $"%{query}%")
                )
            .Skip(skip)
            .OrderByDescending(e => e.CreatedAt)
            .Take(pageSize).Select(e => new
            {
                e.FirstName,
                e.LastName,
                e.EmployeeID,
            })
            .ToListAsync();
            return Ok(list);

        }


        #endregion

        #region Get LeadOwner List
        [HttpGet]
        public async Task<IActionResult> GetLeadOwnerList(string search = "", int page = 1, int pageSize = 20)
        {
            var result = await _leadCreateService.GetLeadOwnerListAsync(
               search, page, pageSize, await GetCurrentOrganizationIdAsync() ?? 0
            );
            var hasMore = (page * pageSize) < result.totalItem;
            var formatted = new
            {
                items = result.data.Select(c => new
                {
                    value = c.LeadID,
                    label = $"{c.LeadName} {c.Phone} {c.Email}",
                    group = ""
                }),
                hasMore
            };

            return Json(formatted);
        }

        #endregion

        #region Get LeadSource List
        [HttpGet]
        public async Task<IActionResult> GetLeadSourceList(string search = "", int page = 1, int pageSize = 20)
        {
            var result = await _leadCreateService.GetLeadSourceListAsync(
               search, page, pageSize, await GetCurrentOrganizationIdAsync() ?? 0
            );
            var hasMore = (page * pageSize) < result.totalItem;
            var formatted = new
            {
                items = result.data.Select(c => new
                {
                    value = c.Id,
                    label = $"{c.Name}",
                    group = ""
                }),
                hasMore
            };

            return Json(formatted);
        }

        #endregion

        #region Get LeadStatus List
        [HttpGet]
        public async Task<IActionResult> GetLeadStatusList(string search = "", int page = 1, int pageSize = 20)
        {
            var result = await _leadCreateService.GetLeadStatusListAsync(
               search, page, pageSize, await GetCurrentOrganizationIdAsync() ?? 0
            );
            var hasMore = (page * pageSize) < result.totalItem;
            var formatted = new
            {
                items = result.data.Select(c => new
                {
                    value = c.Id,
                    label = $"{c.Name}",
                    group = ""
                }),
                hasMore
            };

            return Json(formatted);
        }

        #endregion

        #region Get GetPriorityListAsync List
        [HttpGet]
        public async Task<IActionResult> GetPriorityList(string search = "", int page = 1, int pageSize = 20)
        {
            var result = await _leadCreateService.GetPriorityListAsync(
               search, page, pageSize, await GetCurrentOrganizationIdAsync() ?? 0
            );
            var hasMore = (page * pageSize) < result.totalItem;
            var formatted = new
            {
                items = result.data.Select(c => new
                {
                    value = c.Id.ToString(),
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
