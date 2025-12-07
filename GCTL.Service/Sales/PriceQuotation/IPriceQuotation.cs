using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Sales.PriceQuotation;

namespace GCTL.Service.Sales.PriceQuotation
{
    public interface IPriceQuotation
    {
        Task<string> GetNextPQcode();
        Task<CommonReturnViewModel> SaveAsync(PriceQuotationViewModel vm);
    }
}
