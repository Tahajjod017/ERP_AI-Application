
using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Sales.PriceQuotationDetails;
using GCTL.Core.ViewModels.POS.Sales.SalesOrders;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.Statuse;
using GCTL.Service.POS.Sales.InvoiceF;
using GCTL.Service.POS.Sales.SalesOrderF;
using GCTL.Service.POS.Sales.Shipment;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers.POS.Sales.SalesOrderC
{
    public class SalesOrderDetailsController : BaseController
    {
        #region CTOR
        private readonly IGenericRepository<Challans> _challanRepository;

        private readonly IGenericRepository<SalesOrders> _salesOrderRepository;
        private readonly IGenericRepository<SalesOrderVersionItems> _salesOrderItemRepository;
        private readonly IGenericRepository<SalesOrdersVersions> _salesOrderVersionRepository;
       // private readonly IGenericRepository<InvoicesVersions> _invoiceVersionRepository;
        private readonly ISalesOrder _salesOrderService;
        private readonly IGenericRepository<UnitTypes> _unitTypeRepository;
        private readonly IGenericRepository<Customers> _customerRepository;
        private readonly IGenericRepository<CustomerAddresses> _customerAddressRepository;
        private readonly IGenericRepository<Addresses> _addressRepository;
        private readonly IGenericRepository<PriceQuotations> _priceQuotationRepository;
        private readonly IUserInfoService _userInfoService;
        private readonly IStatusService _statusService;
        private readonly IGenericRepository<Products> _productRepository;
        private readonly IGenericRepository<Locations> _locationRepository;
        private readonly IGenericRepository<Invoices> _invoiceRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Inventory> _inventoryRepository;


        public SalesOrderDetailsController(
            ITranslateService translateService,
            IUserProfileService userProfileService,
            IGenericRepository<SalesOrders> salesOrderRepository,
            IGenericRepository<SalesOrderVersionItems> salesOrderItemRepository,
            ISalesOrder salesOrderService,
            IGenericRepository<UnitTypes> unitTypeRepository,
            IGenericRepository<Customers> customerRepository,
            IGenericRepository<CustomerAddresses> customerAddressRepository,
            IGenericRepository<Addresses> addressRepository,
            IGenericRepository<PriceQuotations> priceQuotationRepository,
            IUserInfoService userInfoService,
            //IGenericRepository<InvoicesVersions> invoiceVersionRepository,
            IGenericRepository<SalesOrdersVersions> salesOrderVersionRepository,
            IGenericRepository<Products> productRepository,
            IGenericRepository<GCTL.Data.Models.Inventory> inventoryRepository,
            IGenericRepository<Locations> locationRepository,
            IStatusService statusService,
            IGenericRepository<Invoices> invoiceRepository,
            IGenericRepository<Challans> challanRepository)
            : base(translateService, userProfileService)
        {
            _salesOrderRepository = salesOrderRepository;
            _salesOrderItemRepository = salesOrderItemRepository;
            _salesOrderService = salesOrderService;
            _unitTypeRepository = unitTypeRepository;
            _customerRepository = customerRepository;
            _customerAddressRepository = customerAddressRepository;
            _addressRepository = addressRepository;
            _priceQuotationRepository = priceQuotationRepository;
            _userInfoService = userInfoService;
            // _invoiceVersionRepository = invoiceVersionRepository;
            _salesOrderVersionRepository = salesOrderVersionRepository;
            _productRepository = productRepository;
            _inventoryRepository = inventoryRepository;
            _locationRepository = locationRepository;
            _statusService = statusService;
            _invoiceRepository = invoiceRepository;
            _challanRepository = challanRepository;
        }

        #endregion


        #region VIEW / EDIT Sales Order (Shared Logic)

        public IActionResult Index(int id) => GetSalesOrder(id, isEditMode: false);

        public IActionResult Edit(int id) => GetSalesOrder(id, isEditMode: true);


        private IActionResult GetSalesOrder(int id, bool isEditMode)
        {
            try
            {
                SetSmartPageCode(9025000);

                // Common ViewBags
                ViewBag.location = new SelectList(
                    _locationRepository.AllActive()
                        .Select(e => new { Id = e.LocationID, Name = e.LocationName + " (" + e.LocationCode + ")" }),
                    "Id", "Name");

                ViewBag.Unit = new SelectList(_unitTypeRepository.AllActive(), "UnitTypeID", "UnitTypeName");

                if (isEditMode)
                {
                    ViewBag.product = new SelectList(_productRepository.AllActive(), "ProductID", "ProductName");
                }

                ViewBag.IsEditMode = isEditMode;

                // Load core sales order with all required includes
                var salesOrder = _salesOrderVersionRepository.AllActive()
                    .Include(e => e.SalesOrderVersionItems).ThenInclude(e => e.Product)
                    .Include(e => e.Customer)
                    .Include(e => e.SalesOrders).ThenInclude(e => e.PriceQuotationVersion).ThenInclude(e => e.PriceQuotation)
                    .Include(e => e.CreatedByNavigation)
                    .Include(e => e.UpdatedByNavigation)
                    .Include(e => e.Location)
                    .Include(e => e.Challans).ThenInclude(e => e.ChallanItems)
                    .Include(e => e.Challans).ThenInclude(e => e.Status)
                    .FirstOrDefault(e => e.SalesOrdersVersionID == id);

                if (salesOrder == null)
                    return NotFound();

                // Versions list for sidebar
                var versions = _salesOrderVersionRepository.AllActive()
                    .Where(e => e.SalesOrdersID == salesOrder.SalesOrdersID)
                    .Select(e => new PriceQuotationVersionViewModel
                    {
                        id = e.SalesOrdersVersionID,
                        number = e.SalesOrders.SalesOrderNumber,
                        version = e.Version ?? 0,
                        draft = e.IsDraft,
                        draftSign = e.IsDraft == true ? "(Draft)" : "",
                        finalSign = e.IsFinal == true && e.IsDraft != true ? "(Final)" : "",
                        current = e.SalesOrdersVersionID == id ? "current" : "",
                        isFinal = e.IsFinal == true && e.IsDraft != true
                    })
                    .ToList();




                // Customer company/contact details
                var company = GetCustomerCompanyDetails(salesOrder.CustomerID);

                // Invoices (only needed in read-only mode)
                var invoices = isEditMode
                    ? new List<InvoiceInfo>()
                    : GetInvoicesForSalesOrder(id);

                // Shipments & shipment status logic
                var (shipments, hasShipmentsStarted, isFullyShipped) = GetShipmentInfo(salesOrder);

                // Permission flags (only relevant in read-only)
                bool canCreateInvoice = false, canCreatePartialInvoice = false;
                if (!isEditMode)
                {
                    canCreateInvoice = hasShipmentsStarted && salesOrder.IsFinal == true &&
                                       !invoices.Any(i => i.IsFinal && !i.IsPartial);

                    canCreatePartialInvoice = hasShipmentsStarted && salesOrder.IsFinal == true && !isFullyShipped &&
                                              !invoices.Any(i => i.IsPartial && i.IsFinal);
                }

                // Build main ViewModel
                var vm = BuildSalesOrderViewModel(salesOrder, company, isEditMode);

                // Sidebar ViewModel
                var sidebarVm = new SalesOrderSidebarDetailsViewModel
                {
                    SalesOrderId = vm.Id,
                    SalesOrderIdList = versions,
                    SalesOrderNumber = vm.OrderNumber,
                    QuotationNumber = vm.QuotationNumber,
                    QuotationId = vm.QuotationId,
                    CreatedByName = vm.CreatedByName,
                    CreatedAt = vm.CreatedAt,
                    UpdatedByName = vm.UpdatedByName,
                    UpdatedAt = vm.UpdatedAt,
                    Shipments = shipments,
                    HasShipmentsStarted = hasShipmentsStarted,
                    IsFullyShipped = isFullyShipped,
                    CanCreateInvoice = canCreateInvoice,
                    CanCreatePartialInvoice = canCreatePartialInvoice,
                    Invoices = invoices,
                    CanMakeFinal = !versions.First(v => v.id == vm.Id).isFinal,
                    CanCreateShipment = salesOrder.IsFinal == true && shipments.Any()
                };

                vm.CanEdit = !hasShipmentsStarted;
                ViewBag.SidebarData = sidebarVm;

                return View("Index", vm);
            }
            catch (Exception)
            {
                throw;
            }
        }



        #endregion

        #region Helper Methods

        private object GetCustomerCompanyDetails(int? customerId)
        {
            return (from customer in _customerRepository.AllActive()
                    join custAddr in _customerAddressRepository.AllActive() on customer.CustomerID equals custAddr.CustomerID
                    join address in _addressRepository.AllActive() on custAddr.AddressID equals address.AddressID
                    where customer.CustomerID == customerId
                    select new
                    {
                        Id = customer.CustomerID,
                        CompanyName = customer.FullName,
                        ContactName = address.FirstName + " " + address.LastName,
                        Email = address.Email,
                        Phone = address.Phone,
                        AddressLine1 = address.FullAddress,
                        AddressLine2 = address.Additionaladdress,
                        TaxNumber = address.OtherPhone
                    }).FirstOrDefault();
        }

        private List<InvoiceInfo> GetInvoicesForSalesOrder(int salesOrderVersionId)
        {
            return _invoiceRepository.AllActive()
                .Include(i => i.InvoiceStatus)
                .Where(i => i.SalesOrderVersionID == salesOrderVersionId)
                .Select(i => new InvoiceInfo
                {
                    InvoiceId = i.InvoiceID,
                    InvoiceNumber = i.InvoiceNumber,
                    InvoiceDate = i.InvoiceDate,
                    GrandTotal = i.GrandTotal ?? 0,
                    PaidAmount = i.PaidAmount ?? 0,
                    IsDraft = i.IsDraft ?? false,
                    IsFinal = i.IsFinal != null ? i.IsFinal : false,
                    InvoiceStatusID = i.InvoiceStatusID,
                    StatusName = i.InvoiceStatus != null ? i.InvoiceStatus.StatusName : "Draft"
                })
                .OrderByDescending(i => i.InvoiceDate)
                .ToList();
        }

        private (List<ShipmentInfo> shipments, bool hasShipmentsStarted, bool isFullyShipped) GetShipmentInfo(SalesOrdersVersions salesOrder)
        {
            var activeChallans = salesOrder.Challans?.Where(c => c.DeletedAt == null).ToList() ?? new List<Challans>();

            var shipments = activeChallans.Select(s => new ShipmentInfo
            {
                ShipmentId = s.ChallanID,
                ShipmentNumber = s.ChallanNumber,
                Status = s.Status?.StatusName ?? "Pending",
                StatusClass = GetShipmentStatusClass(s.Status?.StatusName ?? "Pending")
            }).ToList();

            bool hasShipmentsStarted = activeChallans.Any();

            var cancelledStatusId = _statusService.GetStatusID("Cancelled");

            bool isFullyShipped = hasShipmentsStarted && salesOrder.SalesOrderVersionItems.All(soi =>
            {
                decimal orderedQty = soi.Quantity ?? 0;
                decimal shippedQty = activeChallans
                    .Where(c => c.StatusID != cancelledStatusId)
                    .SelectMany(c => c.ChallanItems)
                    .Where(ci => ci.ProductID == soi.ProductID && ci.DeletedAt == null)
                    .Sum(ci => ci.DeliveredQuantity ?? 0);

                return orderedQty <= shippedQty;
            });

            return (shipments, hasShipmentsStarted, isFullyShipped);
        }

        private SalesOrderDetailsViewModel BuildSalesOrderViewModel(SalesOrdersVersions salesOrder, object company, bool isEditMode)
        {
            var items = salesOrder.SalesOrderVersionItems.Select(m => new SalesOrderItemDetails
            {
                SL = m.SalesOrderVersionItemID,
                Description = m.Description ?? "",
                Product = isEditMode ? m.ProductID ?? 0 : 0, // Only needed in edit mode
                UnitName = isEditMode ? "" : m.Product?.ProductName ?? "",
                Area = m.Area ?? 0m,
                Rate = m.Rate ?? 0m,
                Quantity = m.Quantity ?? 0m
            }).ToList();

            var vm = new SalesOrderDetailsViewModel
            {
                Id = salesOrder.SalesOrdersVersionID,
                OrderDate = salesOrder.SalesOrderDate,
                OrderNumber = salesOrder.SalesOrders?.SalesOrderNumber ?? "",
                SelectedCustomerId = salesOrder.CustomerID ?? 0,
                SelectedQuotationId = salesOrder.SalesOrders?.PriceQuotationVersionID ?? 0,
                QuotationNumber = salesOrder.SalesOrders?.PriceQuotationVersion?.PriceQuotation?.QuotationNumber ?? "",
                QuotationId = salesOrder.SalesOrders?.PriceQuotationVersionID ?? 0,
                Items = items,
                VatPercent = salesOrder.VatPercentage ?? 0m,
                Note = salesOrder.Note ?? "",
                LocationId = isEditMode ? (salesOrder.LocationID ?? 0) : 0,
                LocationName = isEditMode ? "" : salesOrder.Location?.LocationName ?? "",
                CreatedByName = salesOrder.CreatedByNavigation != null
                    ? $"{salesOrder.CreatedByNavigation.FirstName} {salesOrder.CreatedByNavigation.LastName}"
                    : "Unknown",
                CreatedAt = salesOrder.CreatedAt,
                UpdatedByName = salesOrder.UpdatedByNavigation != null
                    ? $"{salesOrder.UpdatedByNavigation.FirstName} {salesOrder.UpdatedByNavigation.LastName}"
                    : "Unknown",
                UpdatedAt = salesOrder.UpdatedAt,
                CustomerData = new CustomerDetailsViewModel
                {
                    CompanyName = company?.GetType().GetProperty("CompanyName")?.GetValue(company)?.ToString() ?? "",
                    AddressLine1 = company?.GetType().GetProperty("AddressLine1")?.GetValue(company)?.ToString() ?? "",
                    ContactName = company?.GetType().GetProperty("ContactName")?.GetValue(company)?.ToString() ?? "",
                    Email = company?.GetType().GetProperty("Email")?.GetValue(company)?.ToString() ?? "",
                    Phone = company?.GetType().GetProperty("Phone")?.GetValue(company)?.ToString() ?? ""
                }
            };

            vm.SubTotal = items.Sum(i => i.Amount);
            vm.VatAmount = vm.SubTotal * vm.VatPercent / 100;
            vm.GrandTotal = vm.SubTotal - vm.VatAmount; // Note: your original code subtracts VAT – confirm if intentional

            return vm;
        }

        private string GetShipmentStatusClass(string? statusId)
        {
            return statusId switch
            {
                "Pending" => "warning",  // Pending
                "Packed" => "info",     // Packed
                "Shipped" => "primary",  // Shipped
                "In Transit" => "primary",  // In Transit
                "Delivered" => "success",  // Delivered
                "Cancelled" => "danger",   // Cancelled
                _ => "secondary"
            };
        }



        #endregion






        #region Sidebar Actions

        [HttpPost]
        public async Task<IActionResult> Duplicate(int id, BaseViewModel vm)
        {
            await _salesOrderRepository.BeginTransactionAsync();

            try
            {
                // Load original order with version + items
                var original = await _salesOrderVersionRepository.AllActive()
                    .Include(e => e.SalesOrderVersionItems)
                    .Include(e => e.SalesOrders)
                    .FirstOrDefaultAsync(e => e.SalesOrdersVersionID == id);

                if (original == null)
                {
                    return Json(new { success = false, message = "Sales Order not found" });
                }

                // Create duplicate SalesOrder header
                var duplicateOrder = new SalesOrders
                {
                    SalesOrderNumber = await _salesOrderService.GetNextSOcode(),
                    PriceQuotationVersionID = original.SalesOrders.PriceQuotationVersionID,
                    CreatedAt = DateTime.Now,
                    CreatedBy = vm.CreatedBy
                };

                await _salesOrderRepository.AddAsync(duplicateOrder, vm);
                await _userInfoService.ActionLogAsync("SalesOrderDup", ActionName.DataAdd, null, duplicateOrder, duplicateOrder.SalesOrdersID, vm);

                // Create duplicate version
                var duplicateVersion = new SalesOrdersVersions
                {
                    SalesOrdersID = duplicateOrder.SalesOrdersID,
                    
                   Version = 1,
                    CustomerID = original.CustomerID,
                    SalesOrderDate = DateTime.Now, // or copy original.SalesOrderDate if needed
                    VatPercentage = original.VatPercentage,
                    Note = (original.Note ?? string.Empty) + " (Copy)",
                    IsDraft = true,
                    CreatedAt = DateTime.Now,
                    CreatedBy = vm.CreatedBy
                };

                await _salesOrderVersionRepository.AddAsync(duplicateVersion, vm);
                await _userInfoService.ActionLogAsync("SalesOrderVersionDup", ActionName.DataAdd, null, duplicateVersion, duplicateVersion.SalesOrdersVersionID, vm);

                // Copy items
                foreach (var item in original.SalesOrderVersionItems)
                {
                    var duplicateItem = new SalesOrderVersionItems
                    {
                        SalesOrdersVersionID = duplicateVersion.SalesOrdersVersionID,
                        Description = item.Description,
                        UnitTypeID = item.UnitTypeID,
                        ProductID = item.ProductID,
                        Area = item.Area,
                        Rate = item.Rate,
                        Quantity = item.Quantity,
                        LIP = item.LIP,
                        LMAC = item.LMAC,
                        CreatedAt = DateTime.Now,
                        CreatedBy = vm.CreatedBy
                    };

                    await _salesOrderItemRepository.AddAsync(duplicateItem, vm);
                    await _userInfoService.ActionLogAsync("SalesOrderItemDup", ActionName.DataAdd, null, duplicateItem, duplicateItem.SalesOrderVersionItemID, vm);
                }

                // Commit transaction
                await _salesOrderItemRepository.CommitTransactionAsync();

                return Json(new { success = true, newSalesOrderId = duplicateVersion.SalesOrdersVersionID });
            }
            catch (Exception ex)
            {
                await _salesOrderItemRepository.RollbackTransactionAsync();
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, int status, BaseViewModel baseView)
        {
            try
            {
                var salesOrder = await _salesOrderVersionRepository.All().Include(e=>e.SalesOrderVersionItems).FirstOrDefaultAsync(e => e.SalesOrdersVersionID == id);
                if (salesOrder == null)
                {
                    return Json(new { success = false, message = "Quotation not found" });
                }

                

                var prevFinal = await _salesOrderVersionRepository.AllActive().Include(w=>w.SalesOrderVersionItems).Where(e => e.SalesOrdersID == salesOrder.SalesOrdersID && e.IsFinal == true).FirstOrDefaultAsync();

                if (prevFinal != null)
                {
                    foreach (var item in prevFinal.SalesOrderVersionItems)
                    {
                        var inv = await _inventoryRepository.AllActive().FirstOrDefaultAsync(e => e.ProductID == item.ProductID);

                        if (inv != null)
                        {
                            inv.ReservedQuantity -= item.Quantity ?? 0;
                            await _inventoryRepository.UpdateAsync(inv);

                        }
                    }
                }


                var quora = await _salesOrderVersionRepository.AllActive().Where(e => e.SalesOrdersID == salesOrder.SalesOrdersID).ToListAsync();
                quora.ForEach(e => e.IsFinal = false);
                quora.ForEach(e => e.IsDraft = false);
                await _salesOrderVersionRepository.UpdateRangeAsync(quora);

                salesOrder.IsFinal = true;

                foreach (var item in salesOrder.SalesOrderVersionItems)
                {
                    var inv = await _inventoryRepository.AllActive().FirstOrDefaultAsync(e => e.ProductID == item.ProductID);

                    if (inv != null)
                    {
                        inv.ReservedQuantity += item.Quantity ?? 0;
                        await _inventoryRepository.UpdateAsync(inv);

                    }
                }

                await _salesOrderVersionRepository.UpdateAsync(salesOrder);




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
                var salesOrder = _salesOrderRepository.All().FirstOrDefault(e => e.SalesOrdersID == id);
                if (salesOrder == null)
                {
                    return Json(new { success = false, message = "Sales Order not found" });
                }

                // Soft delete
                salesOrder.DeletedAt = DateTime.Now;
                salesOrder.DeletedBy = GetCurrentUserId();

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
           
            return GetCurrentEmployeeIdAsync().Result ?? 1;
        }



        //#region READ-ONLY MODE - View Sales Order
        //public IActionResult Index(int id)
        //{

        //    try
        //    {
        //        SetSmartPageCode(9025000);

        //        ViewBag.location = new SelectList(_locationRepository.AllActive().Select(e => new { Id = e.LocationID, Name = e.LocationName + " (" + e.LocationCode + ")" }).ToList(), "Id", "Name");

        //        ViewBag.Unit = new SelectList(_unitTypeRepository.AllActive().ToList(), "UnitTypeID", "UnitTypeName");
        //        ViewBag.IsEditMode = false; // Read-only mode

        //        var salesOrder = _salesOrderVersionRepository.AllActive()
        //            .Include(e => e.SalesOrderVersionItems).ThenInclude(e => e.Product)
        //            .Include(e => e.Customer)
        //            .Include(e => e.SalesOrders).ThenInclude(e => e.PriceQuotationVersion).ThenInclude(e=>e.PriceQuotation)
        //             .Include(e => e.SalesOrders)
        //             .Include(e => e.Challans).ThenInclude(e => e.ChallanItems) // ADD THIS for shipments
        //             .Include(e => e.Challans).ThenInclude(e => e.Status) // ADD THIS for shipments

        //            .Include(e => e.CreatedByNavigation)
        //            .Include(e => e.UpdatedByNavigation)
        //            .Include(e => e.Location)
        //            .FirstOrDefault(e => e.SalesOrdersVersionID == id);

        //        if (salesOrder == null)
        //        {
        //            return NotFound();
        //        }

        //        var versions = _salesOrderVersionRepository.AllActive().Where(e => e.SalesOrdersID == salesOrder.SalesOrdersID).Select(e => new PriceQuotationVersionViewModel
        //        {
        //            id = e.SalesOrdersVersionID,
        //            number = e.SalesOrders.SalesOrderNumber,
        //            version = e.Version ?? 0,
        //            draft = e.IsDraft,
        //            draftSign = e.IsDraft != true ? "" : "(Draft)",
        //            finalSign = e.IsFinal == true && e.IsDraft != true ? "(Final)" : "",
        //            current = e.SalesOrdersVersionID == id ? "current" : "",
        //            isFinal = e.IsFinal == true && e.IsDraft != true ? true : false,
        //        }).ToList();

        //        var company = (from customer in _customerRepository.AllActive()
        //                       join custAddr in _customerAddressRepository.AllActive()
        //                           on customer.CustomerID equals custAddr.CustomerID
        //                       join address in _addressRepository.AllActive()
        //                           on custAddr.AddressID equals address.AddressID
        //                       where customer.CustomerID == salesOrder.CustomerID
        //                       select new
        //                       {
        //                           Id = customer.CustomerID,
        //                           CompanyName = customer.FullName,
        //                           ContactName = address.FirstName + " " + address.LastName,
        //                           address.Email,
        //                           address.Phone,
        //                           AddressLine1 = address.FullAddress,
        //                           AddressLine2 = address.Additionaladdress,
        //                           TaxNumber = address.OtherPhone
        //                       }).FirstOrDefault();




        //        var InvoiceID = _invoiceRepository.AllActive().Where(e=>e.SalesOrderVersionID == salesOrder.SalesOrdersVersionID).Select(e=>e.InvoiceID).FirstOrDefault(); 

        //        var challan = _challanRepository.AllActive().Where(e => e.InvoiceID == InvoiceID || e.SalesOrdersVersionID == salesOrder.SalesOrdersVersionID).Select(s => new ShipmentInfo
        //        {
        //            ShipmentId = s.ChallanID,
        //            ShipmentNumber = s.ChallanNumber,
        //            Status = s.Status != null ? s.Status.StatusName : "Pending",
        //            StatusClass = GetShipmentStatusClass("Pending"),

        //        }).ToList() ?? new List<ShipmentInfo>();

        //        var challanData = _challanRepository.AllActive().Include(e => e.ChallanItems).Where(e => e.InvoiceID == InvoiceID || e.SalesOrdersVersionID == salesOrder.SalesOrdersVersionID).ToList();



        //        bool isFullyShipped = false;
        //        bool hasShipmentsStarted = false;

        //        if (salesOrder.Challans != null && salesOrder.Challans.Any(s => s.DeletedAt == null))
        //        {
        //            hasShipmentsStarted = true;

        //            var cancelledStatusId = _statusService.GetStatusID("Cancelled");

        //            isFullyShipped = salesOrder.SalesOrderVersionItems.All(soi =>
        //            {
        //                decimal orderedQty = soi.Quantity ?? 0;

        //                decimal shippedQty = salesOrder.Challans
        //                    .Where(s => s.StatusID != cancelledStatusId && s.DeletedAt == null)
        //                    .SelectMany(s => s.ChallanItems)
        //                    .Where(si => si.ProductID == soi.ProductID && si.DeletedAt == null)
        //                    .Sum(si => si.DeliveredQuantity ?? 0);

        //                return orderedQty <= shippedQty;
        //            });


        //        }



        //        // Populate invoices with status
        //        var invoices = _invoiceRepository.AllActive()
        //            .Include(i => i.InvoiceStatus) // Include status
        //           // .Include(i => i.PartialForInvoice) // Include parent invoice for partials
        //            .Where(i => i.SalesOrderVersionID == id) // || i.SalesOrdersID == salesOrder.SalesOrdersID)
        //            .Select(i => new InvoiceInfo
        //            {
        //                InvoiceId = i.InvoiceID,
        //                InvoiceNumber = i.InvoiceNumber,
        //                InvoiceDate = i.InvoiceDate,
        //                GrandTotal = i.GrandTotal ?? 0,
        //                PaidAmount = i.PaidAmount ?? 0,
        //                IsDraft = i.IsDraft ?? false,
        //                IsFinal = i.IsFinal != null ? i.IsFinal : false,
        //                InvoiceStatusID = i.InvoiceStatusID,
        //                StatusName = i.InvoiceStatus != null ? i.InvoiceStatus.StatusName : "Draft",
        //                //StatusClass = GetInvoiceStatusClass(i.InvoiceStatusID),

        //            })
        //            .OrderByDescending(i => i.InvoiceDate)
        //            .ToList();

        //        // Calculate invoice creation permissions
        //        bool canCreateInvoice = hasShipmentsStarted &&
        //                               salesOrder.IsFinal == true &&
        //                               !invoices.Any(i => i.IsFinal && !i.IsPartial); // No final invoice exists

        //        bool canCreatePartialInvoice = hasShipmentsStarted &&
        //                                      salesOrder.IsFinal == true &&
        //                                      !isFullyShipped &&
        //                                      !invoices.Any(i => i.IsPartial && i.IsFinal); // No partial final invoice



        //        var vm = new SalesOrderDetailsViewModel
        //        {
        //            Id = salesOrder?.SalesOrdersVersionID ?? 0,
        //            OrderDate = salesOrder?.SalesOrderDate ?? DateTime.MinValue,
        //            OrderNumber = salesOrder?.SalesOrders?.SalesOrderNumber ?? "",
        //            SelectedCustomerId = salesOrder?.CustomerID ?? 0,
        //            SelectedQuotationId = salesOrder?.SalesOrders?.PriceQuotationVersionID ?? 0,
        //            QuotationNumber = salesOrder?.SalesOrders?.PriceQuotationVersion?.PriceQuotation?.QuotationNumber ?? "",
        //            QuotationId = salesOrder?.SalesOrders?.PriceQuotationVersionID ?? 0,
        //            Items = salesOrder?.SalesOrderVersionItems?.Select(m => new SalesOrderItemDetails
        //            {
        //                SL = m.SalesOrderVersionItemID,
        //                Description = m.Description ?? "",
        //                UnitName = m.Product?.ProductName ?? "",
        //                Area = m.Area ?? 0m,
        //                Rate = m.Rate ?? 0m,
        //                Quantity = m.Quantity ?? 0m
        //            }).ToList() ?? new List<SalesOrderItemDetails>(),
        //            VatPercent = salesOrder?.VatPercentage ?? 0m,
        //            Note = salesOrder?.Note ?? "",
        //            LocationName = salesOrder?.Location?.LocationName ?? "",
        //            CreatedByName = salesOrder?.CreatedByNavigation != null ? $"{salesOrder.CreatedByNavigation.FirstName} {salesOrder.CreatedByNavigation.LastName}" : "Unknown", CreatedAt = salesOrder?.CreatedAt,
        //            UpdatedByName = salesOrder?.UpdatedByNavigation != null ? $"{salesOrder.UpdatedByNavigation.FirstName} {salesOrder.UpdatedByNavigation.LastName}" : "",
        //            UpdatedAt = salesOrder?.UpdatedAt,
        //            CustomerData = new CustomerDetailsViewModel()
        //            {
        //                CompanyName = company?.CompanyName ?? "",
        //                AddressLine1 = company?.AddressLine1 ?? "",
        //                ContactName = company?.ContactName ?? "",
        //                Email = company?.Email ?? "",
        //                Phone = company?.Phone ?? ""
        //            }
        //        };

        //        // Calculate totals for display
        //        vm.SubTotal = vm.Items.Sum(i => i.Amount);
        //        vm.VatAmount = vm.SubTotal * vm.VatPercent / 100;
        //        vm.GrandTotal = vm.SubTotal - vm.VatAmount;

        //        // Create sidebar view model
        //        var sidebarVm = new SalesOrderSidebarDetailsViewModel
        //        {
        //            SalesOrderId = vm.Id,
        //            SalesOrderIdList = versions,
        //            SalesOrderNumber = vm.OrderNumber,
        //            QuotationNumber = vm.QuotationNumber,
        //            QuotationId = vm.QuotationId,
        //            CreatedByName = vm.CreatedByName,
        //            CreatedAt = vm.CreatedAt,
        //            UpdatedByName = vm.UpdatedByName,
        //            UpdatedAt = vm.UpdatedAt,
        //            Shipments = challan, // ADD THIS
        //                                   //Status = salesOrder.IsDraft == true ? SalesOrderStatus.Draft : SalesOrderStatus.Confirmed // ADD THIS (adjust based on your status logic)
        //            CanMakeFinal = !versions.Where(e => e.id == vm.Id).Select(e => e.isFinal).FirstOrDefault(), // ✅ true if not current

        //            HasShipmentsStarted = hasShipmentsStarted,
        //            IsFullyShipped = isFullyShipped,
        //            CanCreateInvoice = canCreateInvoice,

        //            Invoices = invoices,

        //            CanCreatePartialInvoice = canCreatePartialInvoice,


        //        };

        //        if (salesOrder?.IsFinal == true)
        //        {
        //            sidebarVm.CanCreateShipment = (challan.Count != 0 || challan != null) ? true : false;

        //        }
        //        vm.CanEdit = !sidebarVm.HasShipments;

        //        ViewBag.SidebarData = sidebarVm;

        //        return View(vm);
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }


        //}
        //#endregion

        //#region EDIT MODE - Edit Sales Order
        //public IActionResult Edit(int id)
        //{
        //    ViewBag.location = new SelectList(_locationRepository.AllActive().Select(e => new { Id = e.LocationID, Name = e.LocationName + " (" + e.LocationCode + ")" }).ToList(), "Id", "Name");

        //    ViewBag.product = new SelectList(_productRepository.AllActive().ToList(), "ProductID", "ProductName");

        //    ViewBag.Unit = new SelectList(_unitTypeRepository.AllActive().ToList(), "UnitTypeID", "UnitTypeName");
        //    ViewBag.IsEditMode = true; // Edit mode

        //    var salesOrder = _salesOrderVersionRepository.AllActive()
        //        .Include(e => e.SalesOrderVersionItems)
        //        .Include(e => e.CreatedByNavigation)
        //        .Include(e => e.UpdatedByNavigation)
        //        .Include(e => e.SalesOrders).ThenInclude(e=>e.PriceQuotationVersion).ThenInclude(e => e.PriceQuotation)
        //        .Include(e => e.SalesOrders)
        //        .Include(e => e.Challans) // ADD THIS
        //        .ThenInclude(s => s.Status) // ADD THIS
        //        .FirstOrDefault(e => e.SalesOrdersVersionID == id);

        //    if (salesOrder == null)
        //    {
        //        return NotFound();
        //    }


        //    var versions = _salesOrderVersionRepository.AllActive().Where(e => e.SalesOrdersID == salesOrder.SalesOrdersID).Select(e => new PriceQuotationVersionViewModel
        //    {
        //        id = e.SalesOrdersVersionID,
        //        number = e.SalesOrders.SalesOrderNumber,
        //        version = e.Version ?? 0,
        //        draft = e.IsDraft,
        //        draftSign = e.IsDraft != true ? "" : "(Draft)",
        //        finalSign = e.IsFinal == true && e.IsDraft != true ? "(Final)" : "",
        //        current = e.SalesOrdersVersionID == id ? "current" : ""
        //    }).ToList();

        //    var shipments = salesOrder.Challans?
        //        .Where(s => s.DeletedAt == null)
        //        .Select(s => new ShipmentInfo
        //        {
        //            ShipmentId = s.ChallanID,
        //            ShipmentNumber = s.ChallanNumber,
        //            Status = s.Status != null ? s.Status.StatusName : "Pending",
        //            StatusClass = GetShipmentStatusClass(s.Status != null ? s.Status.StatusName : "Pending")
        //        }).ToList() ?? new List<ShipmentInfo>();




        //    var vm = new SalesOrderDetailsViewModel
        //    {
        //        Id = salesOrder?.SalesOrdersVersionID ?? 0,
        //        OrderDate = salesOrder?.SalesOrderDate ?? DateTime.MinValue,
        //        OrderNumber = salesOrder?.SalesOrders?.SalesOrderNumber ?? "",
        //        SelectedCustomerId = salesOrder?.CustomerID ?? 0,
        //        SelectedQuotationId = salesOrder?.SalesOrders?.PriceQuotationVersionID ?? 0,
        //        QuotationNumber = salesOrder?.SalesOrders?.PriceQuotationVersion?.PriceQuotation?.QuotationNumber ?? "",
        //        QuotationId = salesOrder?.SalesOrders?.PriceQuotationVersionID ?? 0,
        //        Items = salesOrder?.SalesOrderVersionItems?.Select(m => new SalesOrderItemDetails
        //        {
        //            SL = m.SalesOrderVersionItemID,
        //            Description = m.Description ?? "",
        //            Product = m.ProductID ?? 0,
        //            Area = m.Area ?? 0m,
        //            Rate = m.Rate ?? 0m,
        //            Quantity = m.Quantity ?? 0m
        //        }).ToList() ?? new List<SalesOrderItemDetails>(),
        //        VatPercent = salesOrder?.VatPercentage ?? 0m,
        //        Note = salesOrder?.Note ?? "",
        //        LocationId = salesOrder?.LocationID ?? 0,
        //        CreatedByName = salesOrder?.CreatedByNavigation != null
        //    ? $"{salesOrder.CreatedByNavigation.FirstName} {salesOrder.CreatedByNavigation.LastName}"
        //    : "Unknown",
        //        CreatedAt = salesOrder?.CreatedAt ?? DateTime.MinValue,
        //        UpdatedByName = salesOrder?.UpdatedByNavigation != null
        //    ? $"{salesOrder.UpdatedByNavigation.FirstName} {salesOrder.UpdatedByNavigation.LastName}"
        //    : "Unknown",
        //        UpdatedAt = salesOrder?.UpdatedAt ?? DateTime.MinValue
        //    };




        //    // Calculate totals for display
        //    vm.SubTotal = vm.Items.Sum(i => i.Amount);
        //    vm.VatAmount = vm.SubTotal * vm.VatPercent / 100;
        //    vm.GrandTotal = vm.SubTotal - vm.VatAmount;

        //    // Create sidebar view model
        //    var sidebarVm = new SalesOrderSidebarDetailsViewModel
        //    {
        //        SalesOrderId = vm.Id,
        //        SalesOrderIdList = versions,
        //        SalesOrderNumber = vm.OrderNumber,
        //        QuotationNumber = vm.QuotationNumber,
        //        QuotationId = vm.QuotationId,
        //        CreatedByName = vm.CreatedByName,
        //        CreatedAt = vm.CreatedAt,
        //        UpdatedByName = vm.UpdatedByName,
        //        UpdatedAt = vm.UpdatedAt,
        //        Shipments = shipments,
        //    };


        //    if (salesOrder.IsFinal == true)
        //    {
        //        sidebarVm.CanCreateShipment = salesOrder != null ? false : true;
        //    }


        //    ViewBag.SidebarData = sidebarVm;

        //    // Return the same view but in edit mode
        //    return View("Index", vm);
        //}
        //#endregion


    }
}
