using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Enums;
using GCTL.Core.ViewModels.POS.Purchase.PurchaseOrder;

namespace GCTL.Core.ViewModels.POS.Purchase.PurchaseOrderDetails
{
    public class PurchaseOrderDetailsViewModel
    {
        public int Id { get; set; }
        public string? POID { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string? OtherReference { get; set; }
        public string? WorkorderNo { get; set; }
        public DateTime? WorkOrderDate { get; set; }

        public int? SelectedSupplierId { get; set; }
        public List<SupplierDetailsViewModel> Suppliers { get; set; } = new();
        public SupplierDetailsViewModel SupplierData { get; set; } = new();

        public AddressDetailsViewModel BillingAddress { get; set; } = new();
        public AddressDetailsViewModel ShippingAddress { get; set; } = new();
        public List<AddressDetailsViewModel> Addresses { get; set; } = new();
        public int? BillingAddressId { get; set; }
        public int? ShippingAddressId { get; set; }
        public int? SelectedShippingAddressId { get; set; }
        public List<ShippingAddressDto> ShippingAddresses { get; set; } = new();
        public int? OrganizationId { get; set; }
        public int? OrganizationBranchId { get; set; }

        public int? PaymentMethodId { get; set; }
        public int? BankAccountInfoId { get; set; }
        public string? CheckNumber { get; set; }
        public DateOnly? CheckDate { get; set; }

        public List<PurchaseOrderItemDetails> Items { get; set; } = new();

        public decimal SubTotal { get; set; }
        public decimal? TaxPercent { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? GrandTotal { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? DueAmount { get; set; }

        public string? Note { get; set; }
        public string? TermsAndConditions { get; set; }
        public string? AttachmentLink { get; set; }

        // Sidebar data
        public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
        public string? CreatedByName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? UpdatedByName { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Related documents
        public int? ConvertedToReceiveId { get; set; }
        public string? ReceiveNumber { get; set; }
        public bool HasReceive => ConvertedToReceiveId.HasValue;
    }

    public class PurchaseOrderItemDetails
    {
        public int SL { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount => Quantity * UnitPrice;
    }

    public class SupplierDetailsViewModel
    {
        public int Id { get; set; }
        public string? CompanyName { get; set; }
        public string? ContactName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? TaxNumber { get; set; }
    }

    // Sidebar specific view model
    public class PurchaseOrderSidebarDetailsViewModel
    {
        public int PurchaseOrderId { get; set; }
        public string PurchaseOrderNumber { get; set; }
        public PurchaseOrderStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public string CreatedByName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string UpdatedByName { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Permissions/Actions
        public bool CanEdit => Status == PurchaseOrderStatus.Draft;
        public bool CanDuplicate => true;
        public bool CanConvertToReceive => Status == PurchaseOrderStatus.Approved;
        public bool CanSendEmail => Status != PurchaseOrderStatus.Converted;
        public bool CanDelete => Status == PurchaseOrderStatus.Draft;

        // Related documents
        public int? ReceiveId { get; set; }
        public string ReceiveNumber { get; set; }
        public bool HasReceive => ReceiveId.HasValue;

        public List<PurchaseOrderVersionViewModel> PurchaseOrderIdList { get; set; }
    }

    public class PurchaseOrderVersionViewModel
    {
        public bool? draft;
        public string draftSign;
        public string finalSign;
        public string current;

        public int id { get; set; }
        public string? number { get; set; }
        public int version { get; set; }
    }

}
