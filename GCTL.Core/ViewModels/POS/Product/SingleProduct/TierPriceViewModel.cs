using System.ComponentModel.DataAnnotations;

namespace GCTL.Core.ViewModels.POS.Product.SingleProduct
{
    public class TierPriceViewModel
    {

        [Display(Name = "Customer Group")]
        public string CustomerGroup { get; set; }

        [Display(Name = "Min Quantity")]
        public int MinQuantity { get; set; }

        [Display(Name = "Max Quantity")]
        public int? MaxQuantity { get; set; }

        [Display(Name = "Price Type")]
        public string PriceType { get; set; } // "Fixed" or "Discount"

        [Display(Name = "Value")]
        public decimal Value { get; set; }

    }
}