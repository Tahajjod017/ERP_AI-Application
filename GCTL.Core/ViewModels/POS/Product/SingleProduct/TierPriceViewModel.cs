using System.ComponentModel.DataAnnotations;

namespace GCTL.Core.ViewModels.POS.Product.SingleProduct
{
    public class TierPriceViewModel
    {
        [Display(Name = "Customer Group")]
        //[Required(ErrorMessage = "Customer Group is required")]
        public int? CustomerGroup { get; set; }

        [Display(Name = "Min Quantity")]
        //[Required(ErrorMessage = "Minimum Quantity is required")]
        public int? MinQuantity { get; set; }

        [Display(Name = "Max Quantity")]
        public int? MaxQuantity { get; set; }

        [Display(Name = "Price Type")]
       // [Required(ErrorMessage = "Price Type is required")]
        public int? PriceType { get; set; } // later map to enum

        [Display(Name = "Value")]
        [Required(ErrorMessage = "Price Value is required")]
        public decimal Value { get; set; }


        //[Display(Name = "Customer Group")]
        //public int CustomerGroup { get; set; }

        //[Display(Name = "Min Quantity")]
        //public int MinQuantity { get; set; }

        //[Display(Name = "Max Quantity")]
        //public int? MaxQuantity { get; set; }

        //[Display(Name = "Price Type")]
        //public int PriceType { get; set; } // "Fixed" or "Discount"

        //[Display(Name = "Value")]
        //public decimal Value { get; set; }

    }
}