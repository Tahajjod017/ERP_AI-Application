using System;
using System.Linq;
using System.Threading.Tasks;
using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Purchase.PurchaseOrder;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.MasterSetup.Statuse;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace GCTL.Service.POS.Purchase.PurchaseOrder
{
    public class PurchaseOrderService : IPurchaseOrder
    {
        private readonly IGenericRepository<PurchasOrders> _purchaseOrderRepository;
        private readonly IGenericRepository<PurchasOrderItemVersions> _purchaseOrderItemRepository;
        private readonly IGenericRepository<PurchasOrderVersions> _purchaseOrderVersionsRepository;
        private readonly IGenericRepository<PurOrderBaseSAddresses> _purchaseOrderAddressRepository;
        private readonly IGenericRepository<Statuses> _statusRepository;
        private readonly IUserInfoService _userInfoService;
        private readonly IStatusService _statusService;

        public PurchaseOrderService(
            IGenericRepository<PurchasOrders> purchaseOrderRepository,
            IGenericRepository<PurchasOrderItemVersions> purchaseOrderItemRepository,
            IUserInfoService userInfoService,
            IGenericRepository<PurchasOrderVersions> purchaseOrderVersionsRepository,
            IGenericRepository<Statuses> statusRepository,
            IStatusService statusService,
            IGenericRepository<PurOrderBaseSAddresses> purchaseOrderAddressRepository)
        {
            _purchaseOrderRepository = purchaseOrderRepository;
            _purchaseOrderItemRepository = purchaseOrderItemRepository;
            _userInfoService = userInfoService;
            _purchaseOrderVersionsRepository = purchaseOrderVersionsRepository;
            _statusRepository = statusRepository;
            _statusService = statusService;
            _purchaseOrderAddressRepository = purchaseOrderAddressRepository;
        }

        public async Task<string> GetNextPOCode()
        {
            var lastPO = await _purchaseOrderRepository.AllActive()
                .OrderByDescending(i => i.POID)
                .Select(i => i.POID)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastPO) && lastPO.StartsWith("PO"))
            {
                string numericPart = lastPO.Substring(2);
                if (int.TryParse(numericPart, out int parsed))
                {
                    nextNumber = parsed + 1;
                }
            }

            string nextPO = $"PO{nextNumber.ToString("D5")}";
            return nextPO;
        }

        public async Task<CommonReturnViewModel> SaveAsync(PurchaseOrderViewModel vm, int? org)
        {
            await _purchaseOrderItemRepository.OpenTransactionAsync();

            try
            {
                var OpenStatus = await _statusService.GetStatusIDAsync("Open" , "po");              
                var DraftStatus = await _statusService.GetStatusIDAsync("Draft", "po");

              

                // ============================================================
                // 1️⃣ CHECK IF EXISTING VERSION
                // ============================================================
                var prevVersion = await _purchaseOrderVersionsRepository.AllActive()
                    .FirstOrDefaultAsync(e => e.PurchasOrderVersionID == vm.Id);

                if (prevVersion != null && prevVersion.IsDraft != false)
                {
                    

                    // 🔹 Update header fields
                    prevVersion.SupplierID = vm.SelectedSupplierId;
                    prevVersion.PurchaseDate = vm.PurchaseDate;
                    prevVersion.DueDate = vm.DueDate;
                    prevVersion.OtherReference = vm.OtherReference;
                    prevVersion.WorkorderNo = vm.WorkorderNo;
                    prevVersion.WorkOrderDate = vm.WorkOrderDate;
                    prevVersion.OBBillingAddressID = vm.BillingAddressId;
                    prevVersion.OBShipingAddressID = vm.SelectedShippingAddressId;
                    prevVersion.OrganizationID = vm.OrganizationId;
                    prevVersion.OrganizationBranchID = vm.OrganizationBranchId;
                    prevVersion.PaymentMethodID = vm.PaymentMethodId;
                    prevVersion.BankAccountInfoID = vm.BankAccountInfoId;
                    prevVersion.CheckNumber = vm.CheckNumber;
                    prevVersion.CheckDate = vm.CheckDate;
                    prevVersion.Note = vm.Note;
                    prevVersion.TermsAndConditions = vm.TermsAndConditions;
                    prevVersion.AttachmentLink = vm.AttachmentLink;
                    prevVersion.TaxPercent = vm.TaxPercent;
                    prevVersion.TaxAmount = vm.TaxAmount;
                    prevVersion.TotalAmount = vm.SubTotal;
                    prevVersion.GrandTotalAmount = vm.GrandTotal;
                    prevVersion.PaidAmount = vm.PaidAmount;
                    prevVersion.DueAmount = vm.DueAmount;
                    prevVersion.IsDraft = vm.IsDraft;
                    prevVersion.IsFinal = !vm.IsDraft;
                    prevVersion.UpdatedAt = DateTime.Now;
                    prevVersion.UpdatedBy = vm.CreatedBy;

                    // 🔹 Remove old items
                    var existingItems = await _purchaseOrderItemRepository.All()
                        .Where(i => i.PurchasOrderVersionID == prevVersion.PurchasOrderVersionID)
                        .ToListAsync();

                    await _purchaseOrderItemRepository.DeleteRangeAsync(existingItems);

                    // 🔹 Add new items
                    foreach (var itemVm in vm.Items)
                    {
                        var newItem = new PurchasOrderItemVersions
                        {
                            PurchasOrderVersionID = prevVersion.PurchasOrderVersionID,
                            ProductID = itemVm.ProductId,
                            Quantity = itemVm.Quantity,
                            UnitPrice = itemVm.UnitPrice,
                            CreatedAt = DateTime.Now,
                            CreatedBy = vm.CreatedBy
                        };

                        await _purchaseOrderItemRepository.AddAsync(newItem);
                    }

                    await _purchaseOrderVersionsRepository.UpdateAsync(prevVersion);
                    await _purchaseOrderItemRepository.CommitTransactionAsync();

                    return new CommonReturnViewModel
                    {
                        Success = true,
                        Message = "Save Successful",
                        Data = prevVersion.PurchasOrderVersionID
                    };
                }
                else
                {
                    // ============================================================
                    // 2️⃣ CREATE NEW PURCHASE ORDER + VERSION
                    // ============================================================

                    // 🔹 Find latest purchase order series
                    var previousPO = await _purchaseOrderRepository.AllActive()
                        .Include(e => e.PurchasOrderVersions)
                        .Where(q => q.POID == vm.POID)
                        .FirstOrDefaultAsync();

                    int version = (previousPO?.PurchasOrderVersions.Count ?? 0) + 1;
                    bool latest = true;

                    if (previousPO == null)
                    {
                        // New purchase order series
                        vm.POID = await GetNextPOCode();
                        version = 1;
                        latest = true;

                        previousPO = new PurchasOrders
                        {
                            POID = vm.POID,
                            POStatusID = vm.IsDraft ? DraftStatus : OpenStatus,
                            CreatedAt = DateTime.Now,
                            CreatedBy = vm.CreatedBy
                        };

                        await _purchaseOrderRepository.AddAsync(previousPO);
                    }

                    // 🔹 Create new version
                    var newVersion = new PurchasOrderVersions
                    {
                        PurchasOrderID = previousPO.PurchasOrderID,
                        SupplierID = vm.SelectedSupplierId,
                        PurchaseDate = vm.PurchaseDate,
                        DueDate = vm.DueDate,
                        OtherReference = vm.OtherReference,
                        WorkorderNo = vm.WorkorderNo,
                        WorkOrderDate = vm.WorkOrderDate,
                        OBBillingAddressID = vm.BillingAddressId,
                        OBShipingAddressID = vm.SelectedShippingAddressId,
                        OrganizationID = org,
                        OrganizationBranchID = vm.OrganizationBranchId,
                        PaymentMethodID = vm.PaymentMethodId,
                        BankAccountInfoID = vm.BankAccountInfoId,
                        CheckNumber = vm.CheckNumber,
                        CheckDate = vm.CheckDate,
                        Note = vm.Note,
                        TermsAndConditions = vm.TermsAndConditions,
                        AttachmentLink = vm.AttachmentLink,
                        TaxPercent = vm.TaxPercent,
                        TaxAmount = vm.TaxAmount,
                        TotalAmount = vm.SubTotal,
                        GrandTotalAmount = vm.GrandTotal,
                        PaidAmount = vm.PaidAmount,
                        DueAmount = vm.DueAmount,
                        TotalReceiveCount = 0,
                        IsDraft = vm.IsDraft,
                        IsFinal = !vm.IsDraft,
                        CreatedAt = DateTime.Now,
                        CreatedBy = vm.CreatedBy,
                        
                    };

                    await _purchaseOrderVersionsRepository.AddAsync(newVersion);

                    // 🔹 Add items
                    foreach (var itemVm in vm.Items)
                    {
                        var modelItem = new PurchasOrderItemVersions
                        {
                            PurchasOrderVersionID = newVersion.PurchasOrderVersionID,
                            ProductID = itemVm.ProductId,
                            Quantity = itemVm.Quantity,
                            UnitPrice = itemVm.UnitPrice,
                            CreatedAt = DateTime.Now,
                            CreatedBy = vm.CreatedBy
                        };

                        await _purchaseOrderItemRepository.AddAsync(modelItem);
                    }

                    await _purchaseOrderItemRepository.CompleteTransactionAsync();

                    return new CommonReturnViewModel
                    {
                        Success = true,
                        Message = "Save Successful",
                        Data = newVersion.PurchasOrderVersionID
                    };
                }
            }
            catch (Exception ex)
            {
                await _purchaseOrderItemRepository.AbortTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
    }
}
