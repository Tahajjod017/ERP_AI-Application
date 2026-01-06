using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.POS.Purchase.Rfq
{
    public class RfqComparisonViewModel
    {
        public int MainRfqId { get; set; }
        public string MainRfqReference { get; set; }
        public DateTime MainRfqDate { get; set; }
        public List<ComparisonVendorViewModel> Vendors { get; set; } = new();
        public List<ComparisonItemViewModel> Items { get; set; } = new();
        public bool ShowNegotiation { get; set; }
        public decimal MainRfqTotal { get; set; }
        public string Currency { get; set; } = "USD";
    }

    public class ComparisonVendorViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Reference { get; set; } // e.g., "P00022"
        public string DisplayName { get; set; } // e.g., "Gemini Furniture P00022"
        public bool IsBestOffer { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Savings { get; set; } // Compared to main RFQ
        public decimal SavingsPercentage { get; set; }
        public DateTime QuoteDate { get; set; }
        public string Status { get; set; }
    }

    public class ComparisonItemViewModel
    {
        public int Id { get; set; }
        public string ItemCode { get; set; } // e.g., "[E-COM07]"
        public string ProductName { get; set; } // e.g., "Large Cabinet"
        public string Description { get; set; }
        public string Uom { get; set; } = "Units";
        public decimal Quantity { get; set; }

        // Original vendor prices
        public Dictionary<int, decimal> VendorPrices { get; set; } = new(); // vendorId -> unitPrice
        public Dictionary<int, decimal> VendorTotals { get; set; } = new(); // vendorId -> totalPrice

        // Negotiation prices (editable)
        public Dictionary<int, decimal> NegotiationPrices { get; set; } = new(); // vendorId -> negoUnitPrice
        public Dictionary<int, decimal> NegotiationTotals { get; set; } = new(); // vendorId -> negoTotalPrice

        // For display
        public bool IsSection { get; set; }
        public bool IsNote { get; set; }
    }


    public class NegotiationSaveRequest
    {
        public List<NegotiationItem> Items { get; set; } = new();
        public int MainRfqId { get; set; }
    }

    public class NegotiationItem
    {
        public int ItemId { get; set; }
        public Dictionary<int, decimal> VendorPrices { get; set; } = new(); // vendorId -> price
    }

}
