using GCTL.Core.Repository;
using GCTL.Core.ViewModels.POS.Sales.Shipment;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.POS.Sales.Shipment;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers.POS.Sales.ShipmentF
{
    public class ShipmentController : BaseController
    {
        private readonly IGenericRepository<Products> _productRepository;
        private readonly IGenericRepository<Locations> _locationRepository;
        private readonly IGenericRepository<InvoiceBaseCAddresses> _addressRepository;
        private readonly IGenericRepository<SalesOrders> _salesOrderRepository;
        private readonly IGenericRepository<SalesOrdersVersions> _salesOrderVersionRepository;
        private readonly IGenericRepository<Invoices> _invoiceRepository;

        private readonly IShipment _shipmentService;

        public ShipmentController(
            ITranslateService translateService,
            IUserProfileService userProfileService,
            IGenericRepository<Products> productRepository,
            IGenericRepository<Locations> locationRepository,
            IGenericRepository<InvoiceBaseCAddresses> addressRepository,
            IGenericRepository<SalesOrders> salesOrderRepository,
            IGenericRepository<Invoices> invoiceRepository,
            IShipment shipmentService,
            IGenericRepository<SalesOrdersVersions> salesOrderVersionRepository)
            : base(translateService, userProfileService)
        {
            _productRepository = productRepository;
            _locationRepository = locationRepository;
            _addressRepository = addressRepository;
            _salesOrderRepository = salesOrderRepository;
            _invoiceRepository = invoiceRepository;
            _shipmentService = shipmentService;
            _salesOrderVersionRepository = salesOrderVersionRepository;
        }

        // ==============================
        // GET: /Shipment/Index
        // ==============================
        public IActionResult Index(int? salesOrderId = null, int? invoiceId = null)
        {

            ViewBag.location = new SelectList(_locationRepository.AllActive().Select(e => new { Id = e.LocationID, Name = e.LocationName + " (" + e.LocationCode + ")" }).ToList(), "Id", "Name");


            ViewBag.product = new SelectList(_productRepository.AllActive().ToList(), "ProductID", "ProductName");

            SetSmartPageCode(90260100);

            var vm = new ShipmentViewModel
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
                var salesOrder = _salesOrderVersionRepository.AllActive()
                    .Include(e=>e.SalesOrders)
                    .Include(e=>e.SalesOrderVersionItems)
                    .FirstOrDefault(so => so.SalesOrdersVersionID == salesOrderId);
                if (salesOrder != null)
                {
                    vm.SourceType = "SalesOrder";
                    vm.SourceNumber = salesOrder.SalesOrders.SalesOrderNumber;
                    vm.Items = salesOrder.SalesOrderVersionItems.Select(e => new ShipmentItem
                    {
                        SL = 1,
                        ProductId = e.ProductID,
                        OrderedQuantity = e.Quantity,
                        ShippedQuantity = null,
                        FromLocationId = salesOrder.LocationID
                    }).ToList();
                }
            }
            else if (invoiceId.HasValue)
            {
                var invoice = _invoiceRepository.AllActive()
                    .FirstOrDefault(i => i.InvoiceID == invoiceId);
                if (invoice != null)
                {
                    vm.SourceType = "Invoice";
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
        public JsonResult Save(ShipmentViewModel vm)
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

            var result = _shipmentService.SaveAsync(vm).Result;

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
            var result = _addressRepository.AllActive()
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
            var result = _shipmentService.GetNextShipmentNumber().Result;
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
    }
}
