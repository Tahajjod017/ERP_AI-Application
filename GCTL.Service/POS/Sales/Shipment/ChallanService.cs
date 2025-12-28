using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bogus.DataSets;
using GCTL.Core.Enums;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Sales.Shipment;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.MasterSetup.Statuse;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.POS.Sales.Shipment
{
    public class ChallanService : IChallan
    {
        private readonly IGenericRepository<Challans> _challanRepository;
        private readonly IGenericRepository<ChallanItems> _challanItemRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Inventory> _inventoryRepository;
        private readonly IGenericRepository<InventoryTransactionHistory> _inventoryTransactionRepository;
        private readonly IGenericRepository<SalesOrdersVersions> _salesOrderVersionRepository;

        private readonly IUserInfoService _userInfoService;
        private readonly IStatusService _StatusService;

        public ChallanService(
            IGenericRepository<Challans> shipmentRepository,
            IGenericRepository<ChallanItems> shipmentItemRepository,
            IGenericRepository<GCTL.Data.Models.Inventory> inventoryRepository,
            IGenericRepository<InventoryTransactionHistory> inventoryTransactionRepository,
            IUserInfoService userInfoService,
            IGenericRepository<SalesOrdersVersions> salesOrderVersionRepository,
            IStatusService statusService)
        {
            _challanRepository = shipmentRepository;
            _challanItemRepository = shipmentItemRepository;
            _inventoryRepository = inventoryRepository;
            _inventoryTransactionRepository = inventoryTransactionRepository;
            _userInfoService = userInfoService;
            _salesOrderVersionRepository = salesOrderVersionRepository;
            _StatusService = statusService;
        }

        public async Task<string> GetNextShipmentNumber()
        {
            var lastShipment = await _challanRepository.AllActive()
                .OrderByDescending(i => i.ChallanNumber)
                .Select(i => i.ChallanNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastShipment) && lastShipment.StartsWith("CLN"))
            {
                string numericPart = lastShipment.Substring(3);
                if (int.TryParse(numericPart, out int parsed))
                {
                    nextNumber = parsed + 1;
                }
            }

            string nextShipment = $"CLN{nextNumber.ToString("D5")}";
            return nextShipment;
        }

        public async Task<CommonReturnViewModel> SaveAsync(ChallanViewModel vm)
        {
            await _challanItemRepository.BeginTransactionAsync();

            try
            {
                // ============================================================
                // 1️⃣ CHECK IF EXISTING SHIPMENT (Update scenario)
                // ============================================================
                var prevShipment = await _challanRepository.AllActive()
                    .FirstOrDefaultAsync(e => e.ChallanID == vm.Id);

                if (prevShipment != null)
                {
                    // 🔹 Update header fields
                    prevShipment.ChallanDate = vm.ShipmentDate.Value;
                    prevShipment.ExpectedDeliveryDate = vm.ExpectedDeliveryDate;
                    prevShipment.ActualDeliveryDate = vm.ActualDeliveryDate;
                    prevShipment.DeliveryMethodID = vm.ShippingMethodId;
                    prevShipment.TrackingNumber = vm.TrackingNumber;
                    prevShipment.DeliveryAddressID = vm.ShippingAddressId;
                    prevShipment.DeliveryCost = vm.ShippingCost;
                    prevShipment.Note = vm.Note;
                    prevShipment.UpdatedAt = DateTime.Now;
                    prevShipment.UpdatedBy = vm.CreatedBy;

                    // 🔹 Remove old items
                    var existingItems = await _challanItemRepository.All()
                        .Where(i => i.ChallanID == prevShipment.ChallanID)
                        .ToListAsync();

                    await _challanItemRepository.DeleteRangeAsync(existingItems);

                    // 🔹 Add new items
                    foreach (var itemVm in vm.Items)
                    {
                        var newItem = new ChallanItems
                        {
                            ChallanID = prevShipment.ChallanID,
                            ProductID = itemVm.ProductId,
                            OrderedQuantity = itemVm.OrderedQuantity,
                            DeliveredQuantity = itemVm.ShippedQuantity,
                            FromLocationID = itemVm.FromLocationId,
                            Note = itemVm.Note,
                            CreatedAt = DateTime.Now,
                            CreatedBy = vm.CreatedBy
                        };

                        await _challanItemRepository.AddAsync(newItem);
                    }

                    await _challanRepository.UpdateAsync(prevShipment);
                    await _challanItemRepository.CommitTransactionAsync();

                    return new CommonReturnViewModel
                    {
                        Success = true,
                        Message = "Shipment Updated Successfully",
                        Data = prevShipment.ChallanID
                    };
                }
                else
                {
                    // ============================================================
                    // 2️⃣ CREATE NEW SHIPMENT
                    // ============================================================
                    vm.ShipmentNumber = await GetNextShipmentNumber();

                    var statys = _StatusService.GetStatusID("Pending");

                    var salesOrder = await _salesOrderVersionRepository.AllActive().Where(e => e.SalesOrdersVersionID == vm.SalesOrderId).Select(e => e.SalesOrdersVersionID).FirstOrDefaultAsync();

                    var newShipment = new Challans
                    {
                        ChallanNumber = vm.ShipmentNumber,
                        SalesOrdersVersionID = salesOrder,
                        InvoiceID = vm.InvoiceId,
                        ChallanDate = vm.ShipmentDate.Value,
                        ExpectedDeliveryDate = vm.ExpectedDeliveryDate,
                        ActualDeliveryDate = vm.ActualDeliveryDate,
                        DeliveryMethodID = vm.ShippingMethodId,
                        TrackingNumber = vm.TrackingNumber,
                        DeliveryAddressID = vm.ShippingAddressId,
                        DeliveryCost = vm.ShippingCost,
                        Note = vm.Note,
                        StatusID = statys, //vm.StatusId ?? 1, // Default: Pending
                        CreatedAt = DateTime.Now,
                        CreatedBy = vm.CreatedBy
                    };

                    await _challanRepository.AddAsync(newShipment);

                    // 🔹 Add items
                    foreach (var itemVm in vm.Items)
                    {
                        var modelItem = new ChallanItems
                        {
                            ChallanID = newShipment.ChallanID,
                            ProductID = itemVm.ProductId,
                            OrderedQuantity = itemVm.OrderedQuantity,
                            DeliveredQuantity = itemVm.ShippedQuantity,
                            FromLocationID = itemVm.FromLocationId,
                            Note = itemVm.Note,
                            CreatedAt = DateTime.Now,
                            CreatedBy = vm.CreatedBy
                        };

                        await _challanItemRepository.AddAsync(modelItem);
                    }

                    await _challanItemRepository.CommitTransactionAsync();

                    return new CommonReturnViewModel
                    {
                        Success = true,
                        Message = "Shipment Created Successfully",
                        Data = newShipment.ChallanID
                    };
                }
            }
            catch (Exception ex)
            {
                await _challanItemRepository.RollbackTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<CommonReturnViewModel> UpdateStatusAsync(int challanId, int statusId, int userId)
        {
            await _inventoryRepository.OpenTransactionAsync();

            string name = Enum.GetName(typeof(ShipmentStatus), statusId);

            var statys = await _StatusService.GetStatusIDAsync(name);


            try
            {
                var shipment = await _challanRepository.AllActive()
                    .Include(s => s.ChallanItems)
                    .FirstOrDefaultAsync(s => s.ChallanID == challanId);

                if (shipment == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Challan not found"
                    };
                }

               

                // If status is Shipped or Delivered, deduct inventory
                //if (statusId == 3 || statusId == 5) // Shipped or Delivered
                if (name == "Shipped") // ||name == "Delivered") // Shipped or Delivered
                {
                    foreach (var item in shipment.ChallanItems)
                    {
                       var res =  await DeductInventory(item.ProductID.Value, item.DeliveredQuantity.Value, item.FromLocationID.Value, challanId, userId);

                        if (!res)
                        {
                            await _inventoryRepository.AbortTransactionAsync();
                            return new CommonReturnViewModel
                            {
                                Success = false,
                                Message = "Product short in storage"
                            };
                        }
                    }
                }

                var shipped = await _StatusService.GetStatusIDAsync("Shipped");

                if (name == "Cancelled" && shipment.StatusID == shipped) // Shipped or Delivered
                {
                    foreach (var item in shipment.ChallanItems)
                    {
                       var res =  await ReverseInventory(item.ProductID.Value, item.DeliveredQuantity.Value, item.FromLocationID.Value, challanId, userId);

                        
                    }
                }

                shipment.StatusID = statys;
                shipment.UpdatedBy = userId;
                shipment.UpdatedAt = DateTime.Now;


                await _challanRepository.UpdateAsync(shipment);

                await _inventoryRepository.CompleteTransactionAsync();


                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Status updated successfully"
                };
            }
            catch (Exception ex)
            {
                await _inventoryRepository.AbortTransactionAsync();

                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        private async Task<bool> ReverseInventory(int productId, decimal quantity, int locationId, int challanId, int userId)
        {
            try
            {

                var inventory = await _inventoryRepository.AllActive().FirstOrDefaultAsync(i => i.ProductID == productId && i.LocationID == locationId);

                var statys = await _StatusService.GetStatusIDAsync("Inbound");


                if (inventory != null)
                {
                    inventory.Quantity += quantity;
                    inventory.ReservedQuantity += quantity;
                    inventory.LastTransactionDate = DateTime.Now;
                    inventory.UpdatedBy = userId;
                    inventory.UpdatedAt = DateTime.Now;

                    await _inventoryRepository.UpdateAsync(inventory);

                    // Log transaction
                    var transaction = new InventoryTransactionHistory
                    {
                        ProductID = productId,
                        Quantity = -quantity,
                        TransactionType = statys, // Outbound
                        TransactionDate = DateTime.Now,
                        ReferenceType = "Shipment Return",
                        ReferenceID = challanId,
                        FromLocationID = locationId,
                        BalanceAfter = inventory.Quantity,
                        CreatedBy = userId,
                        CreatedAt = DateTime.Now
                    };

                    await _inventoryTransactionRepository.AddAsync(transaction);
                }
                else
                {
                    return false;
                }

                return true;

            }
            catch (Exception)
            {
                return false;

            }
        }

        private async Task<bool> DeductInventory(int productId, decimal quantity, int locationId, int challanId, int userId)
        {
            try
            {
                
                var inventory = await _inventoryRepository.AllActive()
                .FirstOrDefaultAsync(i => i.ProductID == productId && i.LocationID == locationId);

                var statys = await _StatusService.GetStatusIDAsync("Outbound");


                if (inventory != null && inventory.Quantity >= quantity)
                {
                    inventory.Quantity -= quantity;
                    inventory.ReservedQuantity -= quantity;
                    inventory.LastTransactionDate = DateTime.Now;
                    inventory.UpdatedBy = userId;
                    inventory.UpdatedAt = DateTime.Now;

                    await _inventoryRepository.UpdateAsync(inventory);

                    // Log transaction
                    var transaction = new InventoryTransactionHistory
                    {
                        ProductID = productId,
                        Quantity = -quantity,
                        TransactionType = statys, // Outbound
                        TransactionDate = DateTime.Now,
                        ReferenceType = "Shipment",
                        ReferenceID = challanId,
                        FromLocationID = locationId,
                        BalanceAfter = inventory.Quantity,
                        CreatedBy = userId,
                        CreatedAt = DateTime.Now
                    };

                    await _inventoryTransactionRepository.AddAsync(transaction);
                }
                else
                {
                    return false;
                }

                return true;

            }
            catch (Exception)
            {
                return false;
                
            }

            
        }
    }
}
