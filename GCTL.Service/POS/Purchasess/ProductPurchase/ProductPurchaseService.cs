using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Purchasess.ProductPurchase;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using Microsoft.EntityFrameworkCore;
using static Dapper.SqlMapper;

namespace GCTL.Service.POS.Purchasess.ProductPurchase
{
    public class ProductPurchaseService : IProductPurchaseService
    {
        //private readonly IGenericRepository<Requisitions> _requisitionRepository;
        private readonly IGenericRepository<Inventory> _inventoryRepository;
       // private readonly IGenericRepository<Purchases> _purchaseRepository;
        private readonly IGenericRepository<Wirehouses> _wireHouseRepository;
        //private readonly IGenericRepository<RequisitionItems> _requisitionItemRepository;
        private readonly IGenericRepository<PurchasOrders> _purchasOrdersRepository;
        private readonly IUserInfoService _userInfoService;

        public ProductPurchaseService(IGenericRepository<Inventory> inventoryRepository, IGenericRepository<Wirehouses> wireHouseRepository, IGenericRepository<PurchasOrders> purchasOrdersRepository, IUserInfoService userInfoService)
        {
            _inventoryRepository = inventoryRepository;
            _wireHouseRepository = wireHouseRepository;
            _purchasOrdersRepository = purchasOrdersRepository;
            _userInfoService = userInfoService;
        }



        //private static List<Purchases> _purchases = new List<Purchases>
        //{
        //    new Purchases
        //    {
        //        PurchasesID = 1,
        //        SupplierID = 1,
        //        ProductID = 1,
        //        Quantity = 50,
        //        UnitDistribute = 125.00m,
        //        CreatedBy = 1,
        //        PurchaseBy = 1,
        //        RequisitionID = 9849,
        //        CreatedAt = DateTime.Now.AddDays(-5),
        //        UpdatedAt = DateTime.Now.AddDays(-2)
        //    },
        //    new Purchases
        //    {
        //        PurchasesID = 2,
        //        SupplierID = 2,
        //        ProductID = 2,
        //        Quantity = 100,
        //        UnitDistribute = 85.50m,
        //        CreatedBy = 2,
        //        PurchaseBy = 2,
        //        RequisitionID = 9850,
        //        CreatedAt = DateTime.Now.AddDays(-3),
        //        UpdatedAt = DateTime.Now.AddDays(-1)
        //    },
        //    new Purchases
        //    {
        //        PurchasesID = 3,
        //        SupplierID = 3,
        //        ProductID = 3,
        //        Quantity = 25,
        //        UnitDistribute = 450.00m,
        //        CreatedBy = 1,
        //        PurchaseBy = 3,
        //        RequisitionID = 9851,
        //        CreatedAt = DateTime.Now.AddDays(-1),
        //        UpdatedAt = DateTime.Now
        //    }
        //};



        public async Task<(List<ProductPurchaseOrderListViewModel> Purchases, int TotalCount)> GetAllProductPurchasesAsync(int page, int pageSize, string searchTerm, string sortBy, string sortOrder, string productType)
        {

            var query = _purchasOrdersRepository.AllActive().Include(e=>e.PurchaseReceives)

                .Select(e => new ProductPurchaseOrderListViewModel
                {
                    PurchasOrderID = e.PurchasOrderID,
                    POID = e.POID,
                    Supplier = e.Supplier.FullName,
                    PODate = e.PurchaseDate,
                    Note = e.Note,
                    TotalProduct = e.PurchasOrderItems.Select(m => m.ProductID).Distinct().Count(),
                    TotalQuentity  = e.PurchasOrderItems.Sum(m => m.Quantity),
                    TotalPrice = e.PurchasOrderItems.Sum(m => m.UnitPrice),
                    Received = e.PurchaseReceives.Any()
                }).AsQueryable();


           


            // Search
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(p =>
                    //p.RequisitionID.ToString().Contains(searchTerm) ||
                    p.PurchasOrderID.ToString().Contains(searchTerm) ||
                    p.Supplier.ToString().Contains(searchTerm) ||
                    p.Note.ToString().Contains(searchTerm) ||
                    p.POID.ToString().Contains(searchTerm));
            }

           
            //if (!string.IsNullOrEmpty(productType))
            //{
            //    var productTypeId = int.Parse(productType);
            //    query = query.Where(p => p.ProductTypeID == productTypeId);
            //}

          
            int totalCount = query.Count();

          

