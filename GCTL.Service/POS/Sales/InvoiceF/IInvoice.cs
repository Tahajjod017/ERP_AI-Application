using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Sales.InvoiceF;

namespace GCTL.Service.POS.Sales.InvoiceF
{
    public interface IInvoice
    {
        Task<string> GetNextInvoiceCode();
        Task<CommonReturnViewModel> SaveAsync(InvoiceViewModel vm, bool v);
        Task<CommonReturnViewModel> UpdateAsync(InvoiceViewModel vm);
    }
}
