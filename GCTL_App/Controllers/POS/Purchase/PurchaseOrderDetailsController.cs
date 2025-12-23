using System;
using System.Linq;
using System.Threading.Tasks;
using GCTL.Core.Enums;
using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Purchase.PurchaseOrderDetails;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Language;
using GCTL.Service.POS.Purchase.PurchaseOrder;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers.POS.Purchase
{
    public class PurchaseOrderDetailsController : BaseController
    {
        #region CTOR
        private readonly IGenericRepository<PurchasOrders> _purchaseOrderRepository;
        private readonly IGenericRepository<PurchasOrderItemVersions> _purchaseOrderItemRepository;
        private readonly IGenericRepository<PurchasOrderVersions> _purchaseOrderVersionRepository;
        private readonly IPurchaseOrder _purchaseOrderService;
        private readonly IGenericRepository<Products> _productRepository;
        private readonly IGenericRepository<Suppliers> _supplierRepository;
        private readonly IGenericRepository<PurOrderBaseSAddresses> _addressRepository;

        private readonly IUserInfoService _userInfoService;

        public PurchaseOrderDetailsController(
            ITranslateService translateService,
            IUserProfileService userProfileService,
            IGenericRepository<PurchasOrders> purchaseOrderRepository,
            IGenericRepository<PurchasOrderItemVersions> purchaseOrderItemRepository,
            IPurchaseOrder purchaseOrderService,
            IGenericRepository<Products> productRepository,
            IGenericRepository<Suppliers> supplierRepository,
            IUserInfoService userInfoService,
            IGenericRepository<PurchasOrderVersions> purchaseOrderVersionRepository,
            IGenericRepository<PurOrderBaseSAddresses> addressRepository)
            : base(translateService, userProfileService)
        {
            _purchaseOrderRepository = purchaseOrderRepository;
            _purchaseOrderItemRepository = purchaseOrderItemRepository;
            _purchaseOrderService = purchaseOrderService;
            _productRepository = productRepository;
            _supplierRepository = supplierRepository;
            _userInfoService = userInfoService;
            _purchaseOrderVersionRepository = purchaseOrderVersionRepository;
            _addressRepository = addressRepository;
        }
        #endregion

        #region READ-ONLY MODE - View Purchase Order
        public IActionResult Index(int id)
        {
            ViewBag.Products = new SelectList(_productRepository.AllActive().ToList(), "ProductID", "ProductName");


            ViewBag.IsEditMode = false;
            SetSmartPageCode(90259300);

            var purchaseOrder = _purchaseOrderVersionRepository.AllActive()
                .Include(e => e.PurchasOrder)
                .Include(e => e.PurchasOrderItemVersions)
                    .ThenInclude(i => i.Product)
                .Include(e => e.Supplier)
                .Include(e => e.CreatedByNavigation)
                .Include(e => e.UpdatedByNavigation)
                .FirstOrDefault(e => e.PurchasOrderVersionID == id);

            if (purchaseOrder == null)
            {
                return NotFound();
            }

            var versions = _purchaseOrderVersionRepository.AllActive()
                .Where(e => e.PurchasOrderID == purchaseOrder.PurchasOrderID)
                .Select(e => new PurchaseOrderVersionViewModel
                {
                    id = e.PurchasOrderVersionID,
                    number = e.PurchasOrder.POID,
                    version = 1,
                    draft = e.IsDraft,
                    draftSign = e.IsDraft ? "(Draft)" : "",
                    finalSign = "",
                    current = e.PurchasOrderVersionID == id ? "current" : ""
                }).ToList();

            var supplier = _supplierRepository.AllActive()
                .Where(s => s.SupplierID == purchaseOrder.SupplierID)
                .Select(s => new SupplierDetailsViewModel
                {
                    Id = s.SupplierID,
                    CompanyName = s.FullName,
                    ContactName = "s.ContactName",
                    Email = "s.Email",
                    Phone = "s.Phone",
                    AddressLine1 = "s.Address",
                    AddressLine2 = "s.Address2",
                    TaxNumber = "s.TaxNumber"
                }).FirstOrDefault();

            var billingAddress = purchaseOrder.OBBillingAddressID.HasValue
                ? _addressRepository.AllActive()
                    .Where(a => a.PurOrderBaseSAddressID == purchaseOrder.OBBillingAddressID)
                    .Select(a => new AddressDetailsViewModel
                    {
                        Id = a.PurOrderBaseSAddressID,
                        FullName = a.FirstName + " " + a.LastName,
                        FullAddress = a.FullAddress,
                        City = a.City,
                        State = a.State,
                        PostalCode = a.PostalCode,
                        Phone = a.Phone,
                        Email = a.Email
                    }).FirstOrDefault()
                : new AddressDetailsViewModel();

            var shippingAddress = purchaseOrder.OBShipingAddressID.HasValue
                ? _addressRepository.AllActive()
                    .Where(a => a.PurOrderBaseSAddressID == purchaseOrder.OBShipingAddressID)
                    .Select(a => new AddressDetailsViewModel
                    {
                        Id = a.PurOrderBaseSAddressID,
                        FullName = a.FirstName + " " + a.LastName,
                        FullAddress = a.FullAddress,
                        City = a.City,
                        State = a.State,
                        PostalCode = a.PostalCode,
                        Phone = a.Phone,
                        Email = a.Email
                    }).FirstOrDefault()
                : new AddressDetailsViewModel();

           
           

            var vm = new PurchaseOrderDetailsViewModel
            {
                Id = purchaseOrder.PurchasOrderVersionID,
                POID = purchaseOrder.PurchasOrder?.POID,
                PurchaseDate = purchaseOrder.PurchaseDate,
                DueDate = purchaseOrder.DueDate,
                OtherReference = purchaseOrder.OtherReference,
                WorkorderNo = purchaseOrder.WorkorderNo,
                WorkOrderDate = purchaseOrder.WorkOrderDate,
                SelectedSupplierId = purchaseOrder.SupplierID,
                SelectedShippingAddressId = purchaseOrder.OBShipingAddressID,
                Items = purchaseOrder.PurchasOrderItemVersions.Select(m => new PurchaseOrderItemDetails
                {
                    SL = m.PurchasOrderVersionItemID,
                    ProductId = m.ProductID,
                    ProductName = m.Product?.ProductName ?? "",
                    Quantity = m.Quantity ?? 0m,
                    UnitPrice = m.UnitPrice ?? 0m
                }).ToList(),
                TaxPercent = purchaseOrder.TaxPercent ?? 0m,
                Note = purchaseOrder.Note,
                TermsAndConditions = purchaseOrder.TermsAndConditions,
                Status = PurchaseOrderStatus.Draft,
                CreatedByName = purchaseOrder.CreatedByNavigation != null
                    ? purchaseOrder.CreatedByNavigation.FirstName + " " + purchaseOrder.CreatedByNavigation.LastName
                    : "Unknown",
                CreatedAt = purchaseOrder.CreatedAt,
                UpdatedByName = purchaseOrder.UpdatedByNavigation != null
                    ? purchaseOrder.UpdatedByNavigation.FirstName + " " + purchaseOrder.UpdatedByNavigation.LastName
                    : "Unknown",
                UpdatedAt = purchaseOrder.UpdatedAt,
                SupplierData = supplier ?? new SupplierDetailsViewModel(),
                BillingAddress = billingAddress,
                ShippingAddress = shippingAddress
            };

            vm.SubTotal = vm.Items.Sum(i => i.Amount);
            vm.TaxAmount = vm.SubTotal * vm.TaxPercent / 100;
            vm.GrandTotal = vm.SubTotal + vm.TaxAmount;

            var sidebarVm = new PurchaseOrderSidebarDetailsViewModel
            {
                PurchaseOrderId = vm.Id,
                PurchaseOrderIdList = versions,
                PurchaseOrderNumber = vm.POID,
                Status = vm.Status,
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

            ViewBag.IsEditMode = true;

            var purchaseOrder = _purchaseOrderVersionRepository.AllActive()
                .Include(e => e.PurchasOrder)
                .Include(e => e.PurchasOrderItemVersions)
                    .ThenInclude(i => i.Product)
                .Include(e => e.Supplier)
                .Include(e => e.CreatedByNavigation)
                .Include(e => e.UpdatedByNavigation)
                .FirstOrDefault(e => e.PurchasOrderVersionID == id);

            if (purchaseOrder == null)
            {
                return NotFound();
            }

            var versions = _purchaseOrderVersionRepository.AllActive()
                .Where(e => e.PurchasOrderID == purchaseOrder.PurchasOrderID)
                .Select(e => new PurchaseOrderVersionViewModel
                {
                    id = e.PurchasOrderVersionID,
                    number = e.PurchasOrder.POID,
                    version = 1,
                    draft = e.IsDraft,
                    draftSign = e.IsDraft ? "(Draft)" : "",
                    finalSign = "",
                    current = e.PurchasOrderVersionID == id ? "current" : ""
                }).ToList();

            var vm = new PurchaseOrderDetailsViewModel
            {
                Id = purchaseOrder.PurchasOrderVersionID,
                POID = purchaseOrder.PurchasOrder.POID,
                PurchaseDate = purchaseOrder.PurchaseDate,
                DueDate = purchaseOrder.DueDate,
                OtherReference = purchaseOrder.OtherReference,
                WorkorderNo = purchaseOrder.WorkorderNo,
                WorkOrderDate = purchaseOrder.WorkOrderDate,
                SelectedSupplierId = purchaseOrder.SupplierID,
                SelectedShippingAddressId = purchaseOrder.OBShipingAddressID,
                Items = purchaseOrder.PurchasOrderItemVersions.Select(m => new PurchaseOrderItemDetails
                {
                    SL = m.PurchasOrderVersionItemID,
                    ProductId = m.ProductID,
                    ProductName = m.Product?.ProductName ?? "",
                    Quantity = m.Quantity ?? 0m,
                    UnitPrice = m.UnitPrice ?? 0m
                }).ToList(),
                TaxPercent = purchaseOrder.TaxPercent ?? 0m,
                Note = purchaseOrder.Note,
                TermsAndConditions = purchaseOrder.TermsAndConditions,
                Status = PurchaseOrderStatus.Draft,
                CreatedByName = purchaseOrder.CreatedByNavigation != null
                    ? purchaseOrder.CreatedByNavigation.FirstName + " " + purchaseOrder.CreatedByNavigation.LastName
                    : "Unknown",
                CreatedAt = purchaseOrder.CreatedAt,
                UpdatedByName = purchaseOrder.UpdatedByNavigation != null
                    ? purchaseOrder.UpdatedByNavigation.FirstName + " " + purchaseOrder.UpdatedByNavigation.LastName
                    : "Unknown",
                UpdatedAt = purchaseOrder.UpdatedAt
            };

            vm.SubTotal = vm.Items.Sum(i => i.Amount);
            vm.TaxAmount = vm.SubTotal * vm.TaxPercent / 100;
            vm.GrandTotal = vm.SubTotal + vm.TaxAmount;

            var sidebarVm = new PurchaseOrderSidebarDetailsViewModel
            {
                PurchaseOrderId = vm.Id,
                PurchaseOrderIdList = versions,
                PurchaseOrderNumber = vm.POID,
                Status = vm.Status,
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
            await _purchaseOrderRepository.OpenTransactionAsync();

            try
            {
                var version = _purchaseOrderVersionRepository.AllActive()
                    .Include(q => q.PurchasOrder)
                    .Include(q => q.PurchasOrderItemVersions)
                    .Where(q => q.PurchasOrderVersionID == id)
                    .FirstOrDefault();

                if (version == null)
                {
                    return Json(new { success = false, message = "Purchase Order not found" });
                }

                var duplicate = new PurchasOrders
                {
                    POID = await _purchaseOrderService.GetNextPOCode(),
                    CreatedBy = vm.CreatedBy,
                    CreatedAt = DateTime.Now
                };

                await _purchaseOrderRepository.AddAsync(duplicate, vm);
                await _userInfoService.ActionLogAsync("PurchaseOrderDup", ActionName.DataAdd, null, duplicate, duplicate.PurchasOrderID, vm);

                var duplicateVersion = new PurchasOrderVersions
                {
                    PurchasOrderID = duplicate.PurchasOrderID,
                    SupplierID = version.SupplierID,
                    PurchaseDate = DateTime.Now,
                    DueDate = version.DueDate,
                    OtherReference = version.OtherReference,
                    WorkorderNo = version.WorkorderNo,
                    WorkOrderDate = version.WorkOrderDate,
                    TaxPercent = version.TaxPercent,
                    Note = version.Note + " (Copy)",
                    TermsAndConditions = version.TermsAndConditions,
                    IsDraft = version.IsDraft,
                    CreatedBy = vm.CreatedBy,
                    CreatedAt = DateTime.Now
                };

                await _purchaseOrderVersionRepository.AddAsync(duplicateVersion, vm);
                await _userInfoService.ActionLogAsync("PurchaseOrderDup", ActionName.DataAdd, null, duplicateVersion, duplicateVersion.PurchasOrderVersionID, vm);

                foreach (var item in version.PurchasOrderItemVersions)
                {
                    var duplicateItem = new PurchasOrderItemVersions
                    {
                        PurchasOrderVersionID = duplicateVersion.PurchasOrderVersionID,
                        ProductID = item.ProductID,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        CreatedBy = vm.CreatedBy,
                        CreatedAt = DateTime.Now
                    };

                    await _purchaseOrderItemRepository.AddAsync(duplicateItem, vm);
                    await _userInfoService.ActionLogAsync("PurchaseOrderDup", ActionName.DataAdd, null, duplicateItem, duplicateItem.PurchasOrderVersionItemID, vm);
                }

                await _purchaseOrderItemRepository.CompleteTransactionAsync();

                return Json(new { success = true, newPurchaseOrderId = duplicateVersion.PurchasOrderVersionID });
            }
            catch (Exception ex)
            {
                await _purchaseOrderItemRepository.AbortTransactionAsync();
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, int status, BaseViewModel baseView)
        {
            try
            {
                var purchaseOrder = await _purchaseOrderVersionRepository.All().FirstOrDefaultAsync(e => e.PurchasOrderVersionID == id);
                if (purchaseOrder == null)
                {
                    return Json(new { success = false, message = "Purchase Order not found" });
                }

                //purchaseOrder.StatusID = status;
                await _purchaseOrderVersionRepository.UpdateAsync(purchaseOrder);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
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

        private int GetCurrentUserId()
        {
            return 1;
        }
    }
}
