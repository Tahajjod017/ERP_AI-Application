using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.POS.Purchasess.Purchase.CreatePO
{
    public class PurchaseOrderSubmissionViewModel : BaseViewModel
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
        public BillingInfoViewModel BillingInfo { get; set; }
        public ShippingInfoViewModel ShippingInfo { get; set; }
        public List<PurchaseOrderItemSubmissionViewModel> Items { get; set; }
        public List<string> Attachments { get; set; }


        public decimal TaxAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
        //public decimal? TotalAmount { get; set; }
        public decimal GrandTotalAmount { get; set; }
        public decimal TaxRate { get; set; }
        public decimal? TotalAmount => Items.Sum(i => i.TotalAmount);
        public string? CheckNumber { get; set; }
        public DateOnly? CheckDate { get; set; }
        public int PaymentMethodID { get; set; }
        public int? BankAccountInfoID { get; set; }
        public string? MobileBankingType { get; set; }
        public int? JournalHeadID { get; set; }
    }

    public class BillingInfoViewModel
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string TaxNumber { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }

    public class ShippingInfoViewModel
    {
        public string Address { get; set; }
        public string Contact { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }

    public class PurchaseOrderItemSubmissionViewModel
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
