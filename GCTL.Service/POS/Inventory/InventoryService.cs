using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Inventory;
using GCTL.Core.ViewModels.POS.Requsition.AddRequisition;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;

namespace GCTL.Service.POS.Inventory
{
    public class InventoryService : IInventoryService
    {
        private readonly IGenericRepository<Data.Models.Inventory> _inventoryRepository;
        private readonly IGenericRepository<InventoryTransactionHistory> _transactionRepository;
        private readonly IGenericRepository<Locations> _locationRepository;
        private readonly IGenericRepository<Products> _productRepository;
        private readonly IUserInfoService _userInfoService;

        public InventoryService(
            IGenericRepository<Data.Models.Inventory> inventoryRepository,
            IGenericRepository<InventoryTransactionHistory> transactionRepository,
            IGenericRepository<Locations> locationRepository,
            IGenericRepository<Products> productRepository,
            IUserInfoService userInfoService)
        {
            _inventoryRepository = inventoryRepository;
            _transactionRepository = transactionRepository;
            _locationRepository = locationRepository;
            _productRepository = productRepository;
            _userInfoService = userInfoService;
        }

        #region Core Operations (FOR TODO INTEGRATION)
        public async Task ReceiveStockAsync(ReceiveStockViewModel model)
        {
            await _inventoryRepository.BeginTransactionAsync();

            try
            {
                // Get or create inventory record
                var inventory = await _inventoryRepository.AllActive()
                    .FirstOrDefaultAsync(i => i.ProductID == model.ProductID &&
                                             i.LocationID == model.LocationID);

                if (inventory == null)
                {
                    // Create new inventory record
                    inventory = new Data.Models.Inventory
                    {
                        ProductID = model.ProductID,
                        LocationID = model.LocationID,
                        Quantity = model.Quantity,
                        AverageCost = model.UnitCost,
                        LastTransactionDate = model.TransactionDate,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = model.CreatedBy,
                        LIP = model.LIP,
                        LMAC = model.LMAC
                    };
                    await _inventoryRepository.AddAsync(inventory);
                }
                else
                {
                    // Update existing using weighted average cost
                    var oldTotalCost = (inventory.Quantity ?? 0) * (inventory.AverageCost ?? 0);
                    var newTotalCost = model.Quantity * model.UnitCost;
                    var newTotalQuantity = (inventory.Quantity ?? 0) + model.Quantity;

                    inventory.Quantity = newTotalQuantity;
                    inventory.AverageCost = newTotalQuantity > 0
                        ? (oldTotalCost + newTotalCost) / newTotalQuantity
                        : model.UnitCost;
                    inventory.LastTransactionDate = model.TransactionDate;
                    inventory.UpdatedAt = DateTime.UtcNow;
                    inventory.UpdatedBy = model.UpdatedBy;

                    await _inventoryRepository.UpdateAsync(inventory);
                }

                // Create transaction history
                var transaction = new InventoryTransactionHistory
                {
                    InventoryID = inventory.InventoryID,
                    ProductID = model.ProductID,
                    TransactionType = 1, // IN - map to your status
                    Quantity = model.Quantity,
                    UnitPrice = model.UnitCost,
                    ReferenceType = model.ReferenceType,
                    ReferenceID = model.ReferenceID,
                    TransactionDate = model.TransactionDate,
                    BalanceAfter = inventory.Quantity,
                    Note = model.Note,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = model.CreatedBy,
                    LIP = model.LIP,
                    LMAC = model.LMAC
                };

                await _transactionRepository.AddAsync(transaction);
                await _inventoryRepository.CommitTransactionAsync();
            }
            catch
            {
                await _inventoryRepository.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task ReverseStockAsync(ReverseStockViewModel model)
        {
            await _inventoryRepository.BeginTransactionAsync();

            try
            {
                var inventory = await _inventoryRepository.AllActive()
                    .FirstOrDefaultAsync(i => i.ProductID == model.ProductID &&
                                             i.LocationID == model.LocationID);

                if (inventory != null)
                {
                    inventory.Quantity = (inventory.Quantity ?? 0) - model.Quantity;
                    inventory.LastTransactionDate = model.TransactionDate;
                    inventory.UpdatedAt = DateTime.UtcNow;
                    inventory.UpdatedBy = model.UpdatedBy;

                    await _inventoryRepository.UpdateAsync(inventory);

                    // Create reverse transaction
                    var transaction = new InventoryTransactionHistory
                    {
                        InventoryID = inventory.InventoryID,
                        ProductID = model.ProductID,
                        TransactionType = 2, // OUT - map to your status
                        Quantity = -model.Quantity, // Negative for OUT
                        TransactionDate = model.TransactionDate,
                        BalanceAfter = inventory.Quantity,
                        Note = model.Note,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = model.CreatedBy,
                        LIP = model.LIP,
                        LMAC = model.LMAC
                    };

                    await _transactionRepository.AddAsync(transaction);
                }

                await _inventoryRepository.CommitTransactionAsync();
            }
            catch
            {
                await _inventoryRepository.RollbackTransactionAsync();
                throw;
            }
        }
        #endregion

        #region Dashboard
        public async Task<InventoryDashboardViewModel> GetDashboardDataAsync(int? orgId)
        {
            var inventory = await _inventoryRepository.AllActive()
                .Include(i => i.Product)
                .Include(i => i.Location)
                .Where(i => i.Location.OrganizationID == orgId)
                .ToListAsync();

            var totalValue = inventory.Sum(i => (i.Quantity ?? 0) * (i.AverageCost ?? 0));
            var lowStockItems = inventory.Count(i => (i.Quantity ?? 0) <= (i.MinimumQuantity ?? 0) && (i.Quantity ?? 0) > 0);
            var outOfStock = inventory.Count(i => (i.Quantity ?? 0) == 0);

            var stockByLocation = inventory
                .GroupBy(i => i.Location.LocationName)
                .Select(g => new LocationStockSummary
                {
                    LocationName = g.Key,
                    ProductCount = g.Count(),
                    TotalValue = g.Sum(i => (i.Quantity ?? 0) * (i.AverageCost ?? 0))
                })
                .ToList();

            var lowStockAlerts = inventory
                .Where(i => (i.Quantity ?? 0) <= (i.MinimumQuantity ?? 0) && (i.Quantity ?? 0) > 0)
                .Select(i => new LowStockItemViewModel
                {
                    ProductID = i.ProductID ?? 0,
                    ProductName = i.Product.ProductName,
                    LocationName = i.Location.LocationName,
                    CurrentQuantity = i.Quantity ?? 0,
                    MinimumQuantity = i.MinimumQuantity ?? 0,
                    Shortage = (i.MinimumQuantity ?? 0) - (i.Quantity ?? 0)
                })
                .Take(10)
                .ToList();

            var topProducts = inventory
                .OrderByDescending(i => (i.Quantity ?? 0) * (i.AverageCost ?? 0))
                .Take(10)
                .Select(i => new TopProductViewModel
                {
                    ProductName = i.Product.ProductName,
                    Quantity = i.Quantity ?? 0,
                    Value = (i.Quantity ?? 0) * (i.AverageCost ?? 0)
                })
                .ToList();

            return new InventoryDashboardViewModel
            {
                TotalStockValue = totalValue,
                TotalProducts = inventory.Count,
                LowStockItems = lowStockItems,
                OutOfStockItems = outOfStock,
                StockByLocation = stockByLocation,
                LowStockAlerts = lowStockAlerts,
                TopProductsByValue = topProducts
            };
        }

        public async Task<StockChartDataViewModel> GetStockByLocationChartAsync(int? orgId)
        {
            var stockByLocation = await _inventoryRepository.AllActive()
                .Include(i => i.Location)
                .Where(i => i.Location.OrganizationID == orgId)
                .GroupBy(i => i.Location.LocationName)
                .Select(g => new
                {
                    Location = g.Key,
                    Value = g.Sum(i => (i.Quantity ?? 0) * (i.AverageCost ?? 0)),
                    Count = g.Count()
                })
                .ToListAsync();

            return new StockChartDataViewModel
            {
                Labels = stockByLocation.Select(s => s.Location).ToList(),
                Values = stockByLocation.Select(s => s.Value).ToList(),
                ProductCounts = stockByLocation.Select(s => s.Count).ToList()
            };
        }

        public async Task<List<TransactionHistoryViewModel>> GetRecentTransactionsAsync(int? orgId, int count)
        {
            return await _transactionRepository.AllActive()
                .Include(t => t.Product)
                .Include(t => t.Inventory)
                    .ThenInclude(i => i.Location)
                .Include(t => t.CreatedByNavigation)
                .Where(t => t.Inventory.Location.OrganizationID == orgId)
                .OrderByDescending(t => t.CreatedAt)
                .Take(count)
                .Select(t => new TransactionHistoryViewModel
                {
                    TransactionID = t.InventoryTransactionHistoryID,
                    TransactionDate = t.TransactionDate ?? t.CreatedAt ?? DateTime.UtcNow,
                    TransactionType = t.TransactionType == 1 ? "IN" : "OUT",
                    ProductName = t.Product.ProductName,
                    LocationName = t.Inventory.Location.LocationName,
                    Quantity = t.Quantity ?? 0,
                    UnitCost = t.UnitPrice ?? 0,
                    BalanceAfter = t.BalanceAfter ?? 0,
                    ReferenceType = t.ReferenceType,
                    Note = t.Note,
                    CreatedByName = t.CreatedByNavigation != null
                        ? $"{t.CreatedByNavigation.FirstName} {t.CreatedByNavigation.LastName}"
                        : "System"
                })
                .ToListAsync();
        }
        #endregion


        public async Task<InvPaginatedResultCommon<InventoryStockViewModel>> GetInventoryStockAsync(
        int? orgId, int page, int pageSize, string search, string sortColumn,
        string sortDirection, int? locationId, int? productTypeId, bool? lowStockOnly)
        {
            var query = _inventoryRepository.AllActive()
                .Include(i => i.Product)
                    .ThenInclude(p => p.ProductType)
                .Include(i => i.Product)
                    .ThenInclude(p => p.ProductBrand)
                .Include(i => i.Product)
                    .ThenInclude(p => p.UnitType)
                .Include(i => i.Location)
                .AsQueryable();

            // Apply organization filter
            if (orgId.HasValue)
            {
                query = query.Where(i => i.Location.OrganizationID == orgId);
            }

            // Apply location filter
            if (locationId.HasValue)
            {
                query = query.Where(i => i.LocationID == locationId);
            }

            // Apply product type filter
            if (productTypeId.HasValue)
            {
                query = query.Where(i => i.Product.ProductTypeID == productTypeId);
            }

            // Apply low stock filter
            if (lowStockOnly == true)
            {
                query = query.Where(i =>
                    i.Quantity <= i.MinimumQuantity &&
                    i.MinimumQuantity.HasValue &&
                    i.Quantity > 0);
            }

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(i =>
                    i.Product.ProductName.Contains(search) ||
                    i.Product.SKU.Contains(search) ||
                    i.Product.Barcode.Contains(search) ||
                    i.Location.LocationName.Contains(search) ||
                    i.Product.ProductPartNo.Contains(search));
            }

            // Calculate total records
            int totalRecords = await query.CountAsync();

            // Apply sorting
            if (!string.IsNullOrEmpty(sortColumn))
            {
                bool isDescending = sortDirection?.ToLower() == "desc";
                query = ApplySorting(query, sortColumn, isDescending);
            }
            else
            {
                query = query.OrderByDescending(i => i.CreatedAt);
            }

            // Apply pagination
            query = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            // Project to ViewModel
            var data = await query
                .Select(i => new InventoryStockViewModel
                {
                    InventoryID = i.InventoryID,
                    ProductID = i.ProductID,
                    ProductName = i.Product.ProductName,
                    ProductType = i.Product.ProductType != null ? i.Product.ProductType.ProductTypeName : null,
                    Brand = i.Product.ProductBrand != null ? i.Product.ProductBrand.ProductBrandName : null,
                    Unit = i.Product.UnitType != null ? i.Product.UnitType.UnitTypeName : null,
                    LocationName = i.Location.LocationName,
                    Quantity = i.Quantity ?? 0,
                    ReservedQuantity = i.ReservedQuantity,
                    AvailableQuantity = (i.Quantity ?? 0) - i.ReservedQuantity,
                    AverageCost = i.AverageCost ?? 0,
                    TotalValue = (i.Quantity ?? 0) * (i.AverageCost ?? 0),
                    MinimumQuantity = i.MinimumQuantity ?? 0,
                    MaximumQuantity = i.MaximumQuantity ?? 0,
                    LastTransactionDate = i.LastTransactionDate,
                    IsLowStock = i.MinimumQuantity.HasValue && i.Quantity <= i.MinimumQuantity,
                    IsOutOfStock = i.Quantity <= 0
                })
                .ToListAsync();

            return new InvPaginatedResultCommon<InventoryStockViewModel>
            {
                Data = data,
                TotalRecords = totalRecords,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize)
            };
        }

