using GCTL.Core.ViewModels.RequisitionDistribution.Purchase.CreatePO;

namespace GCTL.Service.RequisitionDistribution.CreatePO
{
    public interface ICreatePurchaseOrder
    {

        Task<List<VendorViewModel>> GetVendorsAsync();
        Task<List<ShippingAddressViewModel>> GetShippingAddressesAsync();
        Task<bool> AddVendorAsync(VendorViewModel vendor);
        Task<bool> AddShippingAddressAsync(ShippingAddressViewModel address);
        Task<(bool Success, string Message, int PurchaseId)> SavePurchaseOrderAsync(PurchaseOrderViewModel model);
        Task<(bool Success, string Message, int PurchaseId)> SavePurchaseOrderAsync(PurchaseOrderSubmissionViewModel model);
        Task<string> GetNextPO();
        Task<(object success, object message, object purchaseId)> SaveManualPurchaseOrderAsync(PurchaseOrderSubmissionViewModel model);
    }
}