using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Sales.Shipment;

namespace GCTL.Service.POS.Sales.Shipment
{
    public interface IChallan
    {
        Task<string> GetNextShipmentNumber();
        Task<CommonReturnViewModel> SaveAsync(ChallanViewModel vm);
        Task<CommonReturnViewModel> UpdateStatusAsync(int shipmentId, int statusId, int userId);
    }
}