using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.POS.Product.SingleProduct
{
    public class BGGiftOfferViewModel
    {
        [Required]
        public string BGGiftName { get; set; }

        public List<BGGiftBuyingProductViewModel> BGGiftBuyingProducts { get; set; } = new List<BGGiftBuyingProductViewModel>();

        [Required]
        public DateTime? BGGiftStartDateTime { get; set; }

        [Required]
        public DateTime? BGGiftEndDateTime { get; set; }

        [Required]
        public string BGGiftSelectGetsProduct { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Gift quantity must be at least 1.")]
        public int? BGGiftGiftQuantity { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Maximum time must be at least 1.")]
        public int? BGGiftMaxTimeOffer { get; set; }

        public string BGGiftSearch { get; set; }
        public string BGGiftSort { get; set; }

        public List<BGGiftOfferListViewModel> BGGiftOffers { get; set; } = new List<BGGiftOfferListViewModel>();
    }

    public class BGGiftBuyingProductViewModel
    {
        [Required]
        public string BGGiftProduct { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int? BGGiftQuantity { get; set; }
    }

    public class BGGiftOfferListViewModel
    {
        public string BGGiftComboName { get; set; }
        public List<BGGiftProductDetailViewModel> BGGiftProductListDetails { get; set; } = new List<BGGiftProductDetailViewModel>();
        public string BGGiftStartDate { get; set; }
        public string BGGiftEndDate { get; set; }
        public string BGGiftGiftProduct { get; set; }
        public int BGGiftQuantity { get; set; }
        public int BGGiftMaximumTime { get; set; }
        public string BGGiftCreatedDate { get; set; }
        public string BGGiftCreatedTime { get; set; }
    }

    public class BGGiftProductDetailViewModel
    {
        public string BGGiftProductName { get; set; }
        public int BGGiftProductQuantity { get; set; }
    }
}
