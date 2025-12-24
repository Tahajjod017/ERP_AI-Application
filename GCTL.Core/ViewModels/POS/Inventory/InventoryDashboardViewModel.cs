using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.POS.Inventory
{
    // Dashboard
    public class InventoryDashboardViewModel
    {
        public decimal TotalStockValue { get; set; }
        public int TotalProducts { get; set; }
        public int LowStockItems { get; set; }
        public int OutOfStockItems { get; set; }
        public List<LocationStockSummary> StockByLocation { get; set; }
        public List<LowStockItemViewModel> LowStockAlerts { get; set; }
        public List<TopProductViewModel> TopProductsByValue { get; set; }
    }

    public class LocationStockSummary
    {
        public string LocationName { get; set; }
        public int ProductCount { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class LowStockItemViewModel
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string LocationName { get; set; }
        public decimal CurrentQuantity { get; set; }
        public decimal MinimumQuantity { get; set; }
        public decimal Shortage { get; set; }
    }

    public class TopProductViewModel
    {
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
        public decimal Value { get; set; }
    }

    // Stock View
    public class InventoryStockViewModel
    {
        public int InventoryID { get; set; }
        public int? ProductID { get; set; }
        public string ProductName { get; set; }
        public string ProductType { get; set; }
        public string Brand { get; set; }
        public string Unit { get; set; }
        public string LocationName { get; set; }
        public decimal Quantity { get; set; }
        public decimal ReservedQuantity { get; set; }
        public decimal AvailableQuantity { get; set; }
        public decimal? MinimumQuantity { get; set; }
        public decimal? MaximumQuantity { get; set; }
        public decimal AverageCost { get; set; }
        public decimal TotalValue { get; set; }
        public DateTime? LastTransactionDate { get; set; }
        public bool IsLowStock { get; set; }
        public bool IsOutOfStock { get; set; }
    }

    public class ProductStockByLocationViewModel
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public List<LocationStockDetail> Locations { get; set; }
    }

    public class LocationStockDetail
    {
        public int? LocationID { get; set; }
        public string LocationName { get; set; }
        public decimal Quantity { get; set; }
        public decimal ReservedQuantity { get; set; }
        public decimal AvailableQuantity { get; set; }
    }

    // Transaction History
    public class TransactionHistoryViewModel
    {
        public int TransactionID { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; }
        public string ProductName { get; set; }
        public string LocationName { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal BalanceAfter { get; set; }
        public string ReferenceType { get; set; }
        public string? ReferenceNumber { get; set; }
        public string Note { get; set; }
        public string CreatedByName { get; set; }
    }

    // Stock Adjustment
    public class StockAdjustmentViewModel : BaseViewModel
    {
        [Required]
        public int ProductID { get; set; }

        [Required]
        public int LocationID { get; set; }

        [Required]
        public string AdjustmentType { get; set; } // "Add", "Remove", "Set"

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal Quantity { get; set; }

        [Required]
        public string Reason { get; set; } // "Damage", "Loss", "Found", "Count Correction"

        public string Note { get; set; }

        public decimal? NewAverageCost { get; set; }
    }

    public class AdjustmentHistoryViewModel
    {
        public int AdjustmentID { get; set; }
        public DateTime AdjustmentDate { get; set; }
        public string ProductName { get; set; }
        public string LocationName { get; set; }
        public string AdjustmentType { get; set; }
        public decimal Quantity { get; set; }
        public string Reason { get; set; }
        public string Note { get; set; }
        public string AdjustedByName { get; set; }
    }

    // For TODO integration
    public class ReceiveStockViewModel : BaseViewModel
    {
        public int ProductID { get; set; }
        public int LocationID { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public string ReferenceType { get; set; }
        public int ReferenceID { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Note { get; set; }
    }

    public class ReverseStockViewModel : BaseViewModel
    {
        public int ProductID { get; set; }
        public int LocationID { get; set; }
        public decimal Quantity { get; set; }
        public string ReferenceType { get; set; }
        public int ReferenceID { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Note { get; set; }
    }

    // Chart Data
    public class StockChartDataViewModel
    {
        public List<string> Labels { get; set; }
        public List<decimal> Values { get; set; }
        public List<int> ProductCounts { get; set; }
    }

   

    public class InvPaginatedResultCommon<T>
    {
        public List<T> Data { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;

        public InvPaginatedResultCommon()
        {
            Data = new List<T>();
        }

        public InvPaginatedResultCommon(List<T> data, int page, int pageSize, int totalRecords)
        {
            Data = data;
            Page = page;
            PageSize = pageSize;
            TotalRecords = totalRecords;
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
        }
    }
}
