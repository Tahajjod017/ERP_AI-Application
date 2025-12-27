using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Sales.Shipment;

namespace GCTL.Service.POS.Sales.Shipment
{
    public interface IShipment
    {
        Task<string> GetNextShipmentNumber();
        Task<CommonReturnViewModel> SaveAsync(ShipmentViewModel vm);
        Task<CommonReturnViewModel> UpdateStatusAsync(int shipmentId, int statusId, int userId);
    }
}