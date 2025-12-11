using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.POS.Purchasess.Purchase.CreatePO
{
    public class PurchaseOrderViewModel
    {
        public List<PurchaseItemViewModel> Items { get; set; } = new();
        public string POID { get; set; }
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

        public int PurchaseOrderID { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime DueDate { get; set; }
        public int? Tolocation { get; set; }
    }
}
