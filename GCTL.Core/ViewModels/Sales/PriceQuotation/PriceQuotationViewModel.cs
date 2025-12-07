using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Sales.PriceQuotation
{
    public class PriceQuotationViewModel : BaseViewModel
    {
        // ----- Header -------------------------------------------------
        public int? Id { get; set; }
        public DateTime? InvoiceDate { get; set; } = DateTime.Today;
        public string? InvoiceNumber { get; set; } = string.Empty;
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(30);
        public string? OtherNumber { get; set; } = string.Empty;

        // ----- Customer ------------------------------------------------
        public int? SelectedCustomerId { get; set; }
        public List<CustomerDto> Customers { get; set; } = new();

        // ----- Line items ----------------------------------------------
        public List<QuotationItem> Items { get; set; } = new() { new QuotationItem() };

        // ----- Totals --------------------------------------------------
        public decimal SubTotal => Items.Sum(i => i.Amount);
        public decimal? RetentionPercent { get; set; } = 5m;
        public decimal? RetentionAmount => SubTotal * (RetentionPercent / 100m);
        public decimal? GrandTotal => SubTotal - RetentionAmount;

        // ----- Note ----------------------------------------------------
        public string? Note { get; set; } = string.Empty;

        public bool IsDraft { get; set; }
    }

    public class CustomerDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string AddressLine2 { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
    }

    public class QuotationItem
    {
        public int SL { get; set; }                     // auto-filled on UI
        public string? Description { get; set; } = string.Empty;
        public int? Unit { get; set; } 
        public decimal? Area { get; set; }
        public decimal? Rate { get; set; }
        public decimal Amount => (Area ?? 0) * (Rate ?? 0);
        public decimal? PercentInBill { get; set; }      // optional
        public decimal AmountPerPercent => Amount * (PercentInBill ?? 100m) / 100m;
    }
}
