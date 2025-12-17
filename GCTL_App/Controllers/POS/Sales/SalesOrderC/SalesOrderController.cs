using GCTL.Core.Repository;
using GCTL.Core.ViewModels.POS.Sales.SalesOrders;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.POS.Sales.SalesOrderF;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers.POS.Sales.SalesOrderC
{
    public class SalesOrderController : BaseController
    {
        private readonly IGenericRepository<UnitTypes> _unitTypeRepository;
        private readonly IGenericRepository<Customers> _customerRepository;
        private readonly IGenericRepository<CustomerAddresses> _customerAddressRepository;
        private readonly IGenericRepository<Addresses> _addressRepository;
        private readonly IGenericRepository<PriceQuotations> _priceQuotationRepository;
        private readonly ISalesOrder _salesOrderService;

        public SalesOrderController(
            ITranslateService translateService,
            IUserProfileService userProfileService,
            IGenericRepository<UnitTypes> unitTypeRepository,
            ISalesOrder salesOrderService,
            IGenericRepository<Customers> customerRepository,
            IGenericRepository<CustomerAddresses> customerAddressRepository,
            IGenericRepository<Addresses> addressRepository,
            IGenericRepository<PriceQuotations> priceQuotationRepository)
            : base(translateService, userProfileService)
        {
            _unitTypeRepository = unitTypeRepository;
            _salesOrderService = salesOrderService;
            _customerRepository = customerRepository;
            _customerAddressRepository = customerAddressRepository;
            _addressRepository = addressRepository;
            _priceQuotationRepository = priceQuotationRepository;
        }

        // ==============================
        // GET: /SalesOrder/Index
        // ==============================
        public IActionResult Index()
        {
            ViewBag.Unit = new SelectList(_unitTypeRepository.AllActive().ToList(), "UnitTypeID", "UnitTypeName");

            SetSmartPageCode(9029000);

            var data = new SalesOrderViewModel();
            data.OrderDate = DateTime.Today;
            return View(data);
        }

        // ==============================
        // GET: /SalesOrder/Create
        // ==============================
        [HttpGet]
        public IActionResult Create(int? selectedId = null, int? quotationId = null)
        {
            var vm = new SalesOrderViewModel
            {
                Id = null,
                OrderDate = DateTime.Today,
                OrderNumber = GenerateOrderNumber(),
                SelectedCustomerId = selectedId,
                SelectedQuotationId = quotationId,

                // Initialize with one empty item
                Items = new List<SalesOrderItem>
                {
                    new SalesOrderItem
                    {
                        SL = 1,
                        Description = "",
                        Unit = 0,
                        Area = null,
                        Rate = null,
                        Quantity = null
                    }
                },

                VatPercent = 5m,
                Note = ""
            };

            // If quotation is selected, load its data
            if (quotationId.HasValue)
            {
                var quotation = _priceQuotationRepository.AllActive()
                    .Include(q => q.PriceQuotationVersions)
                    //.Include(q => q.Customer)
                    .FirstOrDefault(q => q.PriceQuotationID == quotationId.Value);

                if (quotation != null)
                {
                   // vm.SelectedCustomerId = quotation.CustomerID;
                    vm.SelectedQuotationId = quotation.PriceQuotationID;
                   // vm.VatPercent = quotation.VatPercentage ?? 5m;
                   // vm.Note = quotation.Note;

                    vm.Items = quotation.PriceQuotationVersions.Select((item, index) => new SalesOrderItem
                    {
                        SL = index + 1,
                        //Description = item.Description,
                        //Unit = item.UnitTypeID ?? 0,
                        //Area = item.Area ?? 0,
                        //Rate = item.Rate ?? 0,
                        Quantity = 0
                    }).ToList();
                }
            }

            return View("Index", vm);
        }

        // ==============================
        // POST: Save Sales Order (AJAX Version)
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Save(SalesOrderViewModel vm)
        {
            if (!ModelState.IsValid)
            {
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


            var result = _salesOrderService.SaveAsync(vm).Result;

            return Json(new
            {
                success = result.Success,
                message = result.Message,
                salesOrderId = result.Data
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
                              AddressLine2 = address.Additionaladdress,
                              TaxNumber = address.OtherPhone
                          }).ToList();

            return Json(result);
        }

        // ==============================
        // AJAX: Get Units
        // ==============================
        [HttpGet]
        public JsonResult GetUnits()
        {
            var result = _unitTypeRepository.AllActive()
                .Select(r => new { id = r.UnitTypeID, name = r.UnitTypeName })
                .ToList();

            return Json(result);
        }

        // ==============================
        // AJAX: Get Price Quotations by Customer
        // ==============================
        [HttpGet]
        public JsonResult GetQuotationsByCustomer(int customerId)
        {
            var result = _priceQuotationRepository.AllActive()
               // .Where(q => q.CustomerID == customerId && q.IsFinalVersion == true)
                .Select(q => new
                {
                    id = q.PriceQuotationID,
                    number = q.QuotationNumber,
                   // date = q.QuotationDate
                })
               // .OrderByDescending(q => q.date)
                .ToList();

            return Json(result);
        }

        // ==============================
        // AJAX: Get Quotation Details
        // ==============================
        [HttpGet]
        public JsonResult GetQuotationDetails(int quotationId)
        {
            var quotation = _priceQuotationRepository.AllActive()
               // .Include(q => q.PriceQuotationItems)
              //  .ThenInclude(i => i.UnitType)
                .FirstOrDefault(q => q.PriceQuotationID == quotationId);

            if (quotation == null)
            {
                return Json(new { success = false, message = "Quotation not found" });
            }

            var result = new
            {
                success = true,
               // vatPercent = quotation.VatPercentage ?? 0,
               // note = quotation.Note,
                items = quotation.PriceQuotationVersions.Select(item => new
                {
                    //description = item.Description,
                    //unitId = item.UnitTypeID,
                    //unitName = item.UnitType?.UnitTypeName,
                    //area = item.Area ?? 0,
                    //rate = item.Rate ?? 0
                }).ToList()
            };

            return Json(result);
        }

        // ==============================
        // Helper: Generate Order Number
        // ==============================
        private string GenerateOrderNumber()
        {
            return "SO-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        }

        [HttpGet]
        public IActionResult GetNextSalesOrderNumber()
        {
            var result = _salesOrderService.GetNextSOcode().Result;
            return Ok(result);
        }
    }


}
