using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Inventory;
using GCTL.Core.ViewModels.POS.Purchase.PurchaseReceive;
using GCTL.Core.ViewModels.POS.Requsition.AddRequisition;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.MasterSetup.Statuse;
using GCTL.Service.POS.Inventory;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.POS.Purchase.PurchaseReceive
{
    public class PurchaseReceiveService : IPurchaseReceiveService
    {
        #region CTOR
        private readonly IGenericRepository<PurchaseReceives> _receiveRepository;
        private readonly IGenericRepository<PurchaseReceiveItems> _receiveItemRepository;
        private readonly IGenericRepository<PurchaseReceiveItemHistory> _receiveHistoryRepository;
        private readonly IGenericRepository<PurchasOrders> _purchaseOrderRepository;
        private readonly IGenericRepository<PurchasOrderVersions> _poVersionRepository;
        private readonly IGenericRepository<PurchasOrderItemVersions> _poItemVersionRepository;
        private readonly IUserInfoService _userInfoService;
        private readonly IStatusService _statusService;
        private readonly IGenericRepository<Locations> _locationRepository;
        private readonly IInventoryService _inventoryService;


        public PurchaseReceiveService(
            IGenericRepository<PurchaseReceives> receiveRepository,
            IGenericRepository<PurchaseReceiveItems> receiveItemRepository,
            IGenericRepository<PurchaseReceiveItemHistory> receiveHistoryRepository,
            IGenericRepository<PurchasOrders> purchaseOrderRepository,
            IGenericRepository<PurchasOrderVersions> poVersionRepository,
            IGenericRepository<PurchasOrderItemVersions> poItemVersionRepository,
            IUserInfoService userInfoService,
            IStatusService statusService,
            IGenericRepository<Locations> locationRepository,
            IInventoryService inventoryService)
        {
            _receiveRepository = receiveRepository;
            _receiveItemRepository = receiveItemRepository;
            _receiveHistoryRepository = receiveHistoryRepository;
            _purchaseOrderRepository = purchaseOrderRepository;
            _poVersionRepository = poVersionRepository;
            _poItemVersionRepository = poItemVersionRepository;
            _userInfoService = userInfoService;
            _statusService = statusService;
            _locationRepository = locationRepository;
            _inventoryService = inventoryService;
        }
        #endregion

        #region Get ALL / Get Open / Get Details
        public async Task<PaginatedResultCommon<PurchaseReceiveListViewModel>> GetPurchaseReceivesAsync(
            int? orgId, int page, int pageSize, string search, string sortColumn,
            string sortDirection, int? statusId, int? supplierId, string? fromDate, string? toDate)
        {
            var query = _receiveRepository.AllActive()
                .Include(r => r.PurchasOrderVersion)
                    .ThenInclude(pov => pov.Supplier)
                .Include(r => r.PurchasOrderVersion)
                    .ThenInclude(pov => pov.PurchasOrder)
                .Include(r => r.PurchaseReceiveItems)
                .Include(r => r.Status)
               // .Where(r => r.PurchasOrderVersion.OrganizationID == orgId)
                .AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r =>
                    r.PRNumber.Contains(search) ||
                    r.PurchasOrderVersion.PurchasOrder.POID.Contains(search) ||
                    r.VendorBill_Chalan.Contains(search));
            }

            // Status filter
            if (statusId.HasValue)
            {
                query = query.Where(r => r.StatusID == statusId.Value);
            }

            // Supplier filter
            if (supplierId.HasValue)
            {
                query = query.Where(r => r.PurchasOrderVersion.SupplierID == supplierId.Value);
            }

            // Date filter
            if (!string.IsNullOrWhiteSpace(fromDate) || !string.IsNullOrWhiteSpace(toDate))
            {
                var dateFormats = new[] { "dd/MM/yy", "dd/MM/yyyy", "yyyy-MM-dd" };

                if (DateTime.TryParseExact(fromDate, dateFormats, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var parsedFrom))
                {
                    query = query.Where(r => r.PRDate >= parsedFrom);
                }

                if (DateTime.TryParseExact(toDate, dateFormats, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var parsedTo))
                {
                    var toDateEnd = parsedTo.AddDays(1).AddTicks(-1);
                    query = query.Where(r => r.PRDate <= toDateEnd);
                }
            }

            // Sorting
            query = sortColumn switch
            {
                "PurchaseReceiveID" => sortDirection == "desc"
                    ? query.OrderByDescending(r => r.PurchaseReceiveID)
                    : query.OrderBy(r => r.PurchaseReceiveID),
                "PRDate" => sortDirection == "desc"
                    ? query.OrderByDescending(r => r.PRDate)
                    : query.OrderBy(r => r.PRDate),
                _ => query.OrderByDescending(r => r.PurchaseReceiveID)
            };

            var totalRecords = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new PurchaseReceiveListViewModel
                {
                    PurchaseReceiveID = r.PurchaseReceiveID,
                    PRNumber = r.PRNumber,
                    PRDate = r.PRDate,
                    PONumber = r.PurchasOrderVersion.PurchasOrder.POID,
                    SupplierName = r.PurchasOrderVersion.Supplier.FullName,
                    TotalItems = r.PurchaseReceiveItems.Count,
                    TotalReceivedQty = r.TotalReceivedQty ?? 0,
                    Status = r.Status.StatusName,
                    VendorBillChalan = r.VendorBill_Chalan,
                    CanEdit = r.StatusID == 1, // Assuming 1 = Draft
                    CanDelete = r.StatusID == 1
                })
                .ToListAsync();

            return new PaginatedResultCommon<PurchaseReceiveListViewModel>(items, totalRecords);
        }

        public async Task<List<OpenPurchaseOrderViewModel>> GetOpenPurchaseOrdersAsync(int? orgId)
        {
            var openPOs = await _purchaseOrderRepository.AllActive()
                .Include(po => po.PurchasOrderVersions.Where(v => v.IsFinal == true))
                    .ThenInclude(v => v.Supplier)
                .Include(po => po.PurchasOrderVersions)
                    .ThenInclude(v => v.PurchasOrderItemVersions)
                .Include(po => po.POStatus)
                .Where(po => po.POStatus.StatusName == "Open") // Only open POs
               
                .Where(po => po.PurchasOrderVersions.Any(v =>
                    v.IsFinal == true &&
                    v.OrganizationID == orgId &&
                    (v.IsFullyReceived == false || v.IsFullyReceived == null)))
                .Select(po => new
                {
                    po.PurchasOrderID,
                    FinalVersion = po.PurchasOrderVersions.FirstOrDefault(v => v.IsFinal == true)
                })
                .ToListAsync();

            return openPOs.Select(po => new OpenPurchaseOrderViewModel
            {
                PurchaseOrderID = po.PurchasOrderID,
                PurchaseOrderVersionID = po.FinalVersion.PurchasOrderVersionID,
                PONumber = _purchaseOrderRepository.AllActive()
                    .Where(p => p.PurchasOrderID == po.PurchasOrderID)
                    .Select(p => p.POID)
                    .FirstOrDefault(),
                SupplierName = po.FinalVersion?.Supplier?.FullName ?? "N/A",
                PurchaseDate = po.FinalVersion?.PurchaseDate,
                TotalItems = po.FinalVersion?.PurchasOrderItemVersions?.Count ?? 0
            }).ToList();
        }

        public async Task<PODetailsForReceiveViewModel> GetPODetailsForReceiveAsync(int poVersionId)
        {
            var poVersion = await _poVersionRepository.AllActive()
                .Include(v => v.PurchasOrder)
                .Include(v => v.Supplier)
                .Include(v => v.PurchasOrderItemVersions)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.ProductType)
                .Include(v => v.PurchasOrderItemVersions)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.ProductBrand)
                .Include(v => v.PurchasOrderItemVersions)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.UnitType)
                .FirstOrDefaultAsync(v => v.PurchasOrderVersionID == poVersionId);

            if (poVersion == null)
                return null;

            return new PODetailsForReceiveViewModel
            {
                PurchaseOrderID = poVersion.PurchasOrderID ?? 0,
                PurchaseOrderVersionID = poVersion.PurchasOrderVersionID,
                PONumber = poVersion.PurchasOrder?.POID ?? "",
                SupplierName = poVersion.Supplier?.FullName ?? "N/A",
                PurchaseDate = poVersion.PurchaseDate,
                DueDate = poVersion.DueDate,
                Note = poVersion.Note,
                Items = poVersion.PurchasOrderItemVersions
                    .Where(i => i.Quantity > (i.ReceivedQuantity) || i.RemainingQuantity == null)
                    .Select(i => new POItemForReceiveViewModel
                    {
                        POItemVersionID = i.PurchasOrderVersionItemID,
                        ProductID = i.ProductID ?? 0,
                        ProductName = i.Product?.ProductName ?? "N/A",
                        ProductType = i.Product?.ProductType?.ProductTypeName ?? "N/A",
                        Brand = i.Product?.ProductBrand?.ProductBrandName ?? "N/A",
                        Unit = i.Product?.UnitType?.UnitTypeName ?? "N/A",
                        OrderedQuantity = i.Quantity ?? 0,
                        ReceivedQuantity = i.ReceivedQuantity ?? 0,
                        RemainingQuantity = i.RemainingQuantity ?? i.Quantity ?? 0,
                        UnitPrice = i.UnitPrice ?? 0,
                        IsFullyReceived = i.IsFullyReceived == null  ? false : i.IsFullyReceived,
                    })
                    .ToList()
            };
        }

        public async Task<ReceiveDetailsViewModel> GetReceiveDetailsAsync(int receiveId)
        {
            var receive = await _receiveRepository.AllActive()
                .Include(r => r.PurchasOrderVersion)
                    .ThenInclude(v => v.PurchasOrder)
                .Include(r => r.PurchasOrderVersion)
                    .ThenInclude(v => v.Supplier)
                .Include(r => r.PurchaseReceiveItems)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.ProductType)
                .Include(r => r.PurchaseReceiveItems)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.ProductBrand)
                .Include(r => r.PurchaseReceiveItems)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.UnitType)
                .Include(r => r.ReceivedByEmployee)
                .Include(r => r.Status)
                .FirstOrDefaultAsync(r => r.PurchaseReceiveID == receiveId);

            if (receive == null)
                return null;

            return new ReceiveDetailsViewModel
            {
                PurchaseReceiveID = receive.PurchaseReceiveID,
                PRNumber = receive.PRNumber,
                PRDate = receive.PRDate,
                PONumber = receive.PurchasOrderVersion?.PurchasOrder?.POID ?? "",
                SupplierName = receive.PurchasOrderVersion?.Supplier?.FullName ?? "N/A",
                VendorBillChalan = receive.VendorBill_Chalan,
                BillDate = receive.BillDate,
                PRNote = receive.PRNote,
                ReceivedByName = receive.ReceivedByEmployee != null
                    ? $"{receive.ReceivedByEmployee.FirstName} {receive.ReceivedByEmployee.LastName}"
                    : "N/A",
                Status = receive.Status?.StatusName ?? "N/A",
                TotalReceivedQty = receive.TotalReceivedQty ?? 0,
                TotalAcceptedQty = receive.TotalAcceptedQty ?? 0,
                TotalRejectedQty = receive.TotalRejectedQty ?? 0,
                IsPartialReceive = receive.IsPartialReceive ?? false,
                Items = receive.PurchaseReceiveItems.Select(i => new ReceiveItemDetailViewModel
                {
                    ItemID = i.PurchaseReceiveItemID,
                    ProductName = i.Product?.ProductName ?? "N/A",
                    ProductType = i.Product?.ProductType?.ProductTypeName ?? "N/A",
                    Brand = i.Product?.ProductBrand?.ProductBrandName ?? "N/A",
                    Unit = i.Product?.UnitType?.UnitTypeName ?? "N/A",
                    POQuantity = i.POQuantity ?? 0,
                    ReceiveQuantity = i.ReceiveQuantity ?? 0,
                    AcceptedQuantity = i.AcceptedQuantity ?? 0,
                    RejectedQuantity = i.RejectedQuantity ?? 0,
                    RejectionReason = i.RejectionReason,
                    Note = i.Note
                }).ToList()
            };
        }
        #endregion

        public async Task<CommonReturnViewModel> CreatePurchaseReceiveAsync(
            CreatePurchaseReceiveViewModel model, int? empId, BaseViewModel? baseView)
        {
            var response = new CommonReturnViewModel();
            await _receiveRepository.BeginTransactionAsync();

            try
            {
                // Validate PO exists and is open
                var poVersion = await _poVersionRepository.AllActive()
                    .Include(v => v.PurchasOrder)
                        .ThenInclude(po => po.POStatus)
                    .Include(v => v.PurchasOrderItemVersions)
                    .Include(v => v.OBShipingAddress)
                    .FirstOrDefaultAsync(v => v.PurchasOrderVersionID == model.PurchaseOrderVersionID);

                if (poVersion == null)
                {
                    await _receiveRepository.RollbackTransactionAsync();
                    response.Success = false;
                    response.Message = "Purchase Order not found.";
                    return response;
                }

                if (poVersion.PurchasOrder?.POStatus?.StatusName != "Open")
                {
                    await _receiveRepository.RollbackTransactionAsync();
                    response.Success = false;
                    response.Message = "Cannot receive against a closed or cancelled PO.";
                    return response;
                }

                var locationId = poVersion.OBShipingAddress.LocationID;//await _purchaseOrderAddressRepository.AllActive().FirstOrDefaultAsync(e => e.PurOrderBaseSAddressID == vm.SelectedShippingAddressId);

                // Validate items
                foreach (var item in model.Items)
                {
                    if (item.AcceptedQuantity + item.RejectedQuantity != item.ReceiveQuantity)
                    {
                        await _receiveRepository.RollbackTransactionAsync();
                        response.Success = false;
                        response.Message = "Accepted + Rejected must equal Receive quantity for all items.";
                        return response;
                    }

                    var poItem = poVersion.PurchasOrderItemVersions
                        .FirstOrDefault(i => i.PurchasOrderVersionItemID == item.POItemVersionID);

                    if (poItem == null) continue;

                    var remaining = (poItem.RemainingQuantity ?? poItem.Quantity ?? 0);
                    if (item.ReceiveQuantity > remaining)
                    {
                        // Over-receive warning - allow but flag
                        model.Items.First(i => i.POItemVersionID == item.POItemVersionID)
                            .Note = $"Over-receive: Expected {remaining}, Received {item.ReceiveQuantity}";
                    }
                }

                // Calculate totals
                decimal totalReceived = model.Items.Sum(i => i.ReceiveQuantity);
                decimal totalAccepted = model.Items.Sum(i => i.AcceptedQuantity);
                decimal totalRejected = model.Items.Sum(i => i.RejectedQuantity);

                // Create Purchase Receive
                var receive = new PurchaseReceives
                {
                    PurchasOrderVersionID = model.PurchaseOrderVersionID, // Note: FK name issue
                    PRNumber = model.PRNumber,
                    PRDate = model.PRDate,
                    VendorBill_Chalan = model.VendorBillChalan,
                    BillDate = model.BillDate,
                    PRNote = model.PRNote,
                    StatusID = model.StatusID,
                    ReceivedByEmployeeID = model.ReceivedByEmployeeID ?? empId,
                    IsPartialReceive = model.Items.Any(i =>
                    {
                        var poItem = poVersion.PurchasOrderItemVersions
                            .FirstOrDefault(pi => pi.PurchasOrderVersionItemID == i.POItemVersionID);
                        return poItem != null && i.ReceiveQuantity < (poItem.RemainingQuantity ?? poItem.Quantity ?? 0);
                    }),
                    TotalReceivedQty = totalReceived,
                    TotalAcceptedQty = totalAccepted,
                    TotalRejectedQty = totalRejected,
                    AttachmentPath = model.AttachmentPath,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = baseView?.CreatedBy,
                    LIP = baseView?.LIP,
                    LMAC = baseView?.LMAC,
                    LocationID = locationId
                    
                };

                await _receiveRepository.AddAsync(receive);
                await _userInfoService.ActionLogAsync("purchase receive", ActionName.DataAdd,
                    null, receive, receive.PurchaseReceiveID, baseView);

                // Create Receive Items
                foreach (var item in model.Items)
                {
                    var poItem = poVersion.PurchasOrderItemVersions
                        .FirstOrDefault(i => i.PurchasOrderVersionItemID == item.POItemVersionID);

                    if (poItem == null) continue;

                    var receiveItem = new PurchaseReceiveItems
                    {
                        PurchaseReceiveID = receive.PurchaseReceiveID,
                        ProductID = item.ProductID,
                        PurchasOrderVersionItemID = item.POItemVersionID,
                        POQuantity = item.POQuantity,
                        ReceiveQuantity = item.ReceiveQuantity,
                        AcceptedQuantity = item.AcceptedQuantity,
                        RejectedQuantity = item.RejectedQuantity,
                        RejectionReason = item.RejectionReason,
                        Note = item.Note,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = baseView?.CreatedBy,
                        LIP = baseView?.LIP,
                        LMAC = baseView?.LMAC
                    };

                    await _receiveItemRepository.AddAsync(receiveItem);

                    // Update PO Item quantities
                    poItem.ReceivedQuantity = (poItem.ReceivedQuantity ?? 0) + item.ReceiveQuantity;
                    poItem.AcceptedQuantity = (poItem.AcceptedQuantity ?? 0) + item.AcceptedQuantity;
                    poItem.RejectedQuantity = (poItem.RejectedQuantity ?? 0) + item.RejectedQuantity;
                    poItem.RemainingQuantity = (poItem.Quantity ?? 0) - (poItem.ReceivedQuantity ?? 0);
                    poItem.IsFullyReceived = poItem.RemainingQuantity <= 0;
                    poItem.UpdatedAt = DateTime.UtcNow;
                    poItem.UpdatedBy = baseView?.UpdatedBy;

                    await _poItemVersionRepository.UpdateAsync(poItem);

                    // TODO: INVENTORY - Add received items to inventory
                    // Get default receiving location
                    var defaultLocation = await _locationRepository.AllActive()
                        .FirstOrDefaultAsync(l => l.LocationID == locationId);
                        //.FirstOrDefaultAsync(l => l.IsDefaultLocation == true && l.OrganizationID == poVersion.OrganizationID);
                        //.FirstOrDefaultAsync(l => l.OrganizationBranchID == poVersion.OrganizationBranchID && l.OrganizationID == poVersion.OrganizationID);

                    if (defaultLocation != null)
                    {
                        await _inventoryService.ReceiveStockAsync(new ReceiveStockViewModel
                        {
                            ProductID = item.ProductID,
                            LocationID = defaultLocation.LocationID,
                            Quantity = item.AcceptedQuantity,
                            UnitCost = poItem.UnitPrice ?? 0,
                            ReferenceType = "PurchaseReceive",
                            ReferenceID = receive.PurchaseReceiveID,
                            TransactionDate = receive.PRDate ?? DateTime.UtcNow,
                            Note = $"Received from PR: {receive.PRNumber}, PO: {poVersion.PurchasOrder?.POID}",
                            CreatedBy = baseView?.CreatedBy,
                            LIP = baseView?.LIP,
                            LMAC = baseView?.LMAC
                        });
                    }
                    else
                    {
                        await _receiveRepository.RollbackTransactionAsync();
                        response.Success = false;
                        response.Message = "Location is not found , please insert Location Master Setup first";
                        return response;
                    }

                    await _userInfoService.ActionLogAsync("purchase receive item", ActionName.DataAdd,
                        null, receiveItem, receiveItem.PurchaseReceiveItemID, baseView);
                }

                // Update PO Version summary
                poVersion.TotalReceiveCount = (poVersion.TotalReceiveCount) + 1;
                poVersion.LastReceiveDate = DateTime.UtcNow;
                if (poVersion.FirstReceiveDate == null)
                    poVersion.FirstReceiveDate = DateTime.UtcNow;

                var allItemsReceived = poVersion.PurchasOrderItemVersions.All(i => i.IsFullyReceived == true);
                poVersion.IsFullyReceived = allItemsReceived;
                poVersion.IsPartiallyReceived = !allItemsReceived && poVersion.TotalReceiveCount > 0;

                if (poVersion.IsFullyReceived == true)
                {
                    var CloseStatus = await _statusService.GetStatusIDAsync("Close");


                    // Auto-close PO if fully received
                    var po = poVersion.PurchasOrder;
                    // You might want to change status to "Closed" here
                     po.POStatusID = CloseStatus;
                    await _purchaseOrderRepository.UpdateAsync(po);
                }

                await _poVersionRepository.UpdateAsync(poVersion);

                await _receiveRepository.CommitTransactionAsync();

                response.Success = true;
                response.Message = "Purchase receive created successfully.";
                response.Data = receive.PurchaseReceiveID;
                return response;
            }
            catch (Exception ex)
            {
                await _receiveRepository.RollbackTransactionAsync();
                response.Success = false;
                response.Message = "Error creating purchase receive: " + ex.Message;
                return response;
            }
        }

        public async Task<CommonReturnViewModel> UpdatePurchaseReceiveAsync(
            EditPurchaseReceiveViewModel model, int? empId, BaseViewModel? baseView)
        {
            var response = new CommonReturnViewModel();
            await _receiveRepository.BeginTransactionAsync();

            try
            {
                var receive = await _receiveRepository.AllActive()
                    .Include(r => r.PurchaseReceiveItems)
                    .FirstOrDefaultAsync(r => r.PurchaseReceiveID == model.PurchaseReceiveID);

                if (receive == null)
                {
                    await _receiveRepository.RollbackTransactionAsync();
                    response.Success = false;
                    response.Message = "Purchase receive not found.";
                    return response;
                }

                // Check if editable
                if (receive.StatusID != 1) // Assuming 1 = Draft
                {
                    await _receiveRepository.RollbackTransactionAsync();
                    response.Success = false;
                    response.Message = "Cannot edit completed purchase receive.";
                    return response;
                }

                // Reverse old quantities from PO items
                foreach (var oldItem in receive.PurchaseReceiveItems)
                {
                    var poItem = await _poItemVersionRepository.GetByIdAsync(oldItem.PurchasOrderVersionItemID ?? 0);
                    if (poItem != null)
                    {
                        poItem.ReceivedQuantity = (poItem.ReceivedQuantity ?? 0) - (oldItem.ReceiveQuantity ?? 0);
                        poItem.AcceptedQuantity = (poItem.AcceptedQuantity ?? 0) - (oldItem.AcceptedQuantity ?? 0);
                        poItem.RejectedQuantity = (poItem.RejectedQuantity ?? 0) - (oldItem.RejectedQuantity ?? 0);
                        poItem.RemainingQuantity = (poItem.Quantity ?? 0) - (poItem.ReceivedQuantity ?? 0);
                        poItem.IsFullyReceived = poItem.RemainingQuantity <= 0;

                        await _poItemVersionRepository.UpdateAsync(poItem);

                        // TODO: INVENTORY - Reverse old stock entries if needed

                        var defaultLocation = await _locationRepository.AllActive()
                            .FirstOrDefaultAsync(l => l.LocationID == receive.LocationID);

                        if (defaultLocation != null)
                        {
                            await _inventoryService.ReverseStockAsync(new ReverseStockViewModel
                            {
                                ProductID = oldItem.ProductID ?? 0,
                                LocationID = defaultLocation.LocationID,
                                Quantity = oldItem.AcceptedQuantity ?? 0,
                                ReferenceType = "PurchaseReceive",
                                ReferenceID = receive.PurchaseReceiveID,
                                TransactionDate = DateTime.UtcNow,
                                Note = $"Reversed for edit - PR: {receive.PRNumber}",
                                CreatedBy = baseView?.UpdatedBy,
                                LIP = baseView?.LIP,
                                LMAC = baseView?.LMAC
                            });
                        }
                        else
                        {
                            await _receiveRepository.RollbackTransactionAsync();
                            response.Success = false;
                            response.Message = "Location is not found , please insert Location Master Setup first";
                            return response;
                        }
                    }
                }

                // Delete old items
                await _receiveItemRepository.DeleteRangeAsync(receive.PurchaseReceiveItems.ToList());

                // Update receive header
                decimal totalReceived = model.Items.Sum(i => i.ReceiveQuantity);
                decimal totalAccepted = model.Items.Sum(i => i.AcceptedQuantity);
                decimal totalRejected = model.Items.Sum(i => i.RejectedQuantity);

                receive.VendorBill_Chalan = model.VendorBillChalan;
                receive.BillDate = model.BillDate;
                receive.PRNote = model.PRNote;
                receive.TotalReceivedQty = totalReceived;
                receive.TotalAcceptedQty = totalAccepted;
                receive.TotalRejectedQty = totalRejected;
                receive.UpdatedAt = DateTime.UtcNow;
                receive.UpdatedBy = baseView?.UpdatedBy;

                await _receiveRepository.UpdateAsync(receive);

                // Add new items and update PO quantities
                foreach (var item in model.Items)
                {
                    var receiveItem = new PurchaseReceiveItems
                    {
                        PurchaseReceiveID = receive.PurchaseReceiveID,
                        ProductID = item.ProductID,
                        PurchasOrderVersionItemID = item.POItemVersionID,
                        POQuantity = item.POQuantity,
                        ReceiveQuantity = item.ReceiveQuantity,
                        AcceptedQuantity = item.AcceptedQuantity,
                        RejectedQuantity = item.RejectedQuantity,
                        RejectionReason = item.RejectionReason,
                        Note = item.Note,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = baseView?.CreatedBy,
                        LIP = baseView?.LIP,
                        LMAC = baseView?.LMAC
                    };

                    await _receiveItemRepository.AddAsync(receiveItem);

                    var poItem = await _poItemVersionRepository.GetByIdAsync(item.POItemVersionID);
                    if (poItem != null)
                    {
                        poItem.ReceivedQuantity = (poItem.ReceivedQuantity ?? 0) + item.ReceiveQuantity;
                        poItem.AcceptedQuantity = (poItem.AcceptedQuantity ?? 0) + item.AcceptedQuantity;
                        poItem.RejectedQuantity = (poItem.RejectedQuantity ?? 0) + item.RejectedQuantity;
                        poItem.RemainingQuantity = (poItem.Quantity ?? 0) - (poItem.ReceivedQuantity ?? 0);
                        poItem.IsFullyReceived = poItem.RemainingQuantity <= 0;

                        await _poItemVersionRepository.UpdateAsync(poItem);

                        // TODO: INVENTORY - Update stock with new quantities

                        var defaultLocation = await _locationRepository.AllActive().FirstOrDefaultAsync(l => l.LocationID == receive.LocationID);

                        if (defaultLocation != null)
                        {
                            var res = await _inventoryService.ReceiveStockAsync(new ReceiveStockViewModel
                            {
                                ProductID = item.ProductID,
                                LocationID = defaultLocation.LocationID,
                                Quantity = item.AcceptedQuantity,
                                UnitCost = poItem.UnitPrice ?? 0,
                                ReferenceType = "PurchaseReceive",
                                ReferenceID = receive.PurchaseReceiveID,
                                TransactionDate = DateTime.UtcNow,
                                Note = $"Updated receive - PR: {receive.PRNumber}",
                                CreatedBy = baseView?.CreatedBy,
                                LIP = baseView?.LIP,
                                LMAC = baseView?.LMAC
                            });

                            if (!res)
                            {
                                await _receiveRepository.RollbackTransactionAsync();
                                response.Success = false;
                                response.Message = "Error updating purchase receive: ";
                                return response;
                            }
                        }
                        else
                        {
                            await _receiveRepository.RollbackTransactionAsync();
                            response.Success = false;
                            response.Message = "Location is not found , please insert Location Master Setup first";
                            return response;
                        }
                    }
                }

                await _receiveRepository.CommitTransactionAsync();

                response.Success = true;
                response.Message = "Purchase receive updated successfully.";
                return response;
            }
            catch (Exception ex)
            {
                await _receiveRepository.RollbackTransactionAsync();
                response.Success = false;
                response.Message = "Error updating purchase receive: " + ex.Message;
                return response;
            }
        }

        public async Task<CommonReturnViewModel> DeletePurchaseReceiveAsync(
            int id, int? empId, BaseViewModel? baseView)
        {
            var response = new CommonReturnViewModel();
            await _receiveRepository.BeginTransactionAsync();

            try
            {
                var receive = await _receiveRepository.AllActive()
                    .Include(r => r.PurchaseReceiveItems)
                    .FirstOrDefaultAsync(r => r.PurchaseReceiveID == id);

                if (receive == null)
                {
                    await _receiveRepository.RollbackTransactionAsync();
                    response.Success = false;
                    response.Message = "Purchase receive not found.";
                    return response;
                }

                // Check if deletable
                if (receive.StatusID != 1)
                {
                    await _receiveRepository.RollbackTransactionAsync();
                    response.Success = false;
                    response.Message = "Cannot delete completed purchase receive.";
                    return response;
                }

                // Reverse quantities from PO items
                foreach (var item in receive.PurchaseReceiveItems)
                {
                    var poItem = await _poItemVersionRepository.GetByIdAsync(item.PurchasOrderVersionItemID ?? 0);
                    if (poItem != null)
                    {
                        poItem.ReceivedQuantity = (poItem.ReceivedQuantity ?? 0) - (item.ReceiveQuantity ?? 0);
                        poItem.AcceptedQuantity = (poItem.AcceptedQuantity ?? 0) - (item.AcceptedQuantity ?? 0);
                        poItem.RejectedQuantity = (poItem.RejectedQuantity ?? 0) - (item.RejectedQuantity ?? 0);
                        poItem.RemainingQuantity = (poItem.Quantity ?? 0) - (poItem.ReceivedQuantity ?? 0);
                        poItem.IsFullyReceived = poItem.RemainingQuantity <= 0;

                        await _poItemVersionRepository.UpdateAsync(poItem);

                        // TODO: INVENTORY - Reverse stock entries

                        var defaultLocation = await _locationRepository.AllActive().FirstOrDefaultAsync(l => l.IsDefaultLocation == true);

                        if (defaultLocation != null)
                        {
                            await _inventoryService.ReverseStockAsync(new ReverseStockViewModel
                            {
                                ProductID = item.ProductID ?? 0,
                                LocationID = defaultLocation.LocationID,
                                Quantity = item.AcceptedQuantity ?? 0,
                                ReferenceType = "PurchaseReceive",
                                ReferenceID = receive.PurchaseReceiveID,
                                TransactionDate = DateTime.UtcNow,
                                Note = $"Deleted receive - PR: {receive.PRNumber}",
                                CreatedBy = baseView?.DeletedBy,
                                LIP = baseView?.LIP,
                                LMAC = baseView?.LMAC
                            });
                        }
                    }
                }

                // Soft delete
                receive.DeletedAt = DateTime.UtcNow;
                receive.DeletedBy = baseView?.DeletedBy;
                await _receiveRepository.UpdateAsync(receive);

                foreach (var item in receive.PurchaseReceiveItems)
                {
                    item.DeletedAt = DateTime.UtcNow;
                    item.DeletedBy = baseView?.DeletedBy;
                    await _receiveItemRepository.UpdateAsync(item);
                }

                await _receiveRepository.CommitTransactionAsync();

                response.Success = true;
                response.Message = "Purchase receive deleted successfully.";
                return response;
            }
            catch (Exception ex)
            {
                await _receiveRepository.RollbackTransactionAsync();
                response.Success = false;
                response.Message = "Error deleting purchase receive: " + ex.Message;
                return response;
            }
        }

        public async Task<string> GetNextPRNumberAsync()
        {
            var lastCode = await _receiveRepository.AllActive()
                .OrderByDescending(r => r.PurchaseReceiveID)
                .Select(r => r.PRNumber)
                .FirstOrDefaultAsync();

            string yearPart = DateTime.Now.ToString("yy");
            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastCode))
            {
                var parts = lastCode.Split('-');
                if (parts.Length == 3 && parts[1] == yearPart)
                {
                    if (int.TryParse(parts[2], out int lastNumber))
                    {
                        nextNumber = lastNumber + 1;
                    }
                }
            }

            return $"PR-{yearPart}-{nextNumber.ToString("D6")}";
        }

        public Task<byte[]> GeneratePDF(int orgId, string? fromDate, string? toDate)
        {
            // TODO: Implement PDF generation
            throw new NotImplementedException("PDF generation not yet implemented");
        }

        public Task<byte[]> GenerateExcel(int orgId, string? fromDate, string? toDate)
        {
            // TODO: Implement Excel generation
            throw new NotImplementedException("Excel generation not yet implemented");
        }
    }


}
