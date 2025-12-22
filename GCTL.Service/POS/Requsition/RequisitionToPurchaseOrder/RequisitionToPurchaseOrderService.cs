using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Enums;
using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Purchase.PurchaseOrder;
using GCTL.Core.ViewModels.POS.Requsition.AddRequisition;
using GCTL.Core.ViewModels.POS.Requsition.RequisitionToPurchaseOrder;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.POS.Requsition.RequisitionToPurchaseOrder
{
    public class RequisitionToPurchaseOrderService : IRequisitionToPurchaseOrderService
    {
        private readonly IGenericRepository<Requisitions> _requisitionRepository;
        private readonly IGenericRepository<RequisitionItems> _requisitionItemRepository;
        private readonly IGenericRepository<PurchasOrders> _purchaseOrderRepository;
        private readonly IGenericRepository<PurchaseOrderItem> _purchaseOrderItemRepository;
        private readonly IGenericRepository<Products> _productRepository;
        private readonly IUserInfoService _userInfoService;

        public RequisitionToPurchaseOrderService(
            IGenericRepository<Requisitions> requisitionRepository,
            IGenericRepository<RequisitionItems> requisitionItemRepository,
            IGenericRepository<PurchasOrders> purchaseOrderRepository,
            IGenericRepository<PurchaseOrderItem> purchaseOrderItemRepository,
            IGenericRepository<Products> productRepository,
            IUserInfoService userInfoService)
        {
            _requisitionRepository = requisitionRepository;
            _requisitionItemRepository = requisitionItemRepository;
            _purchaseOrderRepository = purchaseOrderRepository;
            _purchaseOrderItemRepository = purchaseOrderItemRepository;
            _productRepository = productRepository;
            _userInfoService = userInfoService;
        }

        public async Task<PaginatedResultCommon<ApprovedRequisitionItemViewModel>> GetApprovedRequisitionsAsync(
            int? orgId, int page, int pageSize, string search, string sortColumn,
            string sortDirection, int? productTypeId, string? fromDate, string? toDate)
        {
            var query = _requisitionRepository.AllActive()
                .Include(r => r.RequisitionByNavigation)
                .Include(r => r.RequisitionItems)
                    .ThenInclude(ri => ri.Product)
                        .ThenInclude(p => p.ProductType)
                .Where(r => r.IsFinalApproved == true)
                .Where(r => r.OrganizationID == orgId)
                .AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r =>
                    r.RequisitionCode.Contains(search) ||
                    r.RequisitionByNavigation.FirstName.Contains(search) ||
                    r.RequisitionByNavigation.LastName.Contains(search));
            }

            // Product type filter
            if (productTypeId.HasValue)
            {
                query = query.Where(r => r.RequisitionItems.Any(ri =>
                    ri.Product.ProductTypeID == productTypeId.Value));
            }

            // Date filter
            if (!string.IsNullOrWhiteSpace(fromDate) || !string.IsNullOrWhiteSpace(toDate))
            {
                var dateFormats = new[] { "dd/MM/yy", "dd/MM/yyyy", "yyyy-MM-dd" };

                if (DateTime.TryParseExact(fromDate, dateFormats, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var parsedFrom))
                {
                    query = query.Where(r => r.CreatedAt >= parsedFrom);
                }

                if (DateTime.TryParseExact(toDate, dateFormats, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var parsedTo))
                {
                    var toDateEnd = parsedTo.AddDays(1).AddTicks(-1);
                    query = query.Where(r => r.CreatedAt <= toDateEnd);
                }
            }

            // Sorting
            query = sortColumn switch
            {
                "RequisitionId" => sortDirection == "desc"
                    ? query.OrderByDescending(r => r.RequisitionID)
                    : query.OrderBy(r => r.RequisitionID),
                "RequisitionDate" => sortDirection == "desc"
                    ? query.OrderByDescending(r => r.CreatedAt)
                    : query.OrderBy(r => r.CreatedAt),
                "RequisitionBy" => sortDirection == "desc"
                    ? query.OrderByDescending(r => r.RequisitionByNavigation.FirstName)
                    : query.OrderBy(r => r.RequisitionByNavigation.FirstName),
                _ => query.OrderByDescending(r => r.RequisitionID)
            };

            var totalRecords = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ApprovedRequisitionItemViewModel
                {
                    RequisitionId = r.RequisitionID,
                    RequisitionCode = r.RequisitionCode,
                    RequisitionDate = r.CreatedAt ?? DateTime.UtcNow,
                    RequisitionBy = r.RequisitionByNavigation.FirstName + " " + r.RequisitionByNavigation.LastName,
                    TotalItems = r.RequisitionItems.Count,
                    Priority = ((Priority)(r.Priority ?? (int)Priority.Normal)).ToString(),
                    ApprovedAt = r.UpdatedAt,
                   // HasPurchaseOrder = r.RequisitionItems.Any(ri => ri.PurchaseOrderItemID != null),
                   // PurchaseOrderId = r.RequisitionItems.Where(ri => ri.PurchaseOrderItemID != null).Select(ri => ri.PurchaseOrderItem.PurchaseOrderID).FirstOrDefault()
                })
                .ToListAsync();

            return new PaginatedResultCommon<ApprovedRequisitionItemViewModel>(items, totalRecords);
        }

        public async Task<RequisitionDetailsForPOViewModel> GetRequisitionDetailsForPOAsync(int requisitionId)
        {
            var requisition = await _requisitionRepository.AllActive()
                .Include(r => r.Organization)
                .Include(r => r.OrganizationBranch)
                .Include(r => r.RequisitionByNavigation)
                .Include(r => r.RequisitionItems)
                    .ThenInclude(ri => ri.Product)
                        .ThenInclude(p => p.ProductType)
                .Include(r => r.RequisitionItems)
                    .ThenInclude(ri => ri.Product)
                        .ThenInclude(p => p.UnitType)
                .Include(r => r.RequisitionItems)
                    .ThenInclude(ri => ri.Product)
                        .ThenInclude(p => p.ProductBrand)
                .Include(r => r.RequisitionItems)
                    .ThenInclude(ri => ri.Product)
                        .ThenInclude(p => p.ProductPricing)
                .Include(r => r.RequisitionItems)
                    //.ThenInclude(ri => ri.PurchaseOrderItem)
                    //    .ThenInclude(poi => poi.PurchaseOrder)
                .FirstOrDefaultAsync(r => r.RequisitionID == requisitionId);

            if (requisition == null)
                return null;

            //var hasPO = requisition.RequisitionItems.Any(ri => ri.PurchaseOrderItemID != null);
            //var poId = requisition.RequisitionItems
            //    .Where(ri => ri.PurchaseOrderItemID != null)
            //    .Select(ri => ri.PurchaseOrderItem?.PurchaseOrderID)
            //    .FirstOrDefault();

            //var poCode = requisition.RequisitionItems
            //    .Where(ri => ri.PurchaseOrderItemID != null)
            //    .Select(ri => ri.PurchaseOrderItem?.PurchaseOrder?.PurchaseOrderCode)
            //    .FirstOrDefault();

            return new RequisitionDetailsForPOViewModel
            {
                RequisitionId = requisition.RequisitionID,
                RequisitionCode = requisition.RequisitionCode,
                RequisitionDate = requisition.CreatedAt ?? DateTime.UtcNow,
                RequisitionBy = requisition.RequisitionByNavigation.FirstName + " " +
                               requisition.RequisitionByNavigation.LastName,
                Organization = requisition.Organization?.OrganizationName ?? "N/A",
                Branch = requisition.OrganizationBranch?.OrganizationBranchName ?? "N/A",
                Priority = ((Priority)(requisition.Priority ?? (int)Priority.Normal)).ToString(),
                RequisitionNote = requisition.RequisitionNote ?? "",
                //HasPurchaseOrder = hasPO,
                //PurchaseOrderId = poId,
                //PurchaseOrderCode = poCode,
                Items = requisition.RequisitionItems.Select(ri => new RequisitionItemForPOViewModel
                {
                    ItemId = ri.RequisitionItemID,
                    ProductId = ri.ProductID ?? 0,
                    ProductType = ri.Product?.ProductType?.ProductTypeName ?? "N/A",
                    ProductName = ri.Product?.ProductName ?? "N/A",
                    Unit = ri.Product?.UnitType?.UnitTypeName ?? "N/A",
                    Brand = ri.Product?.ProductBrand?.ProductBrandName ?? "N/A",
                    RequestedQuantity = ri.RequisitionQuantity ?? 0,
                    ApprovedQuantity = ri.ApprovedQuantity ?? 0,
                    UnitPrice = ri.Product?.ProductPricing?.FirstOrDefault()?.SellingPriceExclVAT ?? 0
                }).ToList()
            };
        }

        //public async Task<CommonReturnViewModel> ConvertToPurchaseOrderAsync(
        //    ConvertToPurchaseOrderViewModel model, int? empId, BaseViewModel? baseView)
        //{
        //    var response = new CommonReturnViewModel();
        //    await _purchaseOrderRepository.BeginTransactionAsync();

        //    try
        //    {
        //        var requisition = await _requisitionRepository.AllActive()
        //            .Include(r => r.RequisitionItems)
        //            .FirstOrDefaultAsync(r => r.RequisitionID == model.RequisitionId);

        //        if (requisition == null)
        //        {
        //            await _purchaseOrderRepository.RollbackTransactionAsync();
        //            response.Success = false;
        //            response.Message = "Requisition not found.";
        //            return response;
        //        }

        //        if (requisition.IsFinalApproved != true)
        //        {
        //            await _purchaseOrderRepository.RollbackTransactionAsync();
        //            response.Success = false;
        //            response.Message = "Only fully approved requisitions can be converted to purchase orders.";
        //            return response;
        //        }

        //        // Check if already converted
        //        if (requisition.RequisitionItems.Any(ri => ri.PurchaseOrderItemID != null))
        //        {
        //            await _purchaseOrderRepository.RollbackTransactionAsync();
        //            response.Success = false;
        //            response.Message = "This requisition has already been converted to a purchase order.";
        //            return response;
        //        }

        //        // Calculate totals
        //        decimal subTotal = model.Items.Sum(i => i.Quantity * i.UnitPrice);
        //        decimal taxAmount = (subTotal * model.TaxPercent) / 100;
        //        decimal grandTotal = subTotal + taxAmount;

        //        // Create Purchase Order
        //        var purchaseOrder = new PurchaseOrders
        //        {
        //            PurchaseOrderCode = model.POCode,
        //            SupplierID = model.SupplierId,
        //            PurchaseDate = DateOnly.FromDateTime(model.PurchaseDate),
        //            DueDate = DateOnly.FromDateTime(model.DueDate),
        //            OtherReference = model.OtherReference,
        //            WorkorderNo = model.WorkorderNo,
        //            SubTotal = subTotal,
        //            TaxPercent = model.TaxPercent,
        //            TaxAmount = taxAmount,
        //            GrandTotal = grandTotal,
        //            Note = model.Note,
        //            TermsAndConditions = model.TermsAndConditions,
        //            IsDraft = model.IsDraft,
        //            ShippingAddressID = model.ShippingAddressId,
        //            RequisitionID = model.RequisitionId,
        //            CreatedAt = DateTime.UtcNow,
        //            CreatedBy = baseView?.CreatedBy,
        //            LIP = baseView?.LIP,
        //            LMAC = baseView?.LMAC
        //        };

        //        await _purchaseOrderRepository.AddAsync(purchaseOrder);
        //        await _userInfoService.ActionLogAsync("purchase order", ActionName.DataAdd,
        //            null, purchaseOrder, purchaseOrder.PurchaseOrderID, baseView);

        //        // Create Purchase Order Items and link to requisition items
        //        foreach (var item in model.Items)
        //        {
        //            var poItem = new PurchaseOrderItem
        //            {
        //                PurchaseOrderID = purchaseOrder.PurchaseOrderID,
        //                ProductID = item.ProductId,
        //                Quantity = item.Quantity,
        //                UnitPrice = item.UnitPrice,
        //                Amount = item.Quantity * item.UnitPrice,
        //                CreatedAt = DateTime.UtcNow,
        //                CreatedBy = baseView?.CreatedBy,
        //                LIP = baseView?.LIP,
        //                LMAC = baseView?.LMAC
        //            };

        //            await _purchaseOrderItemRepository.AddAsync(poItem);

        //            // Link requisition item to PO item
        //            var reqItem = requisition.RequisitionItems
        //                .FirstOrDefault(ri => ri.RequisitionItemID == item.RequisitionItemId);

        //            if (reqItem != null)
        //            {
        //                reqItem.PurchaseOrderItemID = poItem.PurchaseOrderItemID;
        //                reqItem.UpdatedAt = DateTime.UtcNow;
        //                reqItem.UpdatedBy = baseView?.UpdatedBy;
        //                await _requisitionItemRepository.UpdateAsync(reqItem);
        //            }

        //            await _userInfoService.ActionLogAsync("purchase order item", ActionName.DataAdd,
        //                null, poItem, poItem.PurchaseOrderItemID, baseView);
        //        }

        //        await _purchaseOrderRepository.CommitTransactionAsync();

        //        response.Success = true;
        //        response.Message = "Purchase order created successfully.";
        //        response.Data = purchaseOrder.PurchaseOrderID;
        //        return response;
        //    }
        //    catch (Exception ex)
        //    {
        //        await _purchaseOrderRepository.RollbackTransactionAsync();
        //        response.Success = false;
        //        response.Message = "Error creating purchase order: " + ex.Message;
        //        return response;
        //    }
        //}

        //public async Task<string> GetNextPOCodeAsync()
        //{
        //    var lastCode = await _purchaseOrderRepository.AllActive()
        //        .OrderByDescending(po => po.PurchaseOrderID)
        //        .Select(po => po.PurchaseOrderCode)
        //        .FirstOrDefaultAsync();

        //    string yearPart = DateTime.Now.ToString("yy");
        //    int nextNumber = 1;

        //    if (!string.IsNullOrEmpty(lastCode))
        //    {
        //        var parts = lastCode.Split('-');
        //        if (parts.Length == 3 && parts[1] == yearPart)
        //        {
        //            if (int.TryParse(parts[2], out int lastNumber))
        //            {
        //                nextNumber = lastNumber + 1;
        //            }
        //        }
        //    }

        //    return $"PO-{yearPart}-{nextNumber.ToString("D6")}";
        //}

        public Task<byte[]> GeneratePDF(int orgId, string? fromDate, string? toDate)
        {
            throw new NotImplementedException("PDF generation not yet implemented");
        }

        public Task<byte[]> GenerateExcel(int orgId, string? fromDate, string? toDate)
        {
            throw new NotImplementedException("Excel generation not yet implemented");
        }

        public Task<CommonReturnViewModel> ConvertToPurchaseOrderAsync(ConvertToPurchaseOrderViewModel model, int? empId, BaseViewModel? baseView)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetNextPOCodeAsync()
        {
            throw new NotImplementedException();
        }
    }


}
