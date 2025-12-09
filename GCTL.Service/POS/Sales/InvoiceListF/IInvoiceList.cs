using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.POS.Sales.InvoiceListF;

namespace GCTL.Service.POS.Sales.InvoiceListF
{
    public interface IInvoiceList
    {
        Task<InvoiceListResultViewModel> GetInvoicesWithPagination(int page, int pageSize, string searchTerm, string sortColumn, string sortDirection);
    }
}
