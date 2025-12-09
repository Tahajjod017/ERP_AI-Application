using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.POS.Sales.SalesOrderList;

namespace GCTL.Service.POS.Sales.SalesOrderList
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
