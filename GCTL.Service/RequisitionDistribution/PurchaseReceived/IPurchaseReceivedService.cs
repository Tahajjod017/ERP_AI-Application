using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.RequisitionDistribution.Purchase.PurchaseReceived;

namespace GCTL.Service.RequisitionDistribution.PurchaseReceived
{
    public interface IPurchaseReceivedService
    {
        Task<CommonReturnViewModel> SavePurchaseReceivedAsync(PurchaseReceivedViewModel model);
    }
}