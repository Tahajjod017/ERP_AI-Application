using GCTL.Core.ViewModels.POS.Sales.ShipmentList;

namespace GCTL.Service.POS.Sales.ShipmentList
{
    public interface IChallanList
    {
        Task<ShiPaginatedResultData<ShipmentListDto>> GetChallanWithPagination(int page, int pageSize, string searchTerm, string sortColumn, string sortDirection);
    }
}