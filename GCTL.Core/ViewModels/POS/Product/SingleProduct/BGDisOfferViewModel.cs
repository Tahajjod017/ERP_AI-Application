using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.POS.Product.SingleProduct
{
    public class BGDisOfferViewModel
    {
        [Required]
        public string BGDisSelectProductBuys { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Buying quantity must be at least 1.")]
        public int? BGDisBuyingQuantity { get; set; }
        [Required]
        public DateTime? BGDisStartDateTime { get; set; }
        [Required]
        public DateTime? BGDisEndDateTime { get; set; }
        [Required]
        public string BGDisSelectProductGets { get; set; }
        [Required]
        public string BGDisDiscountPercentage { get; set; }  // String to allow "%" in input; change to decimal if needed
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Maximum time must be at least 1.")]
        public int? BGDisMaxTimeOffer { get; set; }
    }
}
