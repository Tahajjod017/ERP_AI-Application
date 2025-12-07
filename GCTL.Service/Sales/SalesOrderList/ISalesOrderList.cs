using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.Sales.SalesOrderList;

namespace GCTL.Service.Sales.SalesOrderList
{
    public interface ISalesOrderList
    {
        Task<SalesOrderListResultViewModel> GetSalesOrdersWithPagination(
            int page,
            int pageSize,
            string searchTerm,
            string sortColumn,
            string sortDirection);
    }
}
