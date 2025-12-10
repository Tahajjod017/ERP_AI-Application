using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.RequisitionDistribution.PurchaseOrder;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Language;
using GCTL.Service.RequisitionDistribution.CreatePO;
using GCTL.Service.RequisitionDistribution.PurchaseOrderDetails;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers.RequisitionAndDistribution.ProductPurchase
{
    public class PurchaseOrderDetailsController : BaseController
    {
        #region CTOR

        private readonly IGenericRepository<PurchasOrders> _purchaseOrderRepository;
        private readonly IGenericRepository<PurchasOrderItems> _purchaseOrderItemRepository;
        private readonly ICreatePurchaseOrder _purchaseOrderService;
        private readonly IGenericRepository<Products> _productRepository;
        private readonly IGenericRepository<Suppliers> _supplierRepository;
        private readonly IGenericRepository<PurOrderBaseSAddresses> _addressRepository;
        private readonly IGenericRepository<Locations> _locationRepository;
        private readonly IGenericRepository<PaymentMethods> _paymentMethodRepository;
        private readonly IGenericRepository<Statuses> _statusRepository;
        private readonly IUserInfoService _userInfoService;

        public PurchaseOrderDetailsController(
            ITranslateService translateService,
            IUserProfileService userProfileService,
            IGenericRepository<PurchasOrders> purchaseOrderRepository,
            IGenericRepository<PurchasOrderItems> purchaseOrderItemRepository,
            ICreatePurchaseOrder purchaseOrderService,
            IGenericRepository<Products> productRepository,
            IGenericRepository<Suppliers> supplierRepository,
            IGenericRepository<PurOrderBaseSAddresses> addressRepository,
            IGenericRepository<Locations> locationRepository,
            IGenericRepository<PaymentMethods> paymentMethodRepository,
            IGenericRepository<Statuses> statusRepository,
            IUserInfoService userInfoService)
            : base(translateService, userProfileService)
        {
            _purchaseOrderRepository = purchaseOrderRepository;
            _purchaseOrderItemRepository = purchaseOrderItemRepository;
            _purchaseOrderService = purchaseOrderService;
            _productRepository = productRepository;
            _supplierRepository = supplierRepository;
            _addressRepository = addressRepository;
            _locationRepository = locationRepository;
            _paymentMethodRepository = paymentMethodRepository;
            _statusRepository = statusRepository;
            _userInfoService = userInfoService;
        }

        #endregion

        #region READ-ONLY MODE - View Purchase Order
        public IActionResult Index(int id)
        {
            ViewBag.Products = new SelectList(_productRepository.AllActive().ToList(), "ProductID", "ProductName");
            ViewBag.PaymentMethods = new SelectList(_paymentMethodRepository.AllActive().ToList(), "PaymentMethodID", "MethodName");
            ViewBag.Locations = new SelectList(_locationRepository.AllActive().ToList(), "LocationID", "LocationName");
            ViewBag.IsEditMode = false;

            var purchaseOrder = _purchaseOrderRepository.AllActive()
                .Include(e => e.PurchasOrderItems).ThenInclude(e => e.Product)
                .Include(e => e.Supplier)
                .Include(e => e.Status)
                .Include(e => e.OBBillingAddress)
                .Include(e => e.OBShipingAddress)
               // .Include(e => e.ToLocationNavigation)
               // .Include(e => e.PaymentMethod)
                .Include(e => e.CreatedByNavigation)
                .Include(e => e.UpdatedByNavigation)
                .FirstOrDefault(e => e.PurchasOrderID == id);

            if (purchaseOrder == null)
            {
                return NotFound();
            }

            var vm = new PurchaseOrderDetailsViewModel
            {
                Id = purchaseOrder.PurchasOrderID,
                POID = purchaseOrder.POID,
                PurchaseDate = purchaseOrder.PurchaseDate ?? DateTime.Today,
                DueDate = purchaseOrder.DueDate ?? DateTime.Today,
                SupplierID = purchaseOrder.SupplierID,
                SupplierName = purchaseOrder.Supplier?.FullName ?? "",
                StatusID = purchaseOrder.StatusID,
                StatusName = purchaseOrder.Status?.StatusName ?? "",
             //   ToLocation = purchaseOrder.ToLocation,
             //   LocationName = purchaseOrder.ToLocationNavigation?.LocationName ?? "",
                PaymentMethodID = purchaseOrder.PaymentMethodID,
            //    PaymentMethodName = purchaseOrder.PaymentMethod?.MethodName ?? "",
                WorkorderNo = purchaseOrder.WorkorderNo,
                WorkOrderDate = purchaseOrder.WorkOrderDate,
                OtherReference = purchaseOrder.OtherReference,
                CheckNumber = purchaseOrder.CheckNumber,
                CheckDate = purchaseOrder.CheckDate,
                IsDraft = purchaseOrder.IsDraft,

                Items = purchaseOrder.PurchasOrderItems.Select(m => new PurchaseOrderItemDetails
                {
                    SL = m.PurchasOrderItemID,
                    ProductId = m.ProductID ?? 0,
                    ProductName = m.Product?.ProductName ?? "",
                    Quantity = m.Quantity ?? 0m,
                    UnitPrice = m.UnitPrice ?? 0m
                }).ToList(),

                TaxPercent = purchaseOrder.TaxPercent ?? 0m,
                TotalAmount = purchaseOrder.TotalAmount ?? 0m,
                TaxAmount = purchaseOrder.TaxAmount ?? 0m,
                GrandTotalAmount = purchaseOrder.GrandTotalAmount ?? 0m,
                PaidAmount = purchaseOrder.PaidAmount ?? 0m,
                DueAmount = purchaseOrder.DueAmount ?? 0m,
                Note = purchaseOrder.Note,
                TermsAndConditions = purchaseOrder.TermsAndConditions,
                AttachmentLink = purchaseOrder.AttachmentLink,

                // Billing Address
                BillingAddress = purchaseOrder.OBBillingAddress != null ? new AddressViewModel
                {
                    FirstName = purchaseOrder.OBBillingAddress.FirstName,
                    LastName = purchaseOrder.OBBillingAddress.LastName,
                    FullAddress = purchaseOrder.OBBillingAddress.FullAddress,
                    City = purchaseOrder.OBBillingAddress.City,
                    State = purchaseOrder.OBBillingAddress.State,
                    PostalCode = purchaseOrder.OBBillingAddress.PostalCode,
                    Phone = purchaseOrder.OBBillingAddress.Phone,
                    Email = purchaseOrder.OBBillingAddress.Email
                } : null,

                // Shipping Address
                ShippingAddress = purchaseOrder.OBShipingAddress != null ? new AddressViewModel
                {
                    FirstName = purchaseOrder.OBShipingAddress.FirstName,
                    LastName = purchaseOrder.OBShipingAddress.LastName,
                    FullAddress = purchaseOrder.OBShipingAddress.FullAddress,
                    City = purchaseOrder.OBShipingAddress.City,
                    State = purchaseOrder.OBShipingAddress.State,
                    PostalCode = purchaseOrder.OBShipingAddress.PostalCode,
                    Phone = purchaseOrder.OBShipingAddress.Phone,
                    Email = purchaseOrder.OBShipingAddress.Email
                } : null,

                // Sidebar data
                CreatedByName = purchaseOrder.CreatedByNavigation != null
                    ? purchaseOrder.CreatedByNavigation.FirstName + " " + purchaseOrder.CreatedByNavigation.LastName
                    : "Unknown",
                CreatedAt = purchaseOrder.CreatedAt,
                UpdatedByName = purchaseOrder.UpdatedByNavigation?.FirstName,
                UpdatedAt = purchaseOrder.UpdatedAt
            };

            // Create sidebar view model
            var sidebarVm = new PurchaseOrderSidebarDetailsViewModel
            {
                PurchaseOrderId = vm.Id,
                POID = vm.POID,
                StatusName = vm.StatusName,
                IsDraft = vm.IsDraft,
                TotalAmount = vm.GrandTotalAmount,
                PaidAmount = vm.PaidAmount,
                DueAmount = vm.DueAmount,
                CreatedByName = vm.CreatedByName,
                CreatedAt = vm.CreatedAt,
                UpdatedByName = vm.UpdatedByName,
                UpdatedAt = vm.UpdatedAt
            };

            ViewBag.SidebarData = sidebarVm;

            return View(vm);
        }
        #endregion

        #region EDIT MODE - Edit Purchase Order
        public IActionResult Edit(int id)
        {
            ViewBag.Products = new SelectList(_productRepository.AllActive().ToList(), "ProductID", "ProductName");
            ViewBag.PaymentMethods = new SelectList(_paymentMethodRepository.AllActive().ToList(), "PaymentMethodID", "MethodName");
            ViewBag.Locations = new SelectList(_locationRepository.AllActive().ToList(), "LocationID", "LocationName");
            ViewBag.Statuses = new SelectList(_statusRepository.AllActive().ToList(), "StatusID", "StatusName");
            ViewBag.IsEditMode = true;

            var purchaseOrder = _purchaseOrderRepository.AllActive()
                .Include(e => e.PurchasOrderItems)
                .Include(e => e.CreatedByNavigation)
                .Include(e => e.UpdatedByNavigation)
                .Include(e => e.Status)
                .Include(e => e.OBBillingAddress)
                .Include(e => e.OBShipingAddress)
                .FirstOrDefault(e => e.PurchasOrderID == id);

            if (purchaseOrder == null)
            {
                return NotFound();
            }

            var vm = new PurchaseOrderDetailsViewModel
            {
                Id = purchaseOrder.PurchasOrderID,
                POID = purchaseOrder.POID,
                PurchaseDate = purchaseOrder.PurchaseDate ?? DateTime.Today,
                DueDate = purchaseOrder.DueDate ?? DateTime.Today,
                SupplierID = purchaseOrder.SupplierID,
                StatusID = purchaseOrder.StatusID,
              //  ToLocation = purchaseOrder.ToLocation,
                PaymentMethodID = purchaseOrder.PaymentMethodID,
                WorkorderNo = purchaseOrder.WorkorderNo,
                WorkOrderDate = purchaseOrder.WorkOrderDate,
                OtherReference = purchaseOrder.OtherReference,
                CheckNumber = purchaseOrder.CheckNumber,
                CheckDate = purchaseOrder.CheckDate,
                IsDraft = purchaseOrder.IsDraft,

                Items = purchaseOrder.PurchasOrderItems.Select(m => new PurchaseOrderItemDetails
                {
                    SL = m.PurchasOrderItemID,
                    ProductId = m.ProductID ?? 0,
                    Quantity = m.Quantity ?? 0m,
                    UnitPrice = m.UnitPrice ?? 0m
                }).ToList(),

                TaxPercent = purchaseOrder.TaxPercent ?? 0m,
                TotalAmount = purchaseOrder.TotalAmount ?? 0m,
                TaxAmount = purchaseOrder.TaxAmount ?? 0m,
                GrandTotalAmount = purchaseOrder.GrandTotalAmount ?? 0m,
                PaidAmount = purchaseOrder.PaidAmount ?? 0m,
                DueAmount = purchaseOrder.DueAmount ?? 0m,
                Note = purchaseOrder.Note,
                TermsAndConditions = purchaseOrder.TermsAndConditions,

                // Sidebar data
                CreatedByName = purchaseOrder.CreatedByNavigation != null
                    ? purchaseOrder.CreatedByNavigation.FirstName + " " + purchaseOrder.CreatedByNavigation.LastName
                    : "Unknown",
                CreatedAt = purchaseOrder.CreatedAt,
                UpdatedByName = purchaseOrder.UpdatedByNavigation?.FirstName,
                UpdatedAt = purchaseOrder.UpdatedAt,
            };

            // Create sidebar view model
            var sidebarVm = new PurchaseOrderSidebarDetailsViewModel
            {
                PurchaseOrderId = vm.Id,
                POID = vm.POID,
                StatusName = vm.StatusName,
                IsDraft = vm.IsDraft,
                TotalAmount = vm.GrandTotalAmount,
                PaidAmount = vm.PaidAmount,
                DueAmount = vm.DueAmount,
                CreatedByName = vm.CreatedByName,
                CreatedAt = vm.CreatedAt,
                UpdatedByName = vm.UpdatedByName,
                UpdatedAt = vm.UpdatedAt
            };

            ViewBag.SidebarData = sidebarVm;

            return View("Index", vm);
        }
        #endregion

        #region Sidebar Actions

        [HttpPost]
        public async Task<IActionResult> Duplicate(int id, BaseViewModel vm)
        {
            await _purchaseOrderRepository.BeginTransactionAsync();

            try
            {
                var original = _purchaseOrderRepository.AllActive()
                    .Include(e => e.PurchasOrderItems)
                    .FirstOrDefault(e => e.PurchasOrderID == id);

                if (original == null)
                {
                    return Json(new { success = false, message = "Purchase Order not found" });
                }

                // Create duplicate
                var duplicate = new PurchasOrders
                {
                    SupplierID = original.SupplierID,
                    POID = await _purchaseOrderService.GetNextPO(),
                    PurchaseDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(30),
                    StatusID = original.StatusID,
                  //  ToLocation = original.ToLocation,
                    PaymentMethodID = original.PaymentMethodID,
                    TaxPercent = original.TaxPercent,
                    TotalAmount = original.TotalAmount,
                    TaxAmount = original.TaxAmount,
                    GrandTotalAmount = original.GrandTotalAmount,
                    Note = original.Note + " (Copy)",
                    TermsAndConditions = original.TermsAndConditions,
                    IsDraft = true,
                    CreatedAt = DateTime.Now,
                    CreatedBy = vm.CreatedBy
                };

                await _purchaseOrderRepository.AddAsync(duplicate, vm);
                await _userInfoService.ActionLogAsync("PurchaseOrderDup", ActionName.DataAdd, null, duplicate, duplicate.PurchasOrderID, vm);

                // Copy items
                foreach (var item in original.PurchasOrderItems)
                {
                    var duplicateItem = new PurchasOrderItems
                    {
                        PurchasOrderID = duplicate.PurchasOrderID,
                        ProductID = item.ProductID,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        CreatedAt = DateTime.Now,
                        CreatedBy = vm.CreatedBy
                    };
                    await _purchaseOrderItemRepository.AddAsync(duplicateItem, vm);
                    await _userInfoService.ActionLogAsync("PurchaseOrderDup", ActionName.DataAdd, null, duplicateItem, duplicateItem.PurchasOrderItemID, vm);
                }

                await _purchaseOrderItemRepository.CommitTransactionAsync();

                return Json(new { success = true, newPurchaseOrderId = duplicate.PurchasOrderID });
            }
            catch (Exception ex)
            {
                await _purchaseOrderItemRepository.RollbackTransactionAsync();
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, int statusId, BaseViewModel vm)
        {
            try
            {
                var purchaseOrder = await _purchaseOrderRepository.All().FirstOrDefaultAsync(e => e.PurchasOrderID == id);
                if (purchaseOrder == null)
                {
                    return Json(new { success = false, message = "Purchase Order not found" });
                }

                purchaseOrder.StatusID = statusId;
                purchaseOrder.UpdatedBy = vm.UpdatedBy;
                purchaseOrder.UpdatedAt = DateTime.Now;

                await _purchaseOrderRepository.UpdateAsync(purchaseOrder);
                await _userInfoService.ActionLogAsync("PurchaseOrder", ActionName.DataUpdated, purchaseOrder, purchaseOrder, purchaseOrder.PurchasOrderID, vm);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult SendEmail(int id, string recipientEmail, string subject, string message)
        {
            try
            {
                // TODO: Implement email sending logic
                return Json(new { success = false, message = "Email functionality not yet implemented" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult PrintPDF(int id)
        {
            // TODO: Implement PDF generation
            return NotFound();
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                var purchaseOrder = _purchaseOrderRepository.All().FirstOrDefault(e => e.PurchasOrderID == id);
                if (purchaseOrder == null)
                {
                    return Json(new { success = false, message = "Purchase Order not found" });
                }

                // Soft delete
                purchaseOrder.DeletedAt = DateTime.Now;
                purchaseOrder.DeletedBy = GetCurrentUserId();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        // Helper methods
        private int GetCurrentUserId()
        {
            return 1; // Replace with actual implementation
        }
    }
}
