using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Purchase.PurchaseReceive;
using GCTL.Core.ViewModels.POS.Requsition.AddRequisition;

namespace GCTL.Service.POS.Purchase.PurchaseReceive
{
    public interface IPurchaseReceiveService
    {
        Task<PaginatedResultCommon<PurchaseReceiveListViewModel>> GetPurchaseReceivesAsync(
            int? orgId, int page, int pageSize, string search, string sortColumn,
            string sortDirection, int? statusId, int? supplierId, string? fromDate, string? toDate);

        Task<List<OpenPurchaseOrderViewModel>> GetOpenPurchaseOrdersAsync(int? orgId);

        Task<PODetailsForReceiveViewModel> GetPODetailsForReceiveAsync(int poVersionId);

        Task<ReceiveDetailsViewModel> GetReceiveDetailsAsync(int receiveId);

        Task<CommonReturnViewModel> CreatePurchaseReceiveAsync(
            CreatePurchaseReceiveViewModel model, int? empId, BaseViewModel? baseView);

        Task<CommonReturnViewModel> UpdatePurchaseReceiveAsync(
            EditPurchaseReceiveViewModel model, int? empId, BaseViewModel? baseView);

        Task<CommonReturnViewModel> DeletePurchaseReceiveAsync(
            int id, int? empId, BaseViewModel? baseView);

        Task<string> GetNextPRNumberAsync();

        Task<byte[]> GeneratePDF(int orgId, string? fromDate, string? toDate);

        Task<byte[]> GenerateExcel(int orgId, string? fromDate, string? toDate);
    }
}