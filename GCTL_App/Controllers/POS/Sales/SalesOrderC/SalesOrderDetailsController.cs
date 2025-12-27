
using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Sales.PriceQuotationDetails;
using GCTL.Core.ViewModels.POS.Sales.SalesOrders;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Language;
using GCTL.Service.POS.Sales.SalesOrderF;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers.POS.Sales.SalesOrderC
{
    public class SalesOrderDetailsController : BaseController
    {
        #region CTOR

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
        private readonly IGenericRepository<Products> _productRepository;
        private readonly IGenericRepository<Locations> _locationRepository;
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
            IGenericRepository<Locations> locationRepository)
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
        }

        #endregion

        #region READ-ONLY MODE - View Sales Order
        public IActionResult Index(int id)
        {
            SetSmartPageCode(9025000);

            ViewBag.location = new SelectList(_locationRepository.AllActive().Select(e => new { Id = e.LocationID, Name = e.LocationName + " (" + e.LocationCode + ")" }).ToList(), "Id", "Name");

            ViewBag.Unit = new SelectList(_unitTypeRepository.AllActive().ToList(), "UnitTypeID", "UnitTypeName");
            ViewBag.IsEditMode = false; // Read-only mode

            var salesOrder = _salesOrderVersionRepository.AllActive()
                .Include(e => e.SalesOrderVersionItems).ThenInclude(e => e.Product)
                .Include(e => e.Customer)
                .Include(e => e.SalesOrders).ThenInclude(e => e.PriceQuotation)
                 .Include(e => e.SalesOrders).ThenInclude(e => e.Shipments) // ADD THIS for shipments
                 .ThenInclude(s => s.Status) // ADD THIS for shipment status
                .Include(e => e.CreatedByNavigation)
                .Include(e => e.UpdatedByNavigation)
                .Include(e => e.Location)
                .FirstOrDefault(e => e.SalesOrdersVersionID == id);

            if (salesOrder == null)
            {
                return NotFound();
            }

            var versions = _salesOrderVersionRepository.AllActive().Where(e => e.SalesOrdersID == salesOrder.SalesOrdersID).Select(e => new PriceQuotationVersionViewModel
            {
                id = e.SalesOrdersVersionID,
                number = e.SalesOrders.SalesOrderNumber,
                version = e.Version ?? 0,
                draft = e.IsDraft,
                draftSign = e.IsDraft != true ? "" : "(Draft)",
                finalSign = e.IsFinal == true && e.IsDraft != true ? "(Final)" : "",
                current = e.SalesOrdersVersionID == id ? "current" : "",
                isFinal = e.IsFinal == true && e.IsDraft != true ? true : false,
            }).ToList();

            var company = (from customer in _customerRepository.AllActive()
                           join custAddr in _customerAddressRepository.AllActive()
                               on customer.CustomerID equals custAddr.CustomerID
                           join address in _addressRepository.AllActive()
                               on custAddr.AddressID equals address.AddressID
                           where customer.CustomerID == salesOrder.CustomerID
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
                           }).FirstOrDefault();


            var shipments = salesOrder.SalesOrders?.Shipments?
                   .Where(s => s.DeletedAt == null)
                   .Select(s => new ShipmentInfo
                   {
                       ShipmentId = s.ShipmentID,
                       ShipmentNumber = s.ShipmentNumber,
                       Status = s.Status != null ? s.Status.StatusName : "Pending",
                       StatusClass = GetShipmentStatusClass(s.StatusID)
                   }).ToList() ?? new List<ShipmentInfo>();

            var vm = new SalesOrderDetailsViewModel
            {
                Id = salesOrder.SalesOrdersVersionID,
                OrderDate = salesOrder.SalesOrderDate,
                OrderNumber = salesOrder.SalesOrders.SalesOrderNumber,
                SelectedCustomerId = salesOrder.CustomerID,
                SelectedQuotationId = salesOrder.SalesOrders.PriceQuotationID,
                QuotationNumber = salesOrder.SalesOrders.PriceQuotation?.QuotationNumber,
                Items = salesOrder.SalesOrderVersionItems.Select(m => new SalesOrderItemDetails
                {
                    SL = m.SalesOrderVersionItemID,
                    Description = m.Description,
                    UnitName = m.Product != null ? m.Product.ProductName : "",
                    Area = m.Area ?? 0m,
                    Rate = m.Rate ?? 0m,
                    Quantity = m.Quantity ?? 0m,
                    //LIP = m.LIP,
                    //LMAC = m.LMAC
                }).ToList(),
                VatPercent = salesOrder.VatPercentage ?? 0m,
                Note = salesOrder.Note ?? "",
                LocationName = salesOrder.Location?.LocationName ?? "",

                // Sidebar data
                CreatedByName = salesOrder.CreatedByNavigation != null ? salesOrder.CreatedByNavigation.FirstName + " " + salesOrder.CreatedByNavigation.LastName : "Unknown",
                CreatedAt = salesOrder.CreatedAt,
               
                UpdatedByName = salesOrder.UpdatedAt != null ? salesOrder.UpdatedByNavigation != null ? salesOrder.UpdatedByNavigation.FirstName + " " + salesOrder.UpdatedByNavigation.LastName : "Unknown" : "",
                UpdatedAt = salesOrder.UpdatedAt,
                CustomerData = new CustomerDetailsViewModel()
                {
                    CompanyName = company != null ? company.CompanyName : "",
                    AddressLine1 = company != null ? company.AddressLine1 : "",
                    ContactName = company != null ? company.ContactName : "",
                    Email = company != null ? company.Email : "",
                    Phone = company != null ? company.Phone : ""
                }
            };

            // Calculate totals for display
            vm.SubTotal = vm.Items.Sum(i => i.Amount);
            vm.VatAmount = vm.SubTotal * vm.VatPercent / 100;
            vm.GrandTotal = vm.SubTotal - vm.VatAmount;

            // Create sidebar view model
            var sidebarVm = new SalesOrderSidebarDetailsViewModel
            {
                SalesOrderId = vm.Id,
                SalesOrderIdList = versions,
                SalesOrderNumber = vm.OrderNumber,
                QuotationNumber = vm.QuotationNumber,
                CreatedByName = vm.CreatedByName,
                CreatedAt = vm.CreatedAt,
                UpdatedByName = vm.UpdatedByName,
                UpdatedAt = vm.UpdatedAt,
                Shipments = shipments, // ADD THIS
                //Status = salesOrder.IsDraft == true ? SalesOrderStatus.Draft : SalesOrderStatus.Confirmed // ADD THIS (adjust based on your status logic)
                CanMakeFinal = !versions.Where(e => e.id == vm.Id).Select(e => e.isFinal).FirstOrDefault() // ✅ true if not current


            };

            if (salesOrder.IsFinal == true)
            {
                sidebarVm.CanCreateShipment = (shipments.Count != 0 || shipments != null) ? true : false;
            }


            ViewBag.SidebarData = sidebarVm;

            return View(vm);
        }
        #endregion

        #region EDIT MODE - Edit Sales Order
        public IActionResult Edit(int id)
        {
            ViewBag.location = new SelectList(_locationRepository.AllActive().Select(e => new { Id = e.LocationID, Name = e.LocationName + " (" + e.LocationCode + ")" }).ToList(), "Id", "Name");

            ViewBag.product = new SelectList(_productRepository.AllActive().ToList(), "ProductID", "ProductName");

            ViewBag.Unit = new SelectList(_unitTypeRepository.AllActive().ToList(), "UnitTypeID", "UnitTypeName");
            ViewBag.IsEditMode = true; // Edit mode

            var salesOrder = _salesOrderVersionRepository.AllActive()
                .Include(e => e.SalesOrderVersionItems)
                .Include(e => e.CreatedByNavigation)
                .Include(e => e.UpdatedByNavigation)
                .Include(e => e.SalesOrders)
                .ThenInclude(e => e.PriceQuotation)
                .Include(e => e.SalesOrders).ThenInclude(e => e.Shipments) // ADD THIS
                .ThenInclude(s => s.Status) // ADD THIS
                .FirstOrDefault(e => e.SalesOrdersVersionID == id);

            if (salesOrder == null)
            {
                return NotFound();
            }


            var versions = _salesOrderVersionRepository.AllActive().Where(e => e.SalesOrdersID == salesOrder.SalesOrdersID).Select(e => new PriceQuotationVersionViewModel
            {
                id = e.SalesOrdersVersionID,
                number = e.SalesOrders.SalesOrderNumber,
                version = e.Version ?? 0,
                draft = e.IsDraft,
                draftSign = e.IsDraft != true ? "" : "(Draft)",
                finalSign = e.IsFinal == true && e.IsDraft != true ? "(Final)" : "",
                current = e.SalesOrdersVersionID == id ? "current" : ""
            }).ToList();

            var shipments = salesOrder.SalesOrders?.Shipments?
                .Where(s => s.DeletedAt == null)
                .Select(s => new ShipmentInfo
                {
                    ShipmentId = s.ShipmentID,
                    ShipmentNumber = s.ShipmentNumber,
                    Status = s.Status != null ? s.Status.StatusName : "Pending",
                    StatusClass = GetShipmentStatusClass(s.StatusID)
                }).ToList() ?? new List<ShipmentInfo>();


            var vm = new SalesOrderDetailsViewModel
            {
                Id = salesOrder.SalesOrdersVersionID,
                OrderDate = salesOrder.SalesOrderDate,
                OrderNumber = salesOrder.SalesOrders.SalesOrderNumber,
                SelectedCustomerId = salesOrder.CustomerID,
                SelectedQuotationId = salesOrder.SalesOrders.PriceQuotationID,
                QuotationNumber = salesOrder.SalesOrders.PriceQuotation?.QuotationNumber,
                Items = salesOrder.SalesOrderVersionItems.Select(m => new SalesOrderItemDetails
                {
                    SL = m.SalesOrderVersionItemID,
                    Description = m.Description,
                    Product = m.ProductID ?? 0,
                    Area = m.Area ?? 0m,
                    Rate = m.Rate ?? 0m,
                    Quantity = m.Quantity ?? 0m,
                    //LIP = m.LIP,
                    //LMAC = m.LMAC
                }).ToList(),
                VatPercent = salesOrder.VatPercentage ?? 0m,
                Note = salesOrder.Note,
                LocationId = salesOrder.LocationID,

                // Sidebar data
                CreatedByName = salesOrder.CreatedByNavigation != null ? salesOrder.CreatedByNavigation.FirstName + " " + salesOrder.CreatedByNavigation.LastName : "Unknown",
                CreatedAt = salesOrder.CreatedAt,
                UpdatedByName = salesOrder.UpdatedByNavigation != null ? salesOrder.UpdatedByNavigation.FirstName + " " + salesOrder.UpdatedByNavigation.LastName : "Unknown",
                UpdatedAt = salesOrder.UpdatedAt,
            };

            // Calculate totals for display
            vm.SubTotal = vm.Items.Sum(i => i.Amount);
            vm.VatAmount = vm.SubTotal * vm.VatPercent / 100;
            vm.GrandTotal = vm.SubTotal - vm.VatAmount;

            // Create sidebar view model
            var sidebarVm = new SalesOrderSidebarDetailsViewModel
            {
                SalesOrderId = vm.Id,
                SalesOrderIdList = versions,
                SalesOrderNumber = vm.OrderNumber,
                QuotationNumber = vm.QuotationNumber,
                CreatedByName = vm.CreatedByName,
                CreatedAt = vm.CreatedAt,
                UpdatedByName = vm.UpdatedByName,
                UpdatedAt = vm.UpdatedAt,
                Shipments = shipments,
            };


            if (salesOrder.IsFinal == true)
            {
                sidebarVm.CanCreateShipment = salesOrder != null ? false : true;
            }


            ViewBag.SidebarData = sidebarVm;

            // Return the same view but in edit mode
            return View("Index", vm);
        }
        #endregion


        private string GetShipmentStatusClass(int? statusId)
        {
            return statusId switch
            {
                1 => "warning",  // Pending
                2 => "info",     // Packed
                3 => "primary",  // Shipped
                4 => "primary",  // In Transit
                5 => "success",  // Delivered
                6 => "danger",   // Cancelled
                _ => "secondary"
            };
        }


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
                    PriceQuotationID = original.SalesOrders.PriceQuotationID,
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
                var salesOrder = await _salesOrderVersionRepository.All().FirstOrDefaultAsync(e => e.SalesOrdersVersionID == id);
                if (salesOrder == null)
                {
                    return Json(new { success = false, message = "Quotation not found" });
                }

                

                var prevFinal = await _salesOrderVersionRepository.AllActive().Where(e => e.SalesOrdersID == salesOrder.SalesOrdersID && e.IsFinal == true).FirstOrDefaultAsync();

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



        //[HttpPost]
        //public async Task<IActionResult> Duplicate(int id, BaseViewModel vm)
        //{
        //    await _salesOrderRepository.BeginTransactionAsync();

        //    try
        //    {



        //        var original = _salesOrderVersionRepository.AllActive()
        //            .Include(e => e.SalesOrderVersionItems)
        //            .Include(e => e.SalesOrders)
        //            .FirstOrDefault(e => e.SalesOrdersVersionID == id);

        //        if (original == null)
        //        {
        //            return Json(new { success = false, message = "Sales Order not found" });
        //        }


        //        var duplicate = new SalesOrders
        //        {
        //            //CustomerID = original.CustomerID,
        //            PriceQuotationID = original.PriceQuotationID,
        //            //SalesOrderDate = DateTime.Now,
        //            SalesOrderNumber = await _salesOrderService.GetNextSOcode(),
        //            //VatPercentage = original.VatPercentage,
        //            //Note = original.Note + " (Copy)",
        //            CreatedAt = DateTime.Now,
        //            CreatedBy = vm.CreatedBy
        //        };

        //        await _salesOrderRepository.AddAsync(duplicate, vm);
        //        await _userInfoService.ActionLogAsync("SalesOrderDup", ActionName.DataAdd, null, duplicate, duplicate.SalesOrderID, vm);

        //        // Copy items
        //        foreach (var item in original.SalesOrderVersionItems)
        //        {
        //            var duplicateItem = new SalesOrderVersionItems
        //            {
        //                SalesOrderVersionItemID = duplicate.SalesOrdersID,
        //                Description = item.Description,
        //                UnitTypeID = item.UnitTypeID,
        //                Area = item.Area,
        //                Rate = item.Rate,
        //                Quantity = item.Quantity,
        //                LIP = item.LIP,
        //                LMAC = item.LMAC,
        //                CreatedAt = DateTime.Now,
        //                CreatedBy = vm.CreatedBy
        //            };
        //            await _salesOrderItemRepository.AddAsync(duplicateItem, vm);
        //            await _userInfoService.ActionLogAsync("SalesOrderDup", ActionName.DataAdd, null, duplicateItem, duplicateItem.SalesOrderItemID, vm);
        //        }

        //        await _salesOrderItemRepository.CommitTransactionAsync();

        //        return Json(new { success = true, newSalesOrderId = duplicate.SalesOrderID });
        //    }
        //    catch (Exception ex)
        //    {
        //        await _salesOrderItemRepository.RollbackTransactionAsync();
        //        return Json(new { success = false, message = ex.Message });
        //    }
        //}

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
            // Get current logged-in user ID
            return 1; // Replace with actual implementation
        }
    }
}
