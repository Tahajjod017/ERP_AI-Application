using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.POS.Product.SingleProduct
{
    public class BGFreeOfferViewModel
    {
        [Required]
        public string BGFreeSelectProductBuys { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Buying quantity must be at least 1.")]
        public int? BGFreeBuyingQuantity { get; set; }
        [Required]
        public DateTime? BGFreeStartDateTime { get; set; }
        [Required]
        public DateTime? BGFreeEndDateTime { get; set; }
        [Required]
        public string BGFreeSelectProductGets { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Get quantity must be at least 1.")]
        public int? BGFreeGetQuantity { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Maximum time must be at least 1.")]
        public int? BGFreeMaxTimeOffer { get; set; }
    }
}
