using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Purchasess.Purchase.PurchaseReceived;

namespace GCTL.Service.POS.Purchasess.PurchaseReceived
{
    public interface IPurchaseReceivedService
    {
        Task<CommonReturnViewModel> SavePurchaseReceivedAsync(PurchaseReceivedViewModel model);
    }
}