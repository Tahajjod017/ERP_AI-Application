using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Sales.SalesOrders;

namespace GCTL.Service.Sales.SalesOrdersF
{
    public interface ISalesOrder
    {
        Task<string> GetNextSOcode();
        Task<CommonReturnViewModel> SaveAsync(SalesOrderViewModel vm);
    }
}
