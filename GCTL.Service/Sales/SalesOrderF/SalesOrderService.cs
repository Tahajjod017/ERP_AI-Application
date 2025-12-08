using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Sales.SalesOrders;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.Sales.SalesOrdersF
{

    public class SalesOrderService : ISalesOrder
    {
        private readonly IGenericRepository<BloodGroup> _bloodRepository;
        private readonly IGenericRepository<SalesOrders> _salesOrderRepository;
        private readonly IGenericRepository<SalesOrderVersionItems> _salesOrderItemRepository;
        private readonly IGenericRepository<SalesOrdersVersions> _salesOrderVersionRepository;
        private readonly IUserInfoService _userInfoService;

        public SalesOrderService(
            IGenericRepository<SalesOrders> salesOrderRepository,
            IGenericRepository<SalesOrderVersionItems> salesOrderItemRepository,
            IGenericRepository<SalesOrdersVersions> salesOrderVersionRepository,
            IUserInfoService userInfoService,
            IGenericRepository<BloodGroup> bloodRepository)
        {
            _salesOrderRepository = salesOrderRepository;
            _salesOrderItemRepository = salesOrderItemRepository;
            _salesOrderVersionRepository = salesOrderVersionRepository;
            _userInfoService = userInfoService;
            _bloodRepository = bloodRepository;
        }

        public async Task<string> GetNextSOcode()
        {
            var lastOrder = await _salesOrderRepository.AllActive()
                .OrderByDescending(i => i.SalesOrderNumber)
                .Select(i => i.SalesOrderNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastOrder) && lastOrder.StartsWith("SO"))
            {
                string numericPart = lastOrder.Substring(2);
                if (int.TryParse(numericPart, out int parsed))
                {
                    nextNumber = parsed + 1;
                }
            }

            string nextOrder = $"SO{nextNumber.ToString("D5")}";
            return nextOrder;
        }

        public async Task<CommonReturnViewModel> SaveAsync(SalesOrderViewModel vm)
        {
            await _bloodRepository.BeginTransactionAsync();

            var result = await _salesOrderVersionRepository.AllActive().FirstOrDefaultAsync(e => e.SalesOrdersVersionID == vm.Id);

           

            try
            {

                if (result != null && result.IsDraft == true)
                {
                    // Update Sales Order header
                    var salesOrder = await _salesOrderRepository
                        .FirstOrDefaultAsync(e => e.SalesOrdersID == result.SalesOrdersID);

                    if (salesOrder != null)
                    {
                        salesOrder.SalesOrderNumber = vm.OrderNumber ?? salesOrder.SalesOrderNumber;
                        await _salesOrderRepository.UpdateAsync(salesOrder, vm);
                        await _userInfoService.ActionLogAsync("SalesOrder", ActionName.DataUpdated, null, salesOrder, salesOrder.SalesOrdersID, vm);
                    }

                    // Update Sales Order version
                    result.Version = 1;
                    result.CustomerID = vm.SelectedCustomerId;
                    result.SalesOrderDate = vm.OrderDate;
                    result.VatPercentage = vm.VatPercent;
                    result.Note = vm.Note;
                    result.IsDraft = vm.IsDraft;

                    await _salesOrderVersionRepository.UpdateAsync(result, vm);
                    await _userInfoService.ActionLogAsync("SalesOrderVersion", ActionName.DataUpdated, null, result, result.SalesOrdersVersionID, vm);

                    // Refresh items: delete old, insert new
                    var existingItems = await _salesOrderItemRepository.AllActive()
                        .Where(i => i.SalesOrdersVersionID == result.SalesOrdersVersionID).ToListAsync();

                    await _salesOrderItemRepository.DeleteRangeAsync(existingItems);

                   

                    foreach (var item in vm.Items)
                    {
                        var modelItem = new SalesOrderVersionItems
                        {
                            SalesOrdersVersionID = result.SalesOrdersVersionID,
                            Description = item.Description,
                            UnitTypeID = Convert.ToInt32(item.Unit),
                            Area = item.Area,
                            Rate = item.Rate,
                            Quantity = item.Quantity,
                            LIP = item.LIP,
                            LMAC = item.LMAC,
                            CreatedAt = DateTime.Now,
                            CreatedBy = vm.CreatedBy
                        };

                        await _salesOrderItemRepository.AddAsync(modelItem);
                        await _userInfoService.ActionLogAsync("SalesOrderItem", ActionName.DataAdd, null, modelItem, modelItem.SalesOrderVersionItemID, vm);
                    }

                    // Commit transaction
                    await _salesOrderItemRepository.CommitTransactionAsync();

                    return new CommonReturnViewModel
                    {
                        Success = true,
                        Message = "Draft Sales Order updated successfully",
                        Data = result.SalesOrdersVersionID
                    };
                }
                else
                {

                    var salesOrderExists = await _salesOrderRepository.AllActive().Include(e=>e.SalesOrdersVersions)
                        .Where(so => so.SalesOrderNumber == vm.OrderNumber).FirstOrDefaultAsync();
                   

                    if (salesOrderExists == null)
                    {
                        // Generate order number if not provided
                        if (string.IsNullOrEmpty(vm.OrderNumber) || vm.OrderNumber.StartsWith("SO-"))
                        {
                            vm.OrderNumber = await GetNextSOcode();
                        }

                         salesOrderExists = new SalesOrders
                        {
                            PriceQuotationID = vm.SelectedQuotationId,
                            SalesOrderNumber = vm.OrderNumber,

                        };
                        await _salesOrderRepository.AddAsync(salesOrderExists, vm);
                        await _userInfoService.ActionLogAsync("SalesOrder", ActionName.DataAdd, null, salesOrderExists, salesOrderExists.SalesOrdersID, vm);

                    }

                    

                    var versionCount = salesOrderExists.SalesOrdersVersions.Count  + 1;



                    var version = new SalesOrdersVersions()
                    {
                        SalesOrdersID = salesOrderExists.SalesOrdersID,
                        Version = versionCount,
                        CustomerID = vm.SelectedCustomerId,
                        SalesOrderDate = vm.OrderDate,
                        VatPercentage = vm.VatPercent,
                        Note = vm.Note,
                        IsDraft = vm.IsDraft,
                        IsFinal = vm.IsDraft ? false : true,
                    };

                    await _salesOrderVersionRepository.AddAsync(version, vm);
                    await _userInfoService.ActionLogAsync("SalesOrderVersion", ActionName.DataAdd, null, version, version.SalesOrdersVersionID, vm);



                    // Save sales order items
                    foreach (var item in vm.Items)
                    {
                        var modelItem = new SalesOrderVersionItems
                        {
                            SalesOrdersVersionID = version.SalesOrdersVersionID,
                            Description = item.Description,
                            UnitTypeID = Convert.ToInt32(item.Unit),
                            Area = item.Area,
                            Rate = item.Rate,
                            Quantity = item.Quantity,
                            LIP = item.LIP,
                            LMAC = item.LMAC,
                            CreatedAt = DateTime.Now,
                            CreatedBy = vm.CreatedBy
                        };

                        await _salesOrderItemRepository.AddAsync(modelItem);
                        await _userInfoService.ActionLogAsync("SalesOrder", ActionName.DataAdd, null, modelItem, modelItem.SalesOrderVersionItemID, vm);
                    }

                    // Commit transaction
                    await _bloodRepository.CommitTransactionAsync();

                    return new CommonReturnViewModel
                    {

                        Success = true,
                        Message = "Sales Order saved successfully",
                        Data = version.SalesOrdersVersionID
                    };
                }

                
            }
            catch (Exception ex)
            {
                await _bloodRepository.RollbackTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
    }


}
