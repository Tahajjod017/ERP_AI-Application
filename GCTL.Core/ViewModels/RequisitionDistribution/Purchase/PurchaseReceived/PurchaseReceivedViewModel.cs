using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.RequisitionDistribution.Purchase.CreatePO;

namespace GCTL.Core.ViewModels.RequisitionDistribution.Purchase.PurchaseReceived
{
    public class PurchaseReceivedViewModel : BaseViewModel
    {
    
        public List<PurchaseRecItemViewModel> Items { get; set; } = new();
        public string POID { get; set; }
        public string VendorBillNO { get; set; }
        public string PRID { get; set; }
        public string Note { get; set; }
        public string TermNcondition { get; set; }
       
        public decimal? TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
        //public decimal? TotalAmount => Items.Sum(i => i.TotalAmount);
        public decimal? TotalAmount { get; set; }
        public decimal GrandTotalAmount { get; set; }

        public int PurchaseOrderID { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime BillDate { get; set; }
        public DateTime PRDate { get; set; }
        public int? Tolocation { get; set; }
        public int? VendorId { get; set; }
        public string driverMobileNo { get; set; }
        public string driverName { get; set; }
        public string truckNo { get; set; }
        public string deliveryOrderName { get; set; }
        //public string paymentType { get; set; }
        //public int PaymentTypeID { get; set; }
        public string? CheckNumber { get; set; }
        public DateOnly? CheckDate { get; set; }
        //public int? Accounts { get; set; }
        public int PaymentMethodID { get; set; }
        public int? BankAccountInfoID { get; set; }
    }
}
