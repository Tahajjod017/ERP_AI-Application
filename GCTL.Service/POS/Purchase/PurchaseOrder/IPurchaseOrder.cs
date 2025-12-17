using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Purchase.PurchaseOrder;

namespace GCTL.Service.POS.Purchase.PurchaseOrder
{
    public interface IPurchaseOrder
    {
        Task<string> GetNextPOCode();
        Task<CommonReturnViewModel> SaveAsync(PurchaseOrderViewModel vm);
    }
}
