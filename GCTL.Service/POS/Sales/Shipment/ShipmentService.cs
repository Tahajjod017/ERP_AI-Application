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
    public class ShipmentService : IShipment
    {
        private readonly IGenericRepository<Shipments> _shipmentRepository;
        private readonly IGenericRepository<ShipmentItems> _shipmentItemRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Inventory> _inventoryRepository;
        private readonly IGenericRepository<InventoryTransactionHistory> _inventoryTransactionRepository;
        private readonly IGenericRepository<SalesOrdersVersions> _salesOrderVersionRepository;

        private readonly IUserInfoService _userInfoService;
        private readonly IStatusService _StatusService;

        public ShipmentService(
            IGenericRepository<Shipments> shipmentRepository,
            IGenericRepository<ShipmentItems> shipmentItemRepository,
            IGenericRepository<GCTL.Data.Models.Inventory> inventoryRepository,
            IGenericRepository<InventoryTransactionHistory> inventoryTransactionRepository,
            IUserInfoService userInfoService,
            IGenericRepository<SalesOrdersVersions> salesOrderVersionRepository,
            IStatusService statusService)
        {
            _shipmentRepository = shipmentRepository;
            _shipmentItemRepository = shipmentItemRepository;
            _inventoryRepository = inventoryRepository;
            _inventoryTransactionRepository = inventoryTransactionRepository;
            _userInfoService = userInfoService;
            _salesOrderVersionRepository = salesOrderVersionRepository;
            _StatusService = statusService;
        }

        public async Task<string> GetNextShipmentNumber()
        {
            var lastShipment = await _shipmentRepository.AllActive()
                .OrderByDescending(i => i.ShipmentNumber)
                .Select(i => i.ShipmentNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastShipment) && lastShipment.StartsWith("SHP"))
            {
                string numericPart = lastShipment.Substring(3);
                if (int.TryParse(numericPart, out int parsed))
                {
                    nextNumber = parsed + 1;
                }
            }

            string nextShipment = $"SHP{nextNumber.ToString("D5")}";
            return nextShipment;
        }

        public async Task<CommonReturnViewModel> SaveAsync(ShipmentViewModel vm)
        {
            await _shipmentItemRepository.BeginTransactionAsync();

            try
            {
                // ============================================================
                // 1️⃣ CHECK IF EXISTING SHIPMENT (Update scenario)
                // ============================================================
                var prevShipment = await _shipmentRepository.AllActive()
                    .FirstOrDefaultAsync(e => e.ShipmentID == vm.Id);

                if (prevShipment != null)
                {
                    // 🔹 Update header fields
                    prevShipment.ShipmentDate = vm.ShipmentDate.Value;
                    prevShipment.ExpectedDeliveryDate = vm.ExpectedDeliveryDate;
                    prevShipment.ActualDeliveryDate = vm.ActualDeliveryDate;
                    prevShipment.ShippingMethodID = vm.ShippingMethodId;
                    prevShipment.TrackingNumber = vm.TrackingNumber;
                    prevShipment.ShippingAddressID = vm.ShippingAddressId;
                    prevShipment.ShippingCost = vm.ShippingCost;
                    prevShipment.Note = vm.Note;
                    prevShipment.UpdatedAt = DateTime.Now;
                    prevShipment.UpdatedBy = vm.CreatedBy;

                    // 🔹 Remove old items
                    var existingItems = await _shipmentItemRepository.All()
                        .Where(i => i.ShipmentID == prevShipment.ShipmentID)
                        .ToListAsync();

                    await _shipmentItemRepository.DeleteRangeAsync(existingItems);

                    // 🔹 Add new items
                    foreach (var itemVm in vm.Items)
                    {
                        var newItem = new ShipmentItems
                        {
                            ShipmentID = prevShipment.ShipmentID,
                            ProductID = itemVm.ProductId,
                            OrderedQuantity = itemVm.OrderedQuantity,
                            ShippedQuantity = itemVm.ShippedQuantity,
                            FromLocationID = itemVm.FromLocationId,
                            Note = itemVm.Note,
                            CreatedAt = DateTime.Now,
                            CreatedBy = vm.CreatedBy
                        };

                        await _shipmentItemRepository.AddAsync(newItem);
                    }

                    await _shipmentRepository.UpdateAsync(prevShipment);
                    await _shipmentItemRepository.CommitTransactionAsync();

                    return new CommonReturnViewModel
                    {
                        Success = true,
                        Message = "Shipment Updated Successfully",
                        Data = prevShipment.ShipmentID
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

                    var newShipment = new Shipments
                    {
                        ShipmentNumber = vm.ShipmentNumber,
                        SalesOrdersVersionID = salesOrder,
                        InvoiceID = vm.InvoiceId,
                        ShipmentDate = vm.ShipmentDate.Value,
                        ExpectedDeliveryDate = vm.ExpectedDeliveryDate,
                        ActualDeliveryDate = vm.ActualDeliveryDate,
                        ShippingMethodID = vm.ShippingMethodId,
                        TrackingNumber = vm.TrackingNumber,
                        ShippingAddressID = vm.ShippingAddressId,
                        ShippingCost = vm.ShippingCost,
                        Note = vm.Note,
                        StatusID = statys, //vm.StatusId ?? 1, // Default: Pending
                        CreatedAt = DateTime.Now,
                        CreatedBy = vm.CreatedBy
                    };

                    await _shipmentRepository.AddAsync(newShipment);

                    // 🔹 Add items
                    foreach (var itemVm in vm.Items)
                    {
                        var modelItem = new ShipmentItems
                        {
                            ShipmentID = newShipment.ShipmentID,
                            ProductID = itemVm.ProductId,
                            OrderedQuantity = itemVm.OrderedQuantity,
                            ShippedQuantity = itemVm.ShippedQuantity,
                            FromLocationID = itemVm.FromLocationId,
                            Note = itemVm.Note,
                            CreatedAt = DateTime.Now,
                            CreatedBy = vm.CreatedBy
                        };

                        await _shipmentItemRepository.AddAsync(modelItem);
                    }

                    await _shipmentItemRepository.CommitTransactionAsync();

                    return new CommonReturnViewModel
                    {
                        Success = true,
                        Message = "Shipment Created Successfully",
                        Data = newShipment.ShipmentID
                    };
                }
            }
            catch (Exception ex)
            {
                await _shipmentItemRepository.RollbackTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<CommonReturnViewModel> UpdateStatusAsync(int shipmentId, int statusId, int userId)
        {
            await _inventoryRepository.OpenTransactionAsync();

            string name = Enum.GetName(typeof(ShipmentStatus), statusId);

            var statys = await _StatusService.GetStatusIDAsync(name);


            try
            {
                var shipment = await _shipmentRepository.AllActive()
                    .Include(s => s.ShipmentItems)
                    .FirstOrDefaultAsync(s => s.ShipmentID == shipmentId);

                if (shipment == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Shipment not found"
                    };
                }

               

                // If status is Shipped or Delivered, deduct inventory
                //if (statusId == 3 || statusId == 5) // Shipped or Delivered
                if (name == "Shipped") // ||name == "Delivered") // Shipped or Delivered
                {
                    foreach (var item in shipment.ShipmentItems)
                    {
                       var res =  await DeductInventory(item.ProductID.Value, item.ShippedQuantity.Value, item.FromLocationID.Value, shipmentId, userId);

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
                    foreach (var item in shipment.ShipmentItems)
                    {
                       var res =  await ReverseInventory(item.ProductID.Value, item.ShippedQuantity.Value, item.FromLocationID.Value, shipmentId, userId);

                        
                    }
                }

                shipment.StatusID = statys;
                shipment.UpdatedBy = userId;
                shipment.UpdatedAt = DateTime.Now;


                await _shipmentRepository.UpdateAsync(shipment);

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

        private async Task<bool> ReverseInventory(int productId, decimal quantity, int locationId, int shipmentId, int userId)
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
                        ReferenceID = shipmentId,
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

        private async Task<bool> DeductInventory(int productId, decimal quantity, int locationId, int shipmentId, int userId)
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
                        ReferenceID = shipmentId,
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
