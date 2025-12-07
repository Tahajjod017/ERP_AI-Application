using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Sales.InvoiceF
{
    public class InvoiceViewModel : BaseViewModel
    {
        public int? Id { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string InvoiceNumber { get; set; }
        public int? SelectedCustomerId { get; set; }
        public int? SelectedSalesOrderId { get; set; }
        public bool IsDraft { get; set; }
        public decimal VatPercent { get; set; }
        public string? OtherReference { get; set; }
        public string? InvoiceNote { get; set; }
        public List<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();

        // Addresses
        public AddressViewModel BillingAddress { get; set; }
        public AddressViewModel ShippingAddress { get; set; }

        // For display
        public decimal SubTotal => Items?.Sum(i => i.Amount) ?? 0;
        public decimal VatAmount => (SubTotal * VatPercent) / 100;
        public decimal GrandTotal => SubTotal + VatAmount;

        // Customer list for dropdown
        public List<CustomerDto> Customers { get; set; } = new List<CustomerDto>();
    }

    public class InvoiceItem
    {
        public int SL { get; set; }
        public int ProductId { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }

        public decimal Amount => (Quantity ?? 0) * (UnitPrice ?? 0);
    }

    public class AddressViewModel
    {
        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public string FullAddress { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string Phone { get; set; }
        public string? Email { get; set; }
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
