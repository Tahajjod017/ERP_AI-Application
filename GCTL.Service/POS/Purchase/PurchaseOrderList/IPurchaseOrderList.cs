using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.POS.Purchase.PurchaseOrderList;

namespace GCTL.Service.POS.Purchase.PurchaseOrderList
{
    public interface IPurchaseOrderList
    {
        Task<PaginatedResultData<PurchaseOrderListDto>> GetPurchaseOrdersWithPagination(
            int page,
            int pageSize,
            string searchTerm,
            string sortColumn,
            string sortDirection);
    }
}
