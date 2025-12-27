using GCTL.Core.Enums;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Sales.ShipmentDetails;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Language;
using GCTL.Service.POS.Sales.Shipment;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers.POS.Sales.ShipmentF
{
    public class ShipmentDetailsController : BaseController
    {
        #region CTOR
        private readonly IGenericRepository<Shipments> _shipmentRepository;
        private readonly IGenericRepository<ShipmentItems> _shipmentItemRepository;
        private readonly IShipment _shipmentService;
        private readonly IGenericRepository<Products> _productRepository;
        private readonly IGenericRepository<Locations> _locationRepository;
        private readonly IGenericRepository<InvoiceBaseCAddresses> _addressRepository;
        private readonly IUserInfoService _userInfoService;

        public ShipmentDetailsController(
            ITranslateService translateService,
            IUserProfileService userProfileService,
            IGenericRepository<Shipments> shipmentRepository,
            IGenericRepository<ShipmentItems> shipmentItemRepository,
            IShipment shipmentService,
            IGenericRepository<Products> productRepository,
            IGenericRepository<Locations> locationRepository,
            IGenericRepository<InvoiceBaseCAddresses> addressRepository,
            IUserInfoService userInfoService)
            : base(translateService, userProfileService)
        {
            _shipmentRepository = shipmentRepository;
            _shipmentItemRepository = shipmentItemRepository;
            _shipmentService = shipmentService;
            _productRepository = productRepository;
            _locationRepository = locationRepository;
            _addressRepository = addressRepository;
            _userInfoService = userInfoService;
        }
        #endregion

        #region READ-ONLY MODE - View Shipment
        public IActionResult Index(int id)
        {
            ViewBag.IsEditMode = false;
            SetSmartPageCode(90260200);

            var shipment = _shipmentRepository.AllActive()
                .Include(s => s.ShipmentItems)
                    .ThenInclude(i => i.Product)
                .Include(s => s.ShipmentItems)
                    .ThenInclude(i => i.FromLocation)
                .Include(s => s.ShippingAddress)
                .Include(s => s.SalesOrdersVersion).ThenInclude(e=>e.SalesOrders)
                .Include(s => s.Invoice)
                .Include(s => s.Status)
                .Include(s => s.CreatedByNavigation)
                .Include(s => s.UpdatedByNavigation)
                .FirstOrDefault(s => s.ShipmentID == id);

            if (shipment == null)
            {
                return NotFound();
            }

            var address = shipment.ShippingAddress != null
                ? new AddressDetailsViewModel
                {
                    Id = shipment.ShippingAddress.InvoiceBaseCAddressID,
                    FullName = shipment.ShippingAddress.FirstName + " " + shipment.ShippingAddress.LastName,
                    FullAddress = shipment.ShippingAddress.FullAddress,
                    City = shipment.ShippingAddress.City,
                    State = shipment.ShippingAddress.State,
                    PostalCode = shipment.ShippingAddress.PostalCode,
                    Phone = shipment.ShippingAddress.Phone,
                    Email = shipment.ShippingAddress.Email
                }
                : new AddressDetailsViewModel();

            var vm = new ShipmentDetailsViewModel
            {
                Id = shipment.ShipmentID,
                ShipmentNumber = shipment.ShipmentNumber,
                ShipmentDate = shipment.ShipmentDate,
                ExpectedDeliveryDate = shipment.ExpectedDeliveryDate,
                ActualDeliveryDate = shipment.ActualDeliveryDate,
                SalesOrderId = shipment.SalesOrdersVersionID,
                InvoiceId = shipment.InvoiceID,
                SourceType = shipment.SalesOrdersVersionID.HasValue ? "Sales Order" : "Invoice",
                SourceNumber = shipment.SalesOrdersVersionID.HasValue
                    ? shipment.SalesOrdersVersion?.SalesOrders?.SalesOrderNumber
                    : shipment.Invoice?.InvoiceNumber,
                TrackingNumber = shipment.TrackingNumber,
                ShippingCost = shipment.ShippingCost,
                Items = shipment.ShipmentItems.Select(i => new ShipmentItemDetails
                {
                    SL = i.ShipmentItemID,
                    ProductId = i.ProductID,
                    ProductName = i.Product?.ProductName ?? "",
                    OrderedQuantity = i.OrderedQuantity ?? 0m,
                    ShippedQuantity = i.ShippedQuantity ?? 0m,
                    FromLocationId = i.FromLocationID,
                    FromLocationName = i.FromLocation?.LocationName ?? "",
                    Note = i.Note
                }).ToList(),
                Note = shipment.Note,
                ShippingAddress = address,
                //Status = (ShipmentStatus)(shipment.Status.StatusName),
                Status = (ShipmentStatus)Enum.Parse(typeof(ShipmentStatus), shipment.Status.StatusName),
                CreatedByName = shipment.CreatedByNavigation != null
                    ? shipment.CreatedByNavigation.FirstName + " " + shipment.CreatedByNavigation.LastName
                    : "Unknown",
                CreatedAt = shipment.CreatedAt,
                UpdatedByName = shipment.UpdatedByNavigation != null
                    ? shipment.UpdatedByNavigation.FirstName + " " + shipment.UpdatedByNavigation.LastName
                    : "Unknown",
                UpdatedAt = shipment.UpdatedAt
            };

            var sidebarVm = new ShipmentSidebarDetailsViewModel
            {
                ShipmentId = vm.Id,
                ShipmentNumber = vm.ShipmentNumber,
                Status = vm.Status,
                CreatedByName = vm.CreatedByName,
                CreatedAt = vm.CreatedAt,
                UpdatedByName = vm.UpdatedByName,
                UpdatedAt = vm.UpdatedAt,
                SourceType = vm.SourceType,
                SourceId = vm.SalesOrderId ?? vm.InvoiceId,
                SourceNumber = vm.SourceNumber,
                TrackingNumber = vm.TrackingNumber
            };

            ViewBag.SidebarData = sidebarVm;

            return View(vm);
        }
        #endregion

        #region EDIT MODE - Edit Shipment
        public IActionResult Edit(int id)
        {
            ViewBag.IsEditMode = true;

            var shipment = _shipmentRepository.AllActive()
                .Include(s => s.ShipmentItems)
                    .ThenInclude(i => i.Product)
                .Include(s => s.ShipmentItems)
                    .ThenInclude(i => i.FromLocation)
                .Include(s => s.ShippingAddress)
                .Include(s => s.SalesOrdersVersion).ThenInclude(e => e.SalesOrders)
                .Include(s => s.Invoice)
                .Include(s => s.Status)
                .Include(s => s.CreatedByNavigation)
                .Include(s => s.UpdatedByNavigation)
                .FirstOrDefault(s => s.ShipmentID == id);

            if (shipment == null)
            {
                return NotFound();
            }

            var vm = new ShipmentDetailsViewModel
            {
                Id = shipment.ShipmentID,
                ShipmentNumber = shipment.ShipmentNumber,
                ShipmentDate = shipment.ShipmentDate,
                ExpectedDeliveryDate = shipment.ExpectedDeliveryDate,
                ActualDeliveryDate = shipment.ActualDeliveryDate,
                SalesOrderId = shipment.SalesOrdersVersionID,
                InvoiceId = shipment.InvoiceID,
                SourceType = shipment.SalesOrdersVersionID.HasValue ? "Sales Order" : "Invoice",
                SourceNumber = shipment.SalesOrdersVersionID.HasValue
                    ? shipment.SalesOrdersVersion?.SalesOrders.SalesOrderNumber
                    : shipment.Invoice?.InvoiceNumber,
                ShippingAddressId = shipment.ShippingAddressID,
                TrackingNumber = shipment.TrackingNumber,
                ShippingCost = shipment.ShippingCost,
                Items = shipment.ShipmentItems.Select(i => new ShipmentItemDetails
                {
                    SL = i.ShipmentItemID,
                    ProductId = i.ProductID,
                    ProductName = i.Product?.ProductName ?? "",
                    OrderedQuantity = i.OrderedQuantity ?? 0m,
                    ShippedQuantity = i.ShippedQuantity ?? 0m,
                    FromLocationId = i.FromLocationID,
                    FromLocationName = i.FromLocation?.LocationName ?? "",
                    Note = i.Note
                }).ToList(),
                Note = shipment.Note,
                Status = (ShipmentStatus)(shipment.StatusID ?? 1),
                CreatedByName = shipment.CreatedByNavigation != null
                    ? shipment.CreatedByNavigation.FirstName + " " + shipment.CreatedByNavigation.LastName
                    : "Unknown",
                CreatedAt = shipment.CreatedAt,
                UpdatedByName = shipment.UpdatedByNavigation != null
                    ? shipment.UpdatedByNavigation.FirstName + " " + shipment.UpdatedByNavigation.LastName
                    : "Unknown",
                UpdatedAt = shipment.UpdatedAt
            };

            var sidebarVm = new ShipmentSidebarDetailsViewModel
            {
                ShipmentId = vm.Id,
                ShipmentNumber = vm.ShipmentNumber,
                Status = vm.Status,
                CreatedByName = vm.CreatedByName,
                CreatedAt = vm.CreatedAt,
                UpdatedByName = vm.UpdatedByName,
                UpdatedAt = vm.UpdatedAt,
                SourceType = vm.SourceType,
                SourceId = vm.SalesOrderId ?? vm.InvoiceId,
                SourceNumber = vm.SourceNumber,
                TrackingNumber = vm.TrackingNumber
            };

            ViewBag.SidebarData = sidebarVm;

            return View("Index", vm);
        }
        #endregion

        #region Sidebar Actions

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, int status, BaseViewModel baseView)
        {
            try
            {
                var result = await _shipmentService.UpdateStatusAsync(id, status, baseView.CreatedBy ?? 1);

                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsPacked(int id, BaseViewModel vm)
        {
            return await UpdateStatus(id, 2, vm); // Status: Packed
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsShipped(int id, BaseViewModel vm)
        {
            return await UpdateStatus(id, 3, vm); // Status: Shipped
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsInTransit(int id, BaseViewModel vm)
        {
            return await UpdateStatus(id, 4, vm); // Status: InTransit
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsDelivered(int id, BaseViewModel vm)
        {
            return await UpdateStatus(id, 5, vm); // Status: Delivered
        }

        [HttpPost]
        public async Task<IActionResult> CancelShipment(int id, BaseViewModel vm)
        {
            return await UpdateStatus(id, 6, vm); // Status: Cancelled
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                var shipment = _shipmentRepository.All().FirstOrDefault(s => s.ShipmentID == id);
                if (shipment == null)
                {
                    return Json(new { success = false, message = "Shipment not found" });
                }

                shipment.DeletedAt = DateTime.Now;
                shipment.DeletedBy = GetCurrentUserId();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        private int GetCurrentUserId()
        {
            var id = GetCurrentEmployeeIdAsync().Result;
            return id ?? 1; // Replace with actual user ID from session
        }
    }

}
