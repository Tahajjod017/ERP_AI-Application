using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.POS.Product.SingleProduct
{
    public class ComboOfferViewModel
    {
        [Required]
        public string ComboComboName { get; set; }

        public List<ComboProductItemViewModel> ComboProductItems { get; set; } = new List<ComboProductItemViewModel>();

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total product price must be greater than 0.")]
        public decimal? ComboTotalProductPrice { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Bundle selling price must be greater than 0.")]
        public decimal? ComboBundleSellingPrice { get; set; }

        [Required]
        public DateTime? ComboStartDateTime { get; set; }

        [Required]
        public DateTime? ComboEndDateTime { get; set; }

        public bool ComboIncludeGift { get; set; }
        public string ComboGiftProduct { get; set; }

        public string ComboSearch { get; set; }
        public string ComboSort { get; set; }

        public List<ComboOfferListViewModel> ComboOffers { get; set; } = new List<ComboOfferListViewModel>();
    }

    public class ComboProductItemViewModel
    {
        [Required]
        public string ComboProduct { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int? ComboQuantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Product price must be greater than 0.")]
        public decimal? ComboUnitPrice { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total price must be greater than 0.")]
        public decimal? ComboTotalPrice { get; set; }
    }

    public class ComboOfferListViewModel
    {
        public string ComboComboName { get; set; }
        public List<ComboProductDetailViewModel> ComboProductListDetails { get; set; } = new List<ComboProductDetailViewModel>();
        public string ComboStartDate { get; set; }
        public string ComboEndDate { get; set; }
        public decimal ComboBundlePrice { get; set; }
        public decimal ComboSellingPrice { get; set; }
        public int ComboMaximumTime { get; set; }
        public string ComboCreatedDate { get; set; }
        public string ComboCreatedTime { get; set; }
    }

    public class ComboProductDetailViewModel
    {
        public string ComboProductName { get; set; }
        public int ComboProductQuantity { get; set; }
    }
}
