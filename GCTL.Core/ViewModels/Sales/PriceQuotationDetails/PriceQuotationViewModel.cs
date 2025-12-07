using GCTL.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Sales.PriceQuotationDetails
{
    public class PriceQuotationDetailsViewModel
    {
        public int Id { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string? InvoiceNumber { get; set; }
        public string? OtherNumber { get; set; }
        public int? SelectedCustomerId { get; set; }
        public List<QuotationItemDetails> Items { get; set; } = new List<QuotationItemDetails>();
        public List<CustomerDetailsViewModel> Customers { get; set; } = new List<CustomerDetailsViewModel>();
        public CustomerDetailsViewModel CustomerData { get; set; } = new CustomerDetailsViewModel();
        public decimal SubTotal { get; set; }
        public decimal? RetentionAmount { get; set; }
        public decimal? GrandTotal { get; set; }
        public decimal RetentionPercent { get; set; }
        public string? Note { get; set; }

        // New properties for sidebar
        public QuotationStatus Status { get; set; } = QuotationStatus.Draft;
        public string? CreatedByName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? UpdatedByName { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Related documents
        public int? ConvertedToWorkOrderId { get; set; }
        public string? WorkOrderNumber { get; set; }
        public bool HasWorkOrder => ConvertedToWorkOrderId.HasValue;

        // For future use when you add Version column
        public string? Version { get; set; }
    }

    public class QuotationItemDetails
    {
        public int SL { get; set; }
        public string? Description { get; set; }
        public int? Unit { get; set; }
        public string? UnitName { get; set; }
        public decimal Area { get; set; }
        public decimal Rate { get; set; }
        public decimal PercentInBill { get; set; }
        public decimal AmountPerPercent => Area * Rate * (PercentInBill / 100);
    }

    public class CustomerDetailsViewModel
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
    public class QuotationSidebarDetailsViewModel
    {
        public int QuotationId { get; set; }
        public string QuotationNumber { get; set; }
        public QuotationStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public string CreatedByName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string UpdatedByName { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Permissions/Actions
        public bool CanEdit => Status == QuotationStatus.Draft;
        public bool CanDuplicate => true;
        public bool CanConvertToWorkOrder => Status == QuotationStatus.Approved;
        public bool CanSendEmail => Status != QuotationStatus.Converted;
        public bool CanDelete => Status == QuotationStatus.Draft;

        // Related documents
        public int? WorkOrderId { get; set; }
        public string WorkOrderNumber { get; set; }
        public bool HasWorkOrder => WorkOrderId.HasValue;

        public List<PriceQuotationVersionViewModel> QuotationIdList { get; set; }
    }

    public class PriceQuotationVersionViewModel
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
