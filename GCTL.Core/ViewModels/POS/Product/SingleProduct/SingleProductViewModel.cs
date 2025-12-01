using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GCTL.Core.ViewModels.POS.Product.SingleProduct
{
    public class SingleProductViewModel : BaseViewModel
    {


        [Required(ErrorMessage = "Product Name is required")]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "SKU is required")]
        [Display(Name = "SKU")]
        public string SKU { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public string Category { get; set; }


       


        [Required]
        [Display(Name = "Product Type")]
        public string ProductType { get; set; } = "sp";

        //[Required]
        //[Display(Name = "Product Name")]
        //public string ProductName { get; set; }

        //[Required]
        //[Display(Name = "SKU")]
        //public string SKU { get; set; }

        //[Required]
        //[Display(Name = "Category")]
        //public string Category { get; set; }

        [Required]
        [Display(Name = "Sub Category")]
        public string SubCategory { get; set; }

        [Required]
        [Display(Name = "Brand")]
        public string Brand { get; set; }

        [Required]
        [Display(Name = "Model No")]
        public string ModelNo { get; set; }

        [Required]
        [Display(Name = "Part No")]
        public string PartNo { get; set; }

        [Display(Name = "Barcode Type")]
        public string BarcodeType { get; set; }

        [Display(Name = "Barcode")]
        public string Barcode { get; set; }

        [Required]
        [Display(Name = "Weight")]
        public string Weight { get; set; }

        [Required]
        [Display(Name = "Unit")]
        public string Unit { get; set; }

        [Required]
        [Display(Name = "Assets Type")]
        public string AssetsType { get; set; }

        [Required]
        [Display(Name = "Quantity Alert")]
        public int QuantityAlert { get; set; }

        [Display(Name = "Track Batch")]
        public bool TrackBatch { get; set; } = true;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Trace Manufacturing Date")]
        public bool TraceManufacturingDate { get; set; } = true;

        [Display(Name = "Trace Expiry Date")]
        public bool TraceExpiryDate { get; set; } = true;

        [Display(Name = "Serial Number")]
        public bool SerialNumber { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        // Pricing
        [Required]
        [Display(Name = "Selling Price (Excluding VAT)")]
        public decimal SellingPrice { get; set; }

        [Required]
        [Display(Name = "VAT %")]
        public string VATPercentage { get; set; }



       

        // Barcode
        //[Display(Name = "Barcode SKU")]
        //public string? BarcodeSKU { get; set; }

        //[Display(Name = "Barcode Template")]
        //public string BarcodeTemplate { get; set; } = "Standard";

        // Custom Fields
        [Display(Name = "Has Warranties")]
        public bool HasWarranties { get; set; }

        [Display(Name = "Has Manufacturer")]
        public bool HasManufacturer { get; set; }

        [Display(Name = "Has Expiry")]
        public bool HasExpiry { get; set; }

        [Display(Name = "Warranty")]
        public int Warranty { get; set; }

        [Display(Name = "Manufacturer")]
        public string Manufacturer { get; set; }

        [Display(Name = "Manufactured Date")]
        public DateOnly? ManufacturedDate { get; set; }

        [Display(Name = "Expiry Date")]
        public DateOnly? ExpiryDate { get; set; }

        // Images
        public List<IFormFile> ProductImages { get; set; } = new List<IFormFile>();


        [Display(Name = "Special Price")]
        public decimal? SpecialPrice { get; set; }

        [Display(Name = "Customer Group")]
        public int? CustomerGroup { get; set; }

        [Display(Name = "Special Price Start Date")]
        public DateTime? SpecialPriceStartDate { get; set; }

        [Display(Name = "Special Price End Date")]
        public DateTime? SpecialPriceEndDate { get; set; }

        // Tier Pricing
        public List<TierPriceViewModel> TierPrices { get; set; } = new List<TierPriceViewModel>();


    }
}
