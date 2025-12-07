using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Sales.SalesOrders
{
    public class SalesOrderViewModel : BaseViewModel
    {
        public int? Id { get; set; }
        public DateTime OrderDate { get; set; }
        public bool IsDraft { get; set; }
        public string OrderNumber { get; set; }
        public int? SelectedCustomerId { get; set; }
        public int? SelectedQuotationId { get; set; }
        public decimal VatPercent { get; set; }
        public string Note { get; set; }
        public List<SalesOrderItem> Items { get; set; } = new List<SalesOrderItem>();

        // For display
        public decimal SubTotal => Items?.Sum(i => i.Amount) ?? 0;
        public decimal VatAmount => (SubTotal * VatPercent) / 100;
        public decimal GrandTotal => SubTotal - VatAmount;

        // Customer list for dropdown
        public List<CustomerDto> Customers { get; set; } = new List<CustomerDto>();
    }

    public class SalesOrderItem
    {
        public int SL { get; set; }
        public string Description { get; set; }
        public int Unit { get; set; }
        public decimal? Area { get; set; }
        public decimal? Rate { get; set; }
        public decimal? Quantity { get; set; }
        public string? LIP { get; set; }
        public string? LMAC { get; set; }

        public decimal Amount => (Area ?? 0) * (Rate ?? 0);
    }

    public class CustomerDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string TaxNumber { get; set; }
    }
}
