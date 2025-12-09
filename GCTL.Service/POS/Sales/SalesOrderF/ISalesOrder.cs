using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Sales.SalesOrders;

namespace GCTL.Service.POS.Sales.SalesOrderF
{
    public interface ISalesOrder
    {
        Task<string> GetNextSOcode();
        Task<CommonReturnViewModel> SaveAsync(SalesOrderViewModel vm);
    }
}
