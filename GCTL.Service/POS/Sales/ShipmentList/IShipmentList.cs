using GCTL.Core.ViewModels.POS.Sales.ShipmentList;

namespace GCTL.Service.POS.Sales.ShipmentList
{
    public interface IShipmentList
    {
        Task<ShiPaginatedResultData<ShipmentListDto>> GetShipmentsWithPagination(
            int page,
            int pageSize,
            string searchTerm,
            string sortColumn,
            string sortDirection);
    }
}