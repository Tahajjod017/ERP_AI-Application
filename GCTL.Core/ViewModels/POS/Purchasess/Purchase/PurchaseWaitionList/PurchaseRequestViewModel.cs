using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.POS.Purchasess.Purchase.PurchaseWaitionList
{
    public class PurchaseRequestViewModel
    {
        public int PurchaseId { get; set; }
        public string PoId { get; set; }
        public string ReqId { get; set; }

        // Project Info
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }

        // Request Info
        public string RequestedBy { get; set; }
        public DateTime? RequestDate { get; set; }
        public string RequestDateFormatted => RequestDate?.ToString("MMM dd, yyyy");

        // Status / Priority
        public string Status { get; set; }
        public string Priority { get; set; }

        // Collection of items
        public List<PurchaseRequestItemViewModel> Items { get; set; } = new();
    }

    public class PurchaseRequestItemViewModel
    {
        public int? ProductId { get; set; }
        public string ProductName { get; set; }

        public int ProductTypeId { get; set; }
        public string ProductType { get; set; }

        public int Quantity { get; set; }
        public decimal EstimatedCost { get; set; }

        public decimal LineTotal => Quantity * EstimatedCost;
    }

}
