using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.RequisitionDistribution.Purchase.CreatePO
{
    public class PurchaseOrderManualSubmissionViewModel : BaseViewModel
    {
        public int PurchaseOderId { get; set; }
        public int SupperId { get; set; }
        public int ToLocationId { get; set; }
        public string PoDate { get; set; }
        public string PoNumber { get; set; }
        public string DueDate { get; set; }
        public string WorkOrderDate { get; set; }
        public string OtherReference { get; set; }
        public string WorkOrderNo { get; set; }
        public string Note { get; set; }
        public string Terms { get; set; }
        public decimal TaxRate { get; set; }
        public BillingManualInfoViewModel BillingInfo { get; set; }
        public ShippingManualInfoViewModel ShippingInfo { get; set; }
        public List<PurchaseOrderManualItemSubmissionViewModel> Items { get; set; }
        public List<string> Attachments { get; set; }
    }

    public class BillingManualInfoViewModel
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string TaxNumber { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }

    public class ShippingManualInfoViewModel
    {
        public string Address { get; set; }
        public string Contact { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }

    public class PurchaseOrderManualItemSubmissionViewModel
    {
        public int SerialNumber { get; set; }
        public int PurchasOrderItemID { get; set; }
        public string ItemName { get; set; }
        public string Unit { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
    }

}
