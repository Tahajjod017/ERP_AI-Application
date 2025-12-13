using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.POS.Purchase.PurchaseOrder
{
    public class PurchaseOrderViewModel : BaseViewModel
    {
        // ----- Header -------------------------------------------------
        public int? Id { get; set; }
        public string? POID { get; set; } = string.Empty;
        public DateTime? PurchaseDate { get; set; } = DateTime.Today;
        public DateTime? DueDate { get; set; } = DateTime.Today.AddDays(30);
        public string? OtherReference { get; set; } = string.Empty;
        public string? WorkorderNo { get; set; } = string.Empty;
        public DateTime? WorkOrderDate { get; set; }

        // ----- Supplier ------------------------------------------------
        public int? SelectedSupplierId { get; set; }
        public List<SupplierDto> Suppliers { get; set; } = new();

        // ----- Addresses -----------------------------------------------
        public int? BillingAddressId { get; set; }
        public int? ShippingAddressId { get; set; }

        // ----- Organization --------------------------------------------
        public int? OrganizationId { get; set; }
        public int? OrganizationBranchId { get; set; }

        // ----- Payment -------------------------------------------------
        public int? PaymentMethodId { get; set; }
        public int? BankAccountInfoId { get; set; }
        public string? CheckNumber { get; set; }
        public DateOnly? CheckDate { get; set; }

        // ----- Line items ----------------------------------------------
        public List<PurchaseOrderItem> Items { get; set; } = new() { new PurchaseOrderItem() };

        // ----- Totals --------------------------------------------------
        public decimal SubTotal => Items.Sum(i => i.Amount);
        public decimal? TaxPercent { get; set; } = 0m;
        public decimal? TaxAmount => SubTotal * (TaxPercent / 100m);
        public decimal? GrandTotal => SubTotal + TaxAmount;
        public decimal? PaidAmount { get; set; } = 0m;
        public decimal? DueAmount => GrandTotal - PaidAmount;

        // ----- Additional ----------------------------------------------
        public string? Note { get; set; } = string.Empty;
        public string? TermsAndConditions { get; set; } = string.Empty;
        public string? AttachmentLink { get; set; } = string.Empty;
        public int? StatusId { get; set; }
        public bool IsDraft { get; set; }
    }

}
