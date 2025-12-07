
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Sales.PriceQuotationList;

namespace GCTL.Service.Sales.PriceQuotationList
{
    public interface IPriceQuotationList
    {
        Task<PaginatedResultData<PriceQuotationListDto>> GetPriceQuotationsWithPagination(int page, int pageSize, string searchTerm, string sortColumn, string sortDirection);
    }
}