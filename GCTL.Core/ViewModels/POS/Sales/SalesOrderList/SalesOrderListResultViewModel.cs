using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.POS.Sales.SalesOrderList
{
    public class SalesOrderListResultViewModel
    {
        public List<SalesOrderListItemViewModel> Data { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
    }

    public class SalesOrderListItemViewModel
    {
        public int SalesOrderID { get; set; }
        public string SalesOrderNumber { get; set; }
        public string CustomerName { get; set; }
        public string QuotationNumber { get; set; }
        public string CreatedBy { get; set; }
        public DateTime SalesOrderDate { get; set; }
        public int TotalItems { get; set; }
        public decimal VatPercentage { get; set; }
        public string Note { get; set; }
    }
}
