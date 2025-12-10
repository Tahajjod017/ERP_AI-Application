using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.RequisitionDistribution.ProductPurchase;
using GCTL.Data.Models;

namespace GCTL.Service.RequisitionDistribution.ProductPurchase
{
    public interface IProductPurchaseService
    {
        Task<(List<ProductPurchaseOrderListViewModel> Purchases, int TotalCount)> GetAllProductPurchasesAsync(int page, int pageSize, string searchTerm, string sortBy, string sortOrder, string productType);
        Task<PurchaseEntryViewModel> GetProductPurchaseByIdAsync(int id);
        Task<CommonReturnViewModel> CreateProductPurchaseAsync(PurchaseEntryViewModel purchase);
        Task<CommonReturnViewModel> UpdateProductPurchaseAsync(PurchaseEntryViewModel purchase);
        Task<bool> DeleteProductPurchaseAsync(int id);

    }
}