using GCTL.Core.Repository;
using GCTL.Core.ViewModels.POS.Sales.PriceQuotation;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.POS.Sales.PriceQuotation;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.POS.Sales
{
    public class PriceQuotationController : BaseController
    {
        private readonly IGenericRepository<UnitTypes> _unitTypeRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Inventory> _inventoryRepository;
        private readonly IGenericRepository<Products> _productRepository;
        private readonly IGenericRepository<Customers> _customerRepository;
        private readonly IGenericRepository<CustomerAddresses> _customerAddressRepository;
        private readonly IGenericRepository<Addresses> _addressRepository;
        private readonly IGenericRepository<Locations> _locationRepository;
        private readonly IPriceQuotation _priceQuotationService;
        public PriceQuotationController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<UnitTypes> unitTypeRepository, IPriceQuotation priceQuotationService, IGenericRepository<Customers> customerRepository, IGenericRepository<CustomerAddresses> customerAddressRepository, IGenericRepository<Addresses> addressRepository, IGenericRepository<Products> productRepository, IGenericRepository<GCTL.Data.Models.Inventory> inventoryRepository, IGenericRepository<Locations> locationRepository)
            : base(translateService, userProfileService)
        {
            _unitTypeRepository = unitTypeRepository;
            _priceQuotationService = priceQuotationService;
            _customerRepository = customerRepository;
            _customerAddressRepository = customerAddressRepository;
            _addressRepository = addressRepository;
            _productRepository = productRepository;
            _inventoryRepository = inventoryRepository;
            _locationRepository = locationRepository;
        }

        #region STATIC DATA
        // ==============================
        // STATIC DATA (Replace with DB later)
        // ==============================
        private static readonly List<CustomerDto> StaticCustomers = new()
        {
            new CustomerDto
            {
                Id = 1,
                CompanyName = "ABC Builders Ltd.",
                ContactName = "Jone",
                Email = "jone@abc.com",
                Phone = "(+880) 01737172631",
                AddressLine1 = "235 Mirpur, Road # 205",
                AddressLine2 = "Dhaka 1216, Bangladesh",
                TaxNumber = "10236254"
            },
            new CustomerDto
            {
                Id = 2,
                CompanyName = "XYZ Construction",
                ContactName = "Riya",
                Email = "riya@xyz.com",
                Phone = "(+880) 01911223344",
                AddressLine1 = "House 12, Road 5",
                AddressLine2 = "Banani, Dhaka",
                TaxNumber = "98765432"
            }
        };

        

        private static readonly List<PriceQuotationViewModel> StaticQuotations = new();

        #endregion

        // ==============================
        // GET: /PriceQuotation/Index
        // ==============================
        public IActionResult Index()
        {
            ViewBag.product = new SelectList(_productRepository.AllActive().ToList(), "ProductID", "ProductName");
            ViewBag.location = new SelectList(_locationRepository.AllActive().Select(e => new { Id = e.LocationID, Name = e.LocationName + " (" + e.LocationCode + ")" }).ToList(), "Id", "Name");


            SetSmartPageCode(90259100);

            var data = new PriceQuotationViewModel();
            return View(data);
        }

        // ==============================
        // GET: /PriceQuotation/Create
        // ==============================
        [HttpGet]
        public IActionResult Create(int? selectedId = null)
        {
            var vm = new PriceQuotationViewModel
            {
                Id = null,
                InvoiceDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(30),
                InvoiceNumber = GenerateInvoiceNumber(),
                OtherNumber = "",
                SelectedCustomerId = selectedId,

                // Initialize with one empty item
                Items = new List<QuotationItem>
                {
                    new QuotationItem
                    {
                        SL = 1,
                        Description = "",
                        Product = 0,
                        Area = null,
                        Rate = null,
                        PercentInBill = 100
                    }
                },

                RetentionPercent = 5m,
                Note = ""
            };

            return View("Index", vm);
        }

       

        // ==============================
        // POST: Save Quotation (AJAX Version)
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Save(PriceQuotationViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                //return Json(new { success = false, message = "Invalid data" });
                var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                        );

                var messages = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .SelectMany(x => x.Value.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();


                return Json(new { success = false, errors, message = messages });
            }

            
            

            var result = _priceQuotationService.SaveAsync(vm).Result;

            return Json(new
            {
                success = result.Success,
                message = result.Message,
                quotationId = result.Data
            });
        }

        // ==============================
        // AJAX: Get All Customers
        // ==============================
        [HttpGet]
        public JsonResult GetAllCustomers()
        {
           

            var result = (from customer in _customerRepository.AllActive()
                         join custAddr in _customerAddressRepository.AllActive()
                             on customer.CustomerID equals custAddr.CustomerID
                         join address in _addressRepository.AllActive()
                             on custAddr.AddressID equals address.AddressID
                         select new
                         {
                             Id = customer.CustomerID,
                             CompanyName = customer.FullName,
                             ContactName = address.FirstName + " " + address.LastName,
                             address.Email,
                             address.Phone,
                             AddressLine1 = address.FullAddress,
                             AddressLine2 =address.Additionaladdress,
                             TaxNumber = address.OtherPhone

                         }).ToList();

           

            return Json(result);
        }


        // ==============================
        // AJAX: Get All Customers
        // ==============================
        [HttpGet]
        public JsonResult GetUnits()
        {
            var result = _unitTypeRepository.AllActive().Select(r=> new {id = r.UnitTypeID , name = r.UnitTypeName}).ToList();

            return Json(result);
        }
        [HttpGet]
        public JsonResult GetProduct()
        {
            var result = _productRepository.AllActive().Select(r=> new {id = r.ProductID , name = r.ProductName}).ToList();

            return Json(result);
        }

        [HttpGet]
        public IActionResult GetStockQuantity(int productId, int? locationId)
        {
            decimal? available = 0m;
            if (locationId == null || locationId == 0)
            {
                 available = _inventoryRepository.AllActive().Where(e => e.ProductID == productId).Sum(e => e.Quantity - e.ReservedQuantity);

            }
            else
            {
                 available = _inventoryRepository.AllActive().Where(e => e.ProductID == productId && e.LocationID == locationId).Sum(e => e.Quantity - e.ReservedQuantity);

            }


            return Json(new { available });
        }

        // ==============================
        // AJAX: Search Customers
        // ==============================
        [HttpGet]
        public JsonResult SearchCustomers(string term)
        {
            var filtered = string.IsNullOrWhiteSpace(term)
                ? StaticCustomers
                : StaticCustomers.Where(c =>
                    c.CompanyName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    c.ContactName.Contains(term, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var result = filtered.Select(c => new
            {
                c.Id,
                c.CompanyName,
                c.ContactName,
                c.Email,
                c.Phone,
                c.AddressLine1,
                c.AddressLine2,
                c.TaxNumber
            });

            return Json(result);
        }

        // ==============================
        // AJAX: Add New Customer
        // ==============================
        [HttpPost]
        public JsonResult AddCustomer([FromBody] CustomerDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.CompanyName))
                return Json(new { error = "Invalid customer data" });

            dto.Id = StaticCustomers.Count > 0 ? StaticCustomers.Max(c => c.Id) + 1 : 1;
            StaticCustomers.Add(dto);

            return Json(dto);
        }

        // ==============================
        // Helper: Generate Invoice Number
        // ==============================
        private string GenerateInvoiceNumber()
        {
            return "INV-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        }

        [HttpGet]
        public IActionResult GetNextQuotationNumber()
        {
            var result = _priceQuotationService.GetNextPQcode().Result;
            return Ok(result);
        }

    }
}