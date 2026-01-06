using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.POS.Purchase.Rfq
{
    public class RfqListViewModel
    {
        public List<ParentRfqViewModel> ParentRfqs { get; set; } = new();
        public List<int> SelectedSubRfqIds { get; set; } = new();
        public string SearchTerm { get; set; }
        public string StatusFilter { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class ParentRfqViewModel
    {
        public int Id { get; set; }
        public string Reference { get; set; }
        public string VendorName { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } // RFQ, RFQ_SENT, PURCHASE_ORDER, CANCELLED
        public string StatusColor { get; set; }
        public bool IsExpanded { get; set; }
        public bool IsSelected { get; set; }
        public List<SubRfqViewModel> SubRfqs { get; set; } = new();
    }

    public class SubRfqViewModel
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Reference { get; set; }
        public string VendorName { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } // DRAFT, SENT, CONFIRMED, CANCELLED
        public string StatusColor { get; set; }
        public bool IsSelected { get; set; }
        public string Notes { get; set; }
        public bool IsBestOffer { get; set; }
        public decimal Savings { get; set; } // Savings compared to parent RFQ
    }


}
