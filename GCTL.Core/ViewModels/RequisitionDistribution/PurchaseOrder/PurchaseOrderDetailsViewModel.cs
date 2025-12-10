using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.RequisitionDistribution.PurchaseOrder
{
    public class PurchaseOrderDetailsViewModel : BaseViewModel
    {
        public int? Id { get; set; }
        public string POID { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime DueDate { get; set; }
        public int? SupplierID { get; set; }
        public string SupplierName { get; set; }
        public int? StatusID { get; set; }
        public string StatusName { get; set; }
        public int? ToLocation { get; set; }
        public string LocationName { get; set; }
        public int? PaymentMethodID { get; set; }
        public string PaymentMethodName { get; set; }
        public string WorkorderNo { get; set; }
        public DateTime? WorkOrderDate { get; set; }
        public string OtherReference { get; set; }
        public string CheckNumber { get; set; }
        public DateOnly? CheckDate { get; set; }
        public bool IsDraft { get; set; }

        public decimal TaxPercent { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }

        public string Note { get; set; }
        public string TermsAndConditions { get; set; }
        public string AttachmentLink { get; set; }

        public List<PurchaseOrderItemDetails> Items { get; set; } = new List<PurchaseOrderItemDetails>();

        // Addresses
        public AddressViewModel BillingAddress { get; set; }
        public AddressViewModel ShippingAddress { get; set; }

        // Sidebar data
        public string CreatedByName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string UpdatedByName { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Supplier data
        public SupplierDetailsViewModel SupplierData { get; set; }
    }

    public class PurchaseOrderItemDetails
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

    public class SupplierDetailsViewModel
    {
        public string SupplierName { get; set; }
        public string ContactName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
    }

    public class PurchaseOrderSidebarDetailsViewModel
    {
        public int? PurchaseOrderId { get; set; }
        public string POID { get; set; }
        public string StatusName { get; set; }
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
    }
}
