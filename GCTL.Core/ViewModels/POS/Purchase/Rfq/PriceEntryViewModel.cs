using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.POS.Purchase.Rfq
{
    public class PriceEntryViewModel
    {
        public List<ItemViewModel> Items { get; set; } = new List<ItemViewModel>();
        public List<VendorViewModel> Vendors { get; set; } = new List<VendorViewModel>();
        public string CurrentDate { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
    }

    public class ItemViewModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
    }

    public class VendorViewModel
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public List<decimal> Prices { get; set; } = new List<decimal>();
        public bool VatIncluded { get; set; } = true;
        public decimal SubTotal { get; set; }
        public decimal VatAmount { get; set; }
        public decimal Total { get; set; }
    }

    public class SavePricesRequest
    {
        public List<VendorPriceUpdate> VendorPrices { get; set; } = new List<VendorPriceUpdate>();
    }

    public class VendorPriceUpdate
    {
        public string VendorCode { get; set; }
        public List<decimal> Prices { get; set; } = new List<decimal>();
        public bool VatIncluded { get; set; }
    }

}