        public async Task<ProductStockByLocationViewModel> GetProductStockByLocationAsync(int productId, int? orgId)
        {
            // Validate product exists
            var product = await _productRepository.AllActive()
                .Include(p => p.ProductType)
                .Include(p => p.UnitType)
                .FirstOrDefaultAsync(p => p.ProductID == productId);

            if (product == null)
                throw new Exception($"Product with ID {productId} not found");

            // Get inventory for this product
            var query = _inventoryRepository.AllActive()
                .Include(i => i.Location)
                .Where(i => i.ProductID == productId);

            if (orgId.HasValue)
            {
                query = query.Where(i => i.Location.OrganizationID == orgId);
            }

            var inventoryList = await query.ToListAsync();

            var locations = inventoryList.Select(i => new LocationStockDetail
            {
                LocationID = i.LocationID,
                LocationName = i.Location.LocationName,
                Quantity = i.Quantity ?? 0,
                ReservedQuantity = i.ReservedQuantity,
                AvailableQuantity = (i.Quantity ?? 0) - i.ReservedQuantity
            }).ToList();

            return new ProductStockByLocationViewModel
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                Locations = locations
            };
        }

        public async Task<InvPaginatedResultCommon<TransactionHistoryViewModel>> GetTransactionHistoryAsync(
            int? orgId, int page, int pageSize, string search, string sortColumn,
            string sortDirection, int? locationId, int? productId, string transactionType,
            string? fromDate, string? toDate)
        {
            var query = _transactionRepository.AllActive()
                .Include(t => t.Product)
                .Include(t => t.Inventory)
                    .ThenInclude(i => i.Location)
                .Include(t => t.FromLocation)
                .Include(t => t.ToLocation)
                .Include(t => t.TransactionTypeNavigation)
                .Include(t => t.CreatedByNavigation)
                .AsQueryable();

            // Apply organization filter
            if (orgId.HasValue)
            {
                query = query.Where(t =>
                    (t.FromLocation != null && t.FromLocation.OrganizationID == orgId) ||
                    (t.ToLocation != null && t.ToLocation.OrganizationID == orgId) ||
                    (t.Inventory != null && t.Inventory.Location.OrganizationID == orgId));
            }

            // Apply location filter
            if (locationId.HasValue)
            {
                query = query.Where(t =>
                    t.FromLocationID == locationId ||
                    t.ToLocationID == locationId ||
                    (t.Inventory != null && t.Inventory.LocationID == locationId));
            }

            // Apply product filter
            if (productId.HasValue)
            {
                query = query.Where(t => t.ProductID == productId);
            }

            // Apply transaction type filter
            if (!string.IsNullOrEmpty(transactionType))
            {
                if (int.TryParse(transactionType, out int typeId))
                {
                    query = query.Where(t => t.TransactionType == typeId);
                }
                else
                {
                    query = query.Where(t =>
                        t.TransactionTypeNavigation != null &&
                        t.TransactionTypeNavigation.StatusName == transactionType);
                }
            }

            // Apply date range filter
            if (!string.IsNullOrEmpty(fromDate) && DateTime.TryParse(fromDate, out DateTime fromDateParsed))
            {
                query = query.Where(t => t.TransactionDate >= fromDateParsed.Date);
            }

            if (!string.IsNullOrEmpty(toDate) && DateTime.TryParse(toDate, out DateTime toDateParsed))
            {
                query = query.Where(t => t.TransactionDate <= toDateParsed.Date.AddDays(1).AddSeconds(-1));
            }

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t =>
                    t.Product.ProductName.Contains(search) ||
                    t.Product.SKU.Contains(search) ||
                    t.ReferenceType.Contains(search) ||
                    t.Note.Contains(search));
            }

            // Calculate total records
            int totalRecords = await query.CountAsync();

            // Apply sorting
            if (!string.IsNullOrEmpty(sortColumn))
            {
                bool isDescending = sortDirection?.ToLower() == "desc";
                query = ApplySorting(query, sortColumn, isDescending);
            }
            else
            {
                query = query.OrderByDescending(t => t.TransactionDate);
            }

            // Apply pagination
            query = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            // Project to ViewModel
            var data = await query
                .Select(t => new TransactionHistoryViewModel
                {
                    TransactionID = t.InventoryTransactionHistoryID,
                    TransactionDate = t.TransactionDate ?? t.CreatedAt ?? DateTime.MinValue,
                    TransactionType = t.TransactionTypeNavigation != null ?
                        t.TransactionTypeNavigation.StatusName : "Unknown",
                    ProductName = t.Product.ProductName,
                    LocationName = t.Inventory != null ? t.Inventory.Location.LocationName :
                                  (t.ToLocation != null ? t.ToLocation.LocationName :
                                   t.FromLocation != null ? t.FromLocation.LocationName : null),
                    Quantity = t.Quantity ?? 0,
                    UnitCost = t.UnitPrice ?? 0,
                    BalanceAfter = t.BalanceAfter ?? 0,
                    ReferenceType = t.ReferenceType,
                    ReferenceNumber = t.ReferenceID.ToString() ?? "N/A",
                    Note = t.Note,
                    CreatedByName = t.CreatedByNavigation != null ?
                        $"{t.CreatedByNavigation.FirstName} {t.CreatedByNavigation.LastName}" : "System"
                })
                .ToListAsync();

            return new InvPaginatedResultCommon<TransactionHistoryViewModel>
            {
                Data = data,
                TotalRecords = totalRecords,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize)
            };
        }

        public async Task<CommonReturnViewModel> CreateStockAdjustmentAsync(
            StockAdjustmentViewModel model, int? empId, BaseViewModel? baseView)
        {
            // Validate product
            var product = await _productRepository.AllActive()
                .FirstOrDefaultAsync(p => p.ProductID == model.ProductID);
            if (product == null)
                throw new Exception("Product not found");

            // Validate location
            var location = await _locationRepository.AllActive()
                .FirstOrDefaultAsync(l => l.LocationID == model.LocationID);
            if (location == null)
                throw new Exception("Location not found");

            // Get or create inventory
            var inventory = await _inventoryRepository.AllActive()
                .FirstOrDefaultAsync(i => i.ProductID == model.ProductID && i.LocationID == model.LocationID);

            decimal oldQuantity = 0;
            decimal newQuantity = 0;
            decimal adjustmentQuantity = 0;

            if (inventory == null)
            {
                // Create new inventory for Add/SET adjustments only
                if (model.AdjustmentType != "Add" && model.AdjustmentType != "Set")
                    throw new Exception("Cannot remove stock from non-existent inventory");

                inventory = new Data.Models.Inventory
                {
                    ProductID = model.ProductID,
                    LocationID = model.LocationID,
                    Quantity = model.Quantity,
                    AverageCost = model.NewAverageCost,
                    ReservedQuantity = 0,
                    CreatedBy = empId,
                    CreatedAt = DateTime.Now,
                    LIP = baseView?.LIP,
                    LMAC = baseView?.LMAC
                };
                await _inventoryRepository.AddAsync(inventory);

                oldQuantity = 0;
                newQuantity = model.Quantity;
                adjustmentQuantity = model.Quantity;
            }
            else
            {
                oldQuantity = inventory.Quantity ?? 0;

                switch (model.AdjustmentType.ToLower())
                {
                    case "add":
                        newQuantity = oldQuantity + model.Quantity;
                        adjustmentQuantity = model.Quantity;
                        break;

                    case "remove":
                        if (oldQuantity < model.Quantity)
                            throw new Exception($"Insufficient stock. Available: {oldQuantity}, Requested: {model.Quantity}");

                        newQuantity = oldQuantity - model.Quantity;
                        adjustmentQuantity = -model.Quantity;
                        break;

                    case "set":
                        newQuantity = model.Quantity;
                        adjustmentQuantity = model.Quantity - oldQuantity;
                        break;

                    default:
                        throw new Exception($"Invalid adjustment type: {model.AdjustmentType}");
                }

                inventory.Quantity = newQuantity;
                inventory.UpdatedBy = empId;
                inventory.UpdatedAt = DateTime.Now;
                inventory.LastTransactionDate = DateTime.Now;

                if (model.NewAverageCost.HasValue)
                {
                    inventory.AverageCost = model.NewAverageCost;
                }

                await _inventoryRepository.UpdateAsync(inventory);
            }

            // Create transaction record
            var transactionTypeId = model.AdjustmentType.ToLower() switch
            {
                "add" => 1,    // Stock In
                "remove" => 2, // Stock Out
                "set" => 3,    // Adjustment
                _ => 3
            };

            var transaction = new InventoryTransactionHistory
            {
                ProductID = model.ProductID,
                InventoryID = inventory.InventoryID,
                Quantity = adjustmentQuantity,
                UnitPrice = model.NewAverageCost,
                TransactionType = transactionTypeId,
                TransactionDate = DateTime.Now,
                ReferenceType = "STOCK_ADJUSTMENT",
                Note = $"{model.Reason}: {model.Note}",
                BalanceAfter = newQuantity,
                FromLocationID = model.AdjustmentType.ToLower() == "remove" ? model.LocationID : null,
                ToLocationID = model.AdjustmentType.ToLower() == "add" ? model.LocationID : null,
                CreatedBy = empId,
                CreatedAt = DateTime.Now,
                LIP = baseView?.LIP,
                LMAC = baseView?.LMAC
            };

            await _transactionRepository.AddAsync(transaction);

            return new CommonReturnViewModel
            {
                Success = true,
                Message = "Stock adjustment completed successfully",
                Data = new
                {
                    InventoryID = inventory.InventoryID,
                    TransactionID = transaction.InventoryTransactionHistoryID,
                    OldQuantity = oldQuantity,
                    NewQuantity = newQuantity,
                    AdjustmentType = model.AdjustmentType
                }
            };
        }

        public async Task<InvPaginatedResultCommon<AdjustmentHistoryViewModel>> GetAdjustmentHistoryAsync(
            int? orgId, int page, int pageSize, string search, string? fromDate, string? toDate)
        {
            var query = _transactionRepository.AllActive()
                .Include(t => t.Product)
                .Include(t => t.Inventory)
                    .ThenInclude(i => i.Location)
                .Include(t => t.CreatedByNavigation)
                .Where(t => t.ReferenceType == "STOCK_ADJUSTMENT")
                .AsQueryable();

            // Apply organization filter
            if (orgId.HasValue)
            {
                query = query.Where(t =>
                    t.Inventory != null &&
                    t.Inventory.Location != null &&
                    t.Inventory.Location.OrganizationID == orgId);
            }

            // Apply date range filter
            if (!string.IsNullOrEmpty(fromDate) && DateTime.TryParse(fromDate, out DateTime fromDateParsed))
            {
                query = query.Where(t => t.TransactionDate >= fromDateParsed.Date);
            }

            if (!string.IsNullOrEmpty(toDate) && DateTime.TryParse(toDate, out DateTime toDateParsed))
            {
                query = query.Where(t => t.TransactionDate <= toDateParsed.Date.AddDays(1).AddSeconds(-1));
            }

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t =>
                    t.Product.ProductName.Contains(search) ||
                    t.Note.Contains(search) ||
                    t.CreatedByNavigation.FirstName.Contains(search) ||
                    t.CreatedByNavigation.LastName.Contains(search));
            }

            // Calculate total records
            int totalRecords = await query.CountAsync();

            // Apply sorting and pagination
            var data = await query
                .OrderByDescending(t => t.TransactionDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new AdjustmentHistoryViewModel
                {
                    AdjustmentID = t.InventoryTransactionHistoryID,
                    AdjustmentDate = t.TransactionDate ?? t.CreatedAt ?? DateTime.MinValue,
                    ProductName = t.Product.ProductName,
                    LocationName = t.Inventory != null ? t.Inventory.Location.LocationName : null,
                    AdjustmentType = (t.Quantity ?? 0) > 0 ? "Add" : "Remove",
                    Quantity = Math.Abs(t.Quantity ?? 0),
                    Reason = t.Note.Contains(":") ? "" : t.Note,
                    Note = t.Note.Contains(":") ? t.Note.Substring(t.Note.IndexOf(':') + 1).Trim() : null,
                    AdjustedByName = t.CreatedByNavigation != null ?
                        $"{t.CreatedByNavigation.FirstName} {t.CreatedByNavigation.LastName}" : "System"
                })
                .ToListAsync();

            return new InvPaginatedResultCommon<AdjustmentHistoryViewModel>
            {
                Data = data,
                TotalRecords = totalRecords,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize)
            };
        }

        // Helper method for sorting
        private IQueryable<T> ApplySorting<T>(IQueryable<T> query, string sortColumn, bool isDescending)
        {
            try
            {
                var parameter = Expression.Parameter(typeof(T), "x");
                var property = Expression.Property(parameter, sortColumn);
                var lambda = Expression.Lambda(property, parameter);

                string methodName = isDescending ? "OrderByDescending" : "OrderBy";
                var resultExpression = Expression.Call(
                    typeof(Queryable),
                    methodName,
                    new Type[] { typeof(T), property.Type },
                    query.Expression,
                    Expression.Quote(lambda));

                return query.Provider.CreateQuery<T>(resultExpression);
            }
            catch
            {
                // If sorting fails, return default sort
                return isDescending ?
                    query.OrderByDescending(x => EF.Property<object>(x, "CreatedAt")) :
                    query.OrderBy(x => EF.Property<object>(x, "CreatedAt"));
            }
        }



        // NOTE: Due to token limits, remaining methods follow same patterns:
        // - GetInventoryStockAsync: Query inventory with filters, joins, pagination
        // - GetTransactionHistoryAsync: Query transactions with filters, pagination
        // - CreateStockAdjustmentAsync: Create adjustment transaction, update inventory
        // - GenerateReports: Use reporting library or implement PDF/Excel generation

        //public Task<PaginatedResultCommon<InventoryStockViewModel>> GetInventoryStockAsync(
        //    int? orgId, int page, int pageSize, string search, string sortColumn,
        //    string sortDirection, int? locationId, int? productTypeId, bool? lowStockOnly)
        //{
        //    // TODO: Implement similar to PurchaseReceiveService.GetPurchaseReceivesAsync
        //    throw new NotImplementedException();
        //}

        //public Task<ProductStockByLocationViewModel> GetProductStockByLocationAsync(int productId, int? orgId)
        //{
        //    // TODO: Query inventory grouped by location for specific product
        //    throw new NotImplementedException();
        //}

        //public Task<PaginatedResultCommon<TransactionHistoryViewModel>> GetTransactionHistoryAsync(
        //    int? orgId, int page, int pageSize, string search, string sortColumn,
        //    string sortDirection, int? locationId, int? productId, string transactionType,
        //    string? fromDate, string? toDate)
        //{
        //    // TODO: Implement with filters similar to purchase receives
        //    throw new NotImplementedException();
        //}

        //public Task<CommonReturnViewModel> CreateStockAdjustmentAsync(
        //    StockAdjustmentViewModel model, int? empId, BaseViewModel? baseView)
        //{
        //    // TODO: Implement adjustment logic similar to ReceiveStockAsync
        //    throw new NotImplementedException();
        //}

        //public Task<PaginatedResultCommon<AdjustmentHistoryViewModel>> GetAdjustmentHistoryAsync(
        //    int? orgId, int page, int pageSize, string search, string? fromDate, string? toDate)
        //{
        //    // TODO: Query adjustments from transaction history
        //    throw new NotImplementedException();
        //}

        //public Task<byte[]> GenerateStockReportPDF(int? orgId, int? locationId, int? productTypeId, bool? lowStockOnly)
        //{
        //    // TODO: Implement PDF generation
        //    throw new NotImplementedException();
        //}

        public Task<byte[]> GenerateStockReportExcel(int? orgId, int? locationId, int? productTypeId, bool? lowStockOnly)
        {
            // TODO: Implement Excel generation
            throw new NotImplementedException();
        }

        public Task<byte[]> GenerateMovementReportPDF(int? orgId, string? fromDate, string? toDate, int? locationId, int? productId)
        {
            // TODO: Implement movement report PDF
            throw new NotImplementedException();
        }

        public Task<byte[]> GenerateStockReportPDF(int? orgId, int? locationId, int? productTypeId, bool? lowStockOnly)
        {
            throw new NotImplementedException();
        }
    }

}
