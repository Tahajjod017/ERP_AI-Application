using GCTL.Core.Repository;
using GCTL.Core.ViewModels.POS.Sales.InvoiceF;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.POS.Sales.InvoiceF;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers.POS.Sales.InvoiceF
{
    public class InvoiceController : BaseController
    {
        private readonly IGenericRepository<Products> _productRepository;
        private readonly IGenericRepository<Customers> _customerRepository;
        private readonly IGenericRepository<CustomerAddresses> _customerAddressRepository;
        private readonly IGenericRepository<Addresses> _addressRepository;
        private readonly IGenericRepository<SalesOrders> _salesOrderRepository;
        private readonly IGenericRepository<SalesOrdersVersions> _salesOrderVersionRepository;
        private readonly IGenericRepository<PaymentMethods> _paymentMethodRepository;
        private readonly IInvoice _invoiceService;

        public InvoiceController(
            ITranslateService translateService,
            IUserProfileService userProfileService,
            IGenericRepository<Products> productRepository,
            IInvoice invoiceService,
            IGenericRepository<Customers> customerRepository,
            IGenericRepository<CustomerAddresses> customerAddressRepository,
            IGenericRepository<Addresses> addressRepository,
            IGenericRepository<SalesOrders> salesOrderRepository,
            IGenericRepository<PaymentMethods> paymentMethodRepository,
            IGenericRepository<SalesOrdersVersions> salesOrderVersionRepository)
            : base(translateService, userProfileService)
        {
            _productRepository = productRepository;
            _invoiceService = invoiceService;
            _customerRepository = customerRepository;
            _customerAddressRepository = customerAddressRepository;
            _addressRepository = addressRepository;
            _salesOrderRepository = salesOrderRepository;
            _paymentMethodRepository = paymentMethodRepository;
            _salesOrderVersionRepository = salesOrderVersionRepository;
        }

        // ==============================
        // GET: /Invoice/Index
        // ==============================
        public IActionResult Index()
        {
            SetSmartPageCode(902700);

            ViewBag.Products = new SelectList(_productRepository.AllActive().ToList(), "ProductID", "ProductName");
            ViewBag.PaymentMethods = new SelectList(_paymentMethodRepository.AllActive().ToList(), "PaymentMethodID", "MethodName");

            var data = new InvoiceViewModel();
            data.InvoiceDate = DateTime.Today;
            return View(data);
        }

        // ==============================
        // GET: /Invoice/Create
        // ==============================
        [HttpGet]
        public IActionResult Create(int? customerId = null, int? salesOrderId = null)
        {
            var vm = new InvoiceViewModel
            {
                Id = null,
                InvoiceDate = DateTime.Today,
                InvoiceNumber = GenerateInvoiceNumber(),
                SelectedCustomerId = customerId,
                SelectedSalesOrderId = salesOrderId,
                IsDraft = true,

                Items = new List<InvoiceItem>
                {
                    new InvoiceItem
                    {
                        SL = 1,
                        ProductId = 0,
                        Quantity = null,
                        UnitPrice = null
                    }
                },

                VatPercent = 5m,
                InvoiceNote = ""
            };

            // If sales order is selected, load its data
            if (salesOrderId.HasValue)
            {
                var salesOrder = _salesOrderVersionRepository.AllActive()
                    .Include(so => so.SalesOrderVersionItems)
                    .Include(so => so.Customer)
                    .FirstOrDefault(so => so.SalesOrdersVersionID == salesOrderId.Value);

                if (salesOrder != null)
                {
                    vm.SelectedCustomerId = salesOrder.CustomerID;
                    vm.SelectedSalesOrderId = salesOrder.SalesOrdersVersionID;
                    vm.VatPercent = salesOrder.VatPercentage ?? 5m;
                    vm.InvoiceNote = salesOrder.Note;

                    // Load items from sales order (you'll need to map these to products)
                    // This is a simplified version - adjust based on your business logic
                }
            }

            return View("Index", vm);
        }

        // ==============================
        // POST: Save Invoice
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Save(InvoiceViewModel vm)
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

            if (vm.Id > 0 && vm.Id != null)
            {
                var result = _invoiceService.UpdateAsync(vm).Result;

                return Json(new
                {
                    success = result.Success,
                    message = result.Message,
                    invoiceId = result.Data
                });
            }
            else
            {
                var result = _invoiceService.SaveAsync(vm, false).Result;

                return Json(new
                {
                    success = result.Success,
                    message = result.Message,
                    invoiceId = result.Data
                });
            }
          

            
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
        // AJAX: Get Products
        // ==============================
        [HttpGet]
        public JsonResult GetProducts()
        {
            var result = _productRepository.AllActive()
                .Select(p => new
                {
                    id = p.ProductID,
                    name = p.ProductName,
                    price = p.ProductAdvancedPricing != null ? p.ProductAdvancedPricing.Select(e => e.PriceValue).FirstOrDefault() : 0m
                })
                .ToList();

           

            return Json(result);
        }

        // ==============================
        // AJAX: Get Sales Orders by Customer
        // ==============================
        [HttpGet]
        public JsonResult GetSalesOrdersByCustomer(int customerId)
        {
            var result = _salesOrderVersionRepository.AllActive()
                .Where(so => so.CustomerID == customerId)
                .Select(so => new
                {
                    id = so.SalesOrdersVersionID,
                    number = so.SalesOrders.SalesOrderNumber,
                    date = so.SalesOrderDate
                })
                .OrderByDescending(so => so.date)
                .ToList();

            return Json(result);
        }

        // ==============================
        // AJAX: Get Sales Order Details
        // ==============================
        [HttpGet]
        public JsonResult GetSalesOrderDetails(int salesOrderId)
        {
            var salesOrder = _salesOrderVersionRepository.AllActive()
                .Include(so => so.SalesOrderVersionItems)
                .ThenInclude(i => i.UnitType)
                .FirstOrDefault(so => so.SalesOrdersVersionID == salesOrderId);

            if (salesOrder == null)
            {
                return Json(new { success = false, message = "Sales Order not found" });
            }

            var result = new
            {
                success = true,
                vatPercent = salesOrder.VatPercentage ?? 0,
                note = salesOrder.Note,
                items = salesOrder.SalesOrderVersionItems.Select(item => new
                {
                    description = item.Description,
                    unitName = item.UnitType?.UnitTypeName,
                    area = item.Area ?? 0,
                    rate = item.Rate ?? 0,
                    quantity = item.Quantity ?? 0
                }).ToList()
            };

            return Json(result);
        }

        // ==============================
        // AJAX: Get Payment Methods
        // ==============================
        [HttpGet]
        public JsonResult GetPaymentMethods()
        {
            var result = _paymentMethodRepository.AllActive()
                .Select(pm => new
                {
                    id = pm.PaymentMethodID,
                    name = pm.MethodName
                })
                .ToList();

            return Json(result);
        }

        // ==============================
        // Helper: Generate Invoice Number
        // ==============================
        private string GenerateInvoiceNumber()
        {
            return "INV-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        }

        [HttpGet]
        public IActionResult GetNextInvoiceNumber()
        {
            var result = _invoiceService.GetNextInvoiceCode().Result;
            return Ok(result);
        }
    }
}
