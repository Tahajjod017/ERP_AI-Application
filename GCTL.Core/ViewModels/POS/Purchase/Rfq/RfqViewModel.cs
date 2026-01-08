using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.POS.Purchase.Rfq
{
    public class RfqViewModel
    {
        public int Id { get; set; }
        public string RfqNumber { get; set; } = "RFQ00001";
        public DateTime RfqDate { get; set; } = DateTime.Now;
        public DateTime OrderDeadline { get; set; } = DateTime.Now.AddDays(7);
        public DateTime ExpectedArrival { get; set; } = DateTime.Now.AddDays(14);
        public string VendorReference { get; set; }
        public List<string> SelectedVendorId { get; set; }
        public string PurchaseAgreement { get; set; }
        public int ConfirmationDays { get; set; } 
        public bool AskConfirmation { get; set; } = true;
        public string Currency { get; set; } = "USD";
        public List<RfqItemViewModel> Items { get; set; } = new();
        public string TermsAndConditions { get; set; }
        public decimal UntaxedAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsDraft { get; set; }
        public string Status { get; set; } = "RFQ"; // RFQ, RFQ_SENT, PURCHASE_ORDER
        public List<AlternativeRfqViewModel> AlternativeRfqs { get; set; } = new();
    }

    public class RfqItemViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal Quantity { get; set; } = 1;
        public string Uom { get; set; } = "Units";
        public decimal UnitPrice { get; set; }
        public decimal TaxRate { get; set; } = 0.15m;
        public decimal Subtotal { get; set; }
        public bool IsSection { get; set; }
        public bool IsNote { get; set; }
        public string SectionTitle { get; set; }
        public string NoteText { get; set; }
    }

    public class AlternativeRfqViewModel
    {
        public int Id { get; set; }
        public string Reference { get; set; }
        public string VendorId { get; set; }
        public string VendorName { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public List<RfqItemViewModel> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Draft";
    }
}
