using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.POS.Sales.PriceQuotationDetails;

namespace GCTL.Core.ViewModels.POS.Sales.InvoiceDetailsF
{
    public class InvoiceDetailsViewModel : BaseViewModel
    {
        public int? Id { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string InvoiceNumber { get; set; }
        public int? SelectedCustomerId { get; set; }
        public int? SelectedSalesOrderId { get; set; }
        public string SalesOrderNumber { get; set; }
        public bool IsDraft { get; set; }
        public decimal VatPercent { get; set; }
        public decimal SubTotal { get; set; }
        public decimal VatAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal PaidAmount { get; set; }
        public string OtherReference { get; set; }
        public string InvoiceNote { get; set; }
        public List<InvoiceItemDetails> Items { get; set; } = new List<InvoiceItemDetails>();

        // Addresses
        public AddressViewModel BillingAddress { get; set; }
        public AddressViewModel ShippingAddress { get; set; }

        // Payment History
        public List<PaymentHistoryViewModel> PaymentHistory { get; set; } = new List<PaymentHistoryViewModel>();

        // Sidebar data
        public string CreatedByName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string UpdatedByName { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Customer data
        public CustomerDetailsViewModel CustomerData { get; set; }

        // Customer list for dropdown
        public List<CustomerDto> Customers { get; set; } = new List<CustomerDto>();
        public bool Finalized { get; set; }
    }

    public class InvoiceItemDetails
    {
        public int SL { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public decimal Amount => Quantity * UnitPrice;
    }

    public class AddressViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullAddress { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }

    public class PaymentHistoryViewModel
    {
        public int PaymentTransactionID { get; set; }
        public string TransactionRefNo { get; set; }
        public DateTime TransactionDate { get; set; }
        public string PaymentMethodName { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
    }

    public class CustomerDetailsInvoiceViewModel
    {
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string TaxNumber { get; set; }
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

    public class InvoiceSidebarDetailsViewModel
    {
        public int? InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public string SalesOrderNumber { get; set; }
        public bool IsDraft { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
        public string CreatedByName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string UpdatedByName { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public bool CanEdit => IsDraft;
        public bool CanSendEmail => true;
        public bool IsFullyPaid => DueAmount <= 0;

        public List<PriceQuotationVersionViewModel> InvoiceIdList { get; set; }
    }
}
