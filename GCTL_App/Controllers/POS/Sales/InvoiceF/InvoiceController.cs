using System.Threading.Tasks;
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
        public async Task<IActionResult> Create(int? customerId = null, int? salesOrderId = null)
        {
            ViewBag.Products = new SelectList(_productRepository.AllActive().ToList(), "ProductID", "ProductName");


            var vm = new InvoiceViewModel
            {
                Id = null,
                InvoiceDate = DateTime.Today,
                InvoiceNumber = await GenerateInvoiceNumber(),
                SelectedCustomerId = customerId,
                SelectedSalesOrderId = salesOrderId,
                IsDraft = true,

                Items = new List<InvoiceItem>
                {
                    new InvoiceItem
                    {
                        SL = 1,
                        ProductId = 0,
                        Quantity = 0,
                        UnitPrice = 0
                    }
                },

                VatPercent = 5m,
                InvoiceNote = ""
            };

            // If sales order is selected, load its data
            if (salesOrderId.HasValue)
            {
                var salesOrder = await _salesOrderVersionRepository.AllActive()
                    .Include(so => so.SalesOrderVersionItems)
                    .Include(so => so.Customer)
                    .FirstOrDefaultAsync(so => so.SalesOrdersVersionID == salesOrderId.Value);

                if (salesOrder != null)
                {
                    vm.SelectedCustomerId = salesOrder.CustomerID;
                    vm.SelectedSalesOrderId = salesOrder.SalesOrdersVersionID;
                    vm.VatPercent = salesOrder.VatPercentage ?? 5m;
                    vm.InvoiceNote = salesOrder.Note;
                    vm.IsDraft = true;
                    vm.OtherReference = "TODO";

                    vm.Items = salesOrder.SalesOrderVersionItems.Select(e => new InvoiceItem
                    {
                        SL = 1,
                        ProductId = e.ProductID ?? 0,
                        Quantity = e.Quantity ?? 0,
                        UnitPrice = e.Rate ?? 0,

                    }).ToList();

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
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToList()
                        );

                var messages = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
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
        //[HttpGet]
        //public JsonResult GetProducts()
        //{
        //    var result = _productRepository.AllActive().Include(e=>e.ProductPricing)
        //        .Select(p => new
        //        {
        //            id = p.ProductID,
        //            name = p.ProductName,
        //            price = p.ProductPricing != null ? p.ProductPricing.Select(e => e.SellingPriceExclVAT).FirstOrDefault() : 0m,
        //           // price =  52m,
        //            vatPercent = p.ProductPricing != null ? p.ProductPricing.Select(e => e.VATPercent).FirstOrDefault() : 0m
        //        })
        //        .ToList();
        //    return Json(result);
        //}

        [HttpGet]
        public JsonResult GetProducts()
        {
            var result = _productRepository.AllActive().Include(e => e.ProductPricing)
                .Select(p => new
                {
                    id = p.ProductID,
                    name = p.ProductName,
                    price = p.ProductPricing != null ? p.ProductPricing.Select(e => e.SellingPriceExclVAT).FirstOrDefault() : 0m,
                    vatPercent = p.ProductPricing != null ? p.ProductPricing.Select(e => e.VATPercent).FirstOrDefault() : 0m,
                    aitPercent = p.ProductPricing != null ? p.ProductPricing.Select(e => e.VATPercent).FirstOrDefault() : 0m // Add this
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
        private async Task<string> GenerateInvoiceNumber()
        {
            return await _invoiceService.GetNextInvoiceCode();
        }

        [HttpGet]
        public IActionResult GetNextInvoiceNumber()
        {
            var result = _invoiceService.GetNextInvoiceCode().Result;
            return Ok(result);
        }
    }
}
