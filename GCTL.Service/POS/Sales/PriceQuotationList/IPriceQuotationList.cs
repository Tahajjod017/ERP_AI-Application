using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Sales.PriceQuotationList;

namespace GCTL.Service.POS.Sales.PriceQuotationList
{
    public interface IPriceQuotationList
    {
        Task<PaginatedResultData<PriceQuotationListDto>> GetPriceQuotationsWithPagination(int page, int pageSize, string searchTerm, string sortColumn, string sortDirection);
    }
}