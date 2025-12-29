using Bogus.DataSets;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.CRM.Customer;
using GCTL.Core.ViewModels.POS.Sales.PriceQuotationDetails;
using GCTL.Core.ViewModels.POS.Sales.Shipment;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.Statuse;
using GCTL.Service.POS.Sales.Shipment;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers.POS.Sales.ShipmentF
{
    public class ChallanController : BaseController
    {
        private readonly IGenericRepository<Products> _productRepository;
        private readonly IGenericRepository<Locations> _locationRepository;
        private readonly IGenericRepository<InvoiceBaseCAddresses> _invAddressRepository;
        private readonly IGenericRepository<SalesOrders> _salesOrderRepository;
        private readonly IGenericRepository<SalesOrdersVersions> _salesOrderVersionRepository;
        private readonly IGenericRepository<Invoices> _invoiceRepository;

        private readonly IGenericRepository<Customers> _customerRepository;
        private readonly IGenericRepository<CustomerAddresses> _customerAddressRepository;
        private readonly IGenericRepository<Addresses> _cusaddressRepository;

        private readonly IChallan _challanService;
        private readonly IStatusService _statusService;

        public ChallanController(
            ITranslateService translateService,
            IUserProfileService userProfileService,
            IGenericRepository<Products> productRepository,
            IGenericRepository<Locations> locationRepository,
            IGenericRepository<InvoiceBaseCAddresses> addressRepository,
            IGenericRepository<SalesOrders> salesOrderRepository,
            IGenericRepository<Invoices> invoiceRepository,
            IChallan shipmentService,
            IGenericRepository<SalesOrdersVersions> salesOrderVersionRepository,
            IStatusService statusService,
            IGenericRepository<Customers> customerRepository,
            IGenericRepository<CustomerAddresses> customerAddressRepository,
            IGenericRepository<Addresses> cusaddressRepository)
            : base(translateService, userProfileService)
        {
            _productRepository = productRepository;
            _locationRepository = locationRepository;
            _invAddressRepository = addressRepository;
            _salesOrderRepository = salesOrderRepository;
            _invoiceRepository = invoiceRepository;
            _challanService = shipmentService;
            _salesOrderVersionRepository = salesOrderVersionRepository;
            _statusService = statusService;
            _customerRepository = customerRepository;
            _customerAddressRepository = customerAddressRepository;
            _cusaddressRepository = cusaddressRepository;
        }

        // ==============================
        // GET: /Shipment/Index
        // ==============================
        public IActionResult Index(int? salesOrderId = null, int? invoiceId = null)
        {

            ViewBag.location = new SelectList(_locationRepository.AllActive().Select(e => new { Id = e.LocationID, Name = e.LocationName + " (" + e.LocationCode + ")" }).ToList(), "Id", "Name");


            ViewBag.product = new SelectList(_productRepository.AllActive().ToList(), "ProductID", "ProductName");

            SetSmartPageCode(90260100);

            var vm = new ChallanViewModel
            {
                Id = null,
                ShipmentDate = DateTime.Today,
                ExpectedDeliveryDate = DateTime.Today.AddDays(7),
                SalesOrderId = salesOrderId,
                InvoiceId = invoiceId,
                Items = new List<ShipmentItem>
                {
                    new ShipmentItem
                    {
                        SL = 1,
                        ProductId = null,
                        OrderedQuantity = null,
                        ShippedQuantity = null,
                        
                    }
                }
            };

            // Load source details if provided
            //if (salesOrderId.HasValue)
            //{
            //    var salesOrder = _salesOrderRepository.AllActive()
            //        .FirstOrDefault(so => so.SalesOrdersID == salesOrderId);
            //    if (salesOrder != null)
            //    {
            //        vm.SourceType = "SalesOrder";
            //        vm.SourceNumber = salesOrder.SalesOrderNumber;
            //    }
            //}
            if (salesOrderId.HasValue)
            {
                var stsus = _statusService.GetStatusID("Cancelled");

                var salesOrder = _salesOrderVersionRepository.AllActive()
                    .Include(e=>e.SalesOrders)
                    .Include(e=>e.SalesOrderVersionItems)
                    .Include(e=>e.Challans).ThenInclude(e=>e.ChallanItems)
                    .Include(e=>e.Customer).ThenInclude(e=>e.CustomerAddresses).ThenInclude(e=>e.Address)
                    .Where(so => so.SalesOrdersVersionID == salesOrderId)
                    .Select(salesOrder => new
                    {
                        SourceNumber = salesOrder.SalesOrders.SalesOrderNumber,
                        SourceId = salesOrder.SalesOrdersVersionID,
                        Items = salesOrder.SalesOrderVersionItems,
                        CustomerData = salesOrder.Customer,
                        cusAddress = salesOrder.Customer.CustomerAddresses.FirstOrDefault().Address,
                        Challans = salesOrder.Challans,
                        LocationID = salesOrder.LocationID
                    })

                    .FirstOrDefault();
                if (salesOrder != null)
                {
                    vm.SourceType = "SalesOrderDetails";
                    vm.SourceNumber = salesOrder.SourceNumber;
                    vm.SourceId = salesOrder.SourceId;
                    vm.Items = salesOrder.Items.Select(e =>
                    {
                        var alreadyShipped = salesOrder.Challans
                            .Where(s => s.StatusID != stsus) // Cancel বাদ
                            .SelectMany(s => s.ChallanItems)
                            .Where(si => si.ProductID == e.ProductID && si.DeletedAt == null)
                            .Sum(si => si.DeliveredQuantity) ?? 0;

                        var orderedQty = e.Quantity ?? 0;

                        return new ShipmentItem
                        {
                            SL = 1,
                            ProductId = e.ProductID,
                            OrderedQuantity = orderedQty,
                            AlreadyShipped = alreadyShipped,
                            //RemainingQuantity = orderedQty - alreadyShipped,
                            ShippedQuantity = orderedQty - alreadyShipped,
                            FromLocationId = salesOrder.LocationID
                        };
                    }).ToList();

                    vm.CustomerData = new CustomerDetailsViewModel()
                    {
                        CompanyName = salesOrder.CustomerData != null ? salesOrder.CustomerData.FullName : "",
                        AddressLine1 = salesOrder.cusAddress != null ? salesOrder.cusAddress.FullAddress : "",
                        ContactName = salesOrder.cusAddress != null ? salesOrder.cusAddress.FirstName + " " + salesOrder.cusAddress.FirstName : "",
                        Email = salesOrder.cusAddress != null ? salesOrder.cusAddress.Email : "",
                        Phone = salesOrder.cusAddress != null ? salesOrder.cusAddress.Phone : "",
                    };
                }
            }
            else if (invoiceId.HasValue)
            {
                var invoice = _invoiceRepository.AllActive()
                    .FirstOrDefault(i => i.InvoiceID == invoiceId);
                if (invoice != null)
                {
                    vm.SourceType = "InvoiceDetails";
                    vm.SourceNumber = invoice.InvoiceNumber;
                }
            }

            return View(vm);
        }

        // ==============================
        // POST: Save Shipment
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Save(ChallanViewModel vm)
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

            var result = _challanService.SaveAsync(vm).Result;

            return Json(new
            {
                success = result.Success,
                message = result.Message,
                shipmentId = result.Data
            });
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
                    name = p.ProductName
                }).ToList();

            return Json(result);
        }

        // ==============================
        // AJAX: Get Locations
        // ==============================
        [HttpGet]
        public JsonResult GetLocations()
        {
            var result = _locationRepository.AllActive()
                .Select(l => new
                {
                    id = l.LocationID,
                    name = l.LocationName
                }).ToList();

            return Json(result);
        }

        // ==============================
        // AJAX: Get Addresses
        // ==============================
        [HttpGet]
        public JsonResult GetAddresses()
        {
            var result = _invAddressRepository.AllActive()
                .Select(a => new
                {
                    id = a.InvoiceBaseCAddressID,
                    fullName = a.FirstName + " " + a.LastName,
                    fullAddress = a.FullAddress,
                    city = a.City,
                    state = a.State,
                    postalCode = a.PostalCode,
                    phone = a.Phone,
                    email = a.Email
                }).ToList();

            return Json(result);
        }

        // ==============================
        // Helper: Get Next Shipment Number
        // ==============================
        [HttpGet]
        public IActionResult GetNextShipmentNumber()
        {
            var result = _challanService.GetNextShipmentNumber().Result;
            return Ok(result);
        }

        // ==============================
        // AJAX: Get Sales Order Items
        // ==============================
        [HttpGet]
        public JsonResult GetSalesOrderItems(int salesOrderId)
        {
            // TODO: Implement when SalesOrderVersionItems is available
            return Json(new List<object>());
        }

        // ==============================
        // AJAX: Get Invoice Items
        // ==============================
        [HttpGet]
        public JsonResult GetInvoiceItems(int invoiceId)
        {
            var invoice = _invoiceRepository.AllActive()
                .Where(i => i.InvoiceID == invoiceId)
                .FirstOrDefault();

            if (invoice == null)
            {
                return Json(new List<object>());
            }

            // TODO: Implement when InvoiceItems relationship is properly set
            return Json(new List<object>());
        }


        [HttpGet]
        public JsonResult GetAllAddresss(int? saleOrderId, int? invoiceId)
        {
            int? inv = 0;
            int? sal = 0;

            if (saleOrderId != null && saleOrderId != 0)
            {
                sal = _salesOrderVersionRepository.AllActive().Where(e => e.SalesOrdersVersionID == saleOrderId).Select(e => e.CustomerID).FirstOrDefaultAsync().Result;
            }

            if (invoiceId != null && invoiceId != 0)
            {
                inv = _invoiceRepository.AllActive().Where(e => e.InvoiceID == invoiceId).Select(e => e.CustomerID).FirstOrDefaultAsync().Result;
            }


            var result = _customerAddressRepository.AllActive()
                .Include(e=>e.Address)
                .Include(e=>e.Customer)
                .Where(e=>e.CustomerID == inv || e.CustomerID == sal)
                .Select(address => new
                {
                    Id = address.AddressID,
                    CompanyName = address.Customer.FullName,
                    ContactName = address.Address.FirstName + " " + address.Address.LastName,
                    address.Address.Email,
                    address.Address.Phone,
                    AddressLine1 = address.Address.FullAddress,
                    AddressLine2 = address.Address.Additionaladdress,
                    TaxNumber = address.Address.OtherPhone
                }).ToList();

            return Json(result);
        }
    }
}
