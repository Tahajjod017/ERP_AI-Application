using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Sales.PriceQuotation;

namespace GCTL.Service.POS.Sales.PriceQuotation
{
    public interface IPriceQuotation
    {
        Task<CommonReturnViewModel> ConvertToSalesOrder(int id , BaseViewModel? baseView);
        Task<string> GetNextPQcode();
        Task<CommonReturnViewModel> SaveAsync(PriceQuotationViewModel vm);
    }
}