            switch (sortBy.ToLower())
            {
                case "reqId":
                    query = sortOrder.ToLower() == "asc" ? query.OrderBy(p => p.PurchasOrderID) : query.OrderByDescending(p => p.PurchasOrderID);
                    break;
                case "productName":
                    query = sortOrder.ToLower() == "asc" ? query.OrderBy(p => p.POID) : query.OrderByDescending(p => p.POID);
                    break;
                case "purpose":
                    query = sortOrder.ToLower() == "asc" ? query.OrderBy(p => p.TotalProduct) : query.OrderByDescending(p => p.TotalProduct);
                    break;
                case "type":
                    query = sortOrder.ToLower() == "asc" ? query.OrderBy(p => p.PODate) : query.OrderByDescending(p => p.PODate);
                    break;
                case "quantity":
                    query = sortOrder.ToLower() == "asc" ? query.OrderBy(p => p.TotalPrice) : query.OrderByDescending(p => p.TotalPrice);
                    break;
                default:
                    query = query.OrderByDescending(p => p.PurchasOrderID);
                    break;
            }

            // Pagination
            var purchases = await Task.FromResult(query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList());

            return (purchases, totalCount);
        }

        public async Task<PurchaseEntryViewModel> GetProductPurchaseByIdAsync(int id)
        {
            return await Task.FromResult(new PurchaseEntryViewModel());
        }

        public async Task<CommonReturnViewModel> CreateProductPurchaseAsync(PurchaseEntryViewModel purchase)
        {
            var restult = await UpdateProductPurchaseAsync(purchase);

            return restult;
        }

        public async Task<CommonReturnViewModel> UpdateProductPurchaseAsync(PurchaseEntryViewModel model)
        {
            await _inventoryRepository.BeginTransactionAsync();

            try
            {
                //var purchase = new
                //{
                //    SupplierID = model.SupplierID != 0 ? model.SupplierID : null,
                //    ProductID = model.ProductID != 0 ? model.ProductID : null,
                //    Quantity = model.Quantity,
                //    UnitDistribute = model.UnitDistribute,
                //    PurchaseBy = model.PurchaseBy != 0 ? model.PurchaseBy : null,
                //    RequisitionID = model.Id != 0 ? model.Id : null,
                //};
                //await _purchaseRepository.AddAsync(purchase, model);

                var location = 3; // await _wireHouseRepository.AllActive().Where(e=>e.WirehouseID == model.WarehouseID).Select(e=>e.LocationID).FirstOrDefaultAsync();

                var existingInventory = await _inventoryRepository.AllActive()
                    .FirstOrDefaultAsync(i => i.ProductID == model.ProductID && i.LocationID == location);

                if (existingInventory != null)
                {
                    existingInventory.Quantity += model.Quantity;
                    existingInventory.UnitPrice = model.UnitPrice;
                    await _inventoryRepository.UpdateAsync(existingInventory, model);
                }
                else
                {
                    var newInventory = new Inventory
                    {
                        ProductID = model.ProductID != 0 ? model.ProductID : null,
                        Quantity = model.Quantity,
                        UnitPrice = model.UnitPrice,
                        LocationID = location,
                        CreatedBy = model.CreatedBy,
                        CreatedAt = DateTime.Now
                    };
                    await _inventoryRepository.AddAsync(newInventory, model);
                    await _userInfoService.ActionLogAsync("newInventory", ActionName.DataAdd, null, newInventory, newInventory.LocationID, model);
                }
                await _inventoryRepository.CommitTransactionAsync();
                return new CommonReturnViewModel { Success = true, Message = "Product purchase updated successfully." };
            }
            catch (Exception ex)
            {
                 await _inventoryRepository.RollbackTransactionAsync();
                return new CommonReturnViewModel { Success = false, Message = "An error occurred while updating the product purchase. " + ex.Message };
                
            }

            

            

        }

        public async Task<bool> DeleteProductPurchaseAsync(int id)
        {
            //var purchase = _purchases.FirstOrDefault(p => p.PurchasesID == id);
            //if (purchase != null)
            //{
            //    purchase.DeletedAt = DateTime.Now;
            //    return await Task.FromResult(true);
            //}
            return await Task.FromResult(false);
        }

        private int GetProductTypeId(int? productId)
        {
            var productTypeMap = new Dictionary<int, int>
            {
                { 1, 1 }, // 1700 Scaffolding -> Shuttering Materials
                { 2, 2 }, // Steel Rod 16mm -> Steel Materials
                { 3, 3 }, // Cement Bag (50kg) -> Construction Materials
                { 4, 4 }, // Brick (1000 pcs) -> Construction Materials
                { 5, 5 }, // Safety Helmet -> Safety Equipment
                { 6, 6 }, // Concrete Mixer -> Construction Materials
                { 7, 7 }, // Metal Sheets -> Steel Materials
                { 8, 8 }  // Wire Mesh -> Steel Materials
            };
            return productTypeMap.ContainsKey(productId ?? 0) ? productTypeMap[productId ?? 0] : 0;
        }




    }
}
