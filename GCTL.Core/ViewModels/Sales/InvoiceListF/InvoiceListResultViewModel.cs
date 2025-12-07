using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Sales.InvoiceListF
{
    public class InvoiceListResultViewModel
    {
        public List<InvoiceListItemViewModel> Data { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
    }

    public class InvoiceListItemViewModel
    {
        public int InvoiceID { get; set; }
        public string InvoiceNumber { get; set; }
        public string CustomerName { get; set; }
        public string SalesOrderNumber { get; set; }
        public string CreatedBy { get; set; }
        public DateTime InvoiceDate { get; set; }
        public int TotalItems { get; set; }
        public decimal VatPercentage { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
        public bool IsDraft { get; set; }
        public string Status { get; set; }
    }
}
