using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Inventory;
using GCTL.Core.ViewModels.POS.Requsition.AddRequisition;

namespace GCTL.Service.POS.Inventory
{
    public interface IInventoryService
    {
        // Dashboard
        Task<InventoryDashboardViewModel> GetDashboardDataAsync(int? orgId);
        Task<StockChartDataViewModel> GetStockByLocationChartAsync(int? orgId);
        Task<List<TransactionHistoryViewModel>> GetRecentTransactionsAsync(int? orgId, int count);

        // Stock View
        Task<InvPaginatedResultCommon<InventoryStockViewModel>> GetInventoryStockAsync(
            int? orgId, int page, int pageSize, string search, string sortColumn,
            string sortDirection, int? locationId, int? productTypeId, bool? lowStockOnly);

        Task<ProductStockByLocationViewModel> GetProductStockByLocationAsync(int productId, int? orgId);

        // Transaction History
        Task<InvPaginatedResultCommon<TransactionHistoryViewModel>> GetTransactionHistoryAsync(
            int? orgId, int page, int pageSize, string search, string sortColumn,
            string sortDirection, int? locationId, int? productId, string transactionType,
            string? fromDate, string? toDate);

        // Stock Adjustment
        Task<CommonReturnViewModel> CreateStockAdjustmentAsync(
            StockAdjustmentViewModel model, int? empId, BaseViewModel? baseView);

        Task<InvPaginatedResultCommon<AdjustmentHistoryViewModel>> GetAdjustmentHistoryAsync(
            int? orgId, int page, int pageSize, string search, string? fromDate, string? toDate);

        // Core Operations (for TODO integration)
        Task ReceiveStockAsync(ReceiveStockViewModel model);
        Task ReverseStockAsync(ReverseStockViewModel model);

        // Reports
        Task<byte[]> GenerateStockReportPDF(int? orgId, int? locationId, int? productTypeId, bool? lowStockOnly);
        Task<byte[]> GenerateStockReportExcel(int? orgId, int? locationId, int? productTypeId, bool? lowStockOnly);
        Task<byte[]> GenerateMovementReportPDF(int? orgId, string? fromDate, string? toDate, int? locationId, int? productId);
    }
}