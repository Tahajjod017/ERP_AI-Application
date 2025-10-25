using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GCTL.Core.ViewModels.POS.Product.SingleProduct
{
    /// <summary>
    /// View model for the product add form in `first.html`.
    /// Maps form fields and attribute groups to strongly-typed properties.
    /// </summary>
    public class AttrProductAddViewModel
    {
        // Basic product info
        [Display(Name = "Product Type")]
        [Required(ErrorMessage = "Product type is required.")]
        [StringLength(50)]
        public string AttrProductType { get; set; }

        [Display(Name = "Product Name")]
        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters.")]
        public string AttrProductName { get; set; }

        [Display(Name = "Base SKU")]
        [Required(ErrorMessage = "Base SKU is required.")]
        [StringLength(100, ErrorMessage = "Base SKU cannot exceed 100 characters.")]
        public string AttrBaseSku { get; set; }

        // Categories / brand / model
        [Display(Name = "Category")]
        [StringLength(100)]
        public string AttrCategory { get; set; }

        [Display(Name = "Sub Category")]
        [StringLength(100)]
        public string AttrSubCategory { get; set; }

        [Display(Name = "Brand")]
        [StringLength(100)]
        public string AttrBrand { get; set; }

        [Display(Name = "Model No")]
        [StringLength(100)]
        public string AttrModelNo { get; set; }

        [Display(Name = "Part No")]
        [StringLength(100)]
        public string AttrPartNo { get; set; }

        // Barcode
        [Display(Name = "Barcode Type")]
        [StringLength(50)]
        public string AttrBarcodeType { get; set; }

        [Display(Name = "Barcode Value")]
        [StringLength(200)]
        public string AttrBarcodeValue { get; set; }

        // Unit / assets / quantity
        [Display(Name = "Unit")]
        [StringLength(50)]
        public string AttrUnit { get; set; }

        [Display(Name = "Assets Type")]
        [StringLength(100)]
        public string AttrAssetsType { get; set; }

        [Display(Name = "Quantity Alert")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity alert must be zero or a positive number.")]
        public int? AttrQuantityAlert { get; set; }

        // Flags
        [Display(Name = "Track Batch")]
        public bool AttrTrackBatch { get; set; }

        [Display(Name = "Is Active")]
        public bool AttrIsActive { get; set; }

        [Display(Name = "Trace Manufacturing Date")]
        public bool AttrTraceManufacturingDate { get; set; }

        [Display(Name = "Trace Expiry Date")]
        public bool AttrTraceExpiryDate { get; set; }

        [Display(Name = "Serial Number")]
        public bool AttrSerialNumber { get; set; }

        // Images and description
        // For file uploads from the form (multiple files)
        [Display(Name = "Product Images")]
        [DataType(DataType.Upload)]
        public List<IFormFile> AttrProductImages { get; set; } = new List<IFormFile>();

        [Display(Name = "Description")]
        [DataType(DataType.MultilineText)]
        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        public string AttrDescription { get; set; }

        // Attribute toggles and selected attribute values
        [Display(Name = "Size Enabled")]
        public bool AttrSizeEnabled { get; set; }

        [Display(Name = "Color Enabled")]
        public bool AttrColorEnabled { get; set; }

        [Display(Name = "Materials Enabled")]
        public bool AttrMaterialsEnabled { get; set; }

        // Selected lists for checkboxes (Size, Color, Materials)
        [Display(Name = "Selected Sizes")]
        public List<string> AttrSelectedSizes { get; set; } = new List<string>();

        [Display(Name = "Selected Colors")]
        public List<string> AttrSelectedColors { get; set; } = new List<string>();

        [Display(Name = "Selected Materials")]
        public List<string> AttrSelectedMaterials { get; set; } = new List<string>();

        // Product variants (repeats rendered in the price table)
        [Display(Name = "Product Variants")]
        public List<AttrProductVariantViewModel> AttrProductVariants { get; set; } = new List<AttrProductVariantViewModel>();

        // Custom fields
        [Display(Name = "Warranties Enabled")]
        public bool AttrWarrantiesEnabled { get; set; }

        [Display(Name = "Manufacturer Enabled")]
        public bool AttrManufacturerEnabled { get; set; }

        [Display(Name = "Expiry Enabled")]
        public bool AttrExpiryEnabled { get; set; }

        [Display(Name = "Warranty")]
        [StringLength(200)]
        public string AttrWarranty { get; set; }

        [Display(Name = "Manufacturer Name")]
        [StringLength(200)]
        public string AttrManufacturerName { get; set; }

        [Display(Name = "Manufactured Date")]
        [DataType(DataType.Date)]
        public DateTime? AttrManufacturedDate { get; set; }

        [Display(Name = "Expiry Date")]
        [DataType(DataType.Date)]
        public DateTime? AttrExpiryDate { get; set; }

        // Barcode/SKU template selection
        [Display(Name = "SKU Template")]
        [StringLength(100)]
        public string AttrSkuTemplate { get; set; }

        [Display(Name = "Barcode Template")]
        [StringLength(100)]
        public string AttrBarcodeTemplate { get; set; }
    }

    /// <summary>
    /// Represents a single product variant row shown in the price/variants table.
    /// Fields chosen based on inputs bound with `asp-for` inside the table rows.
    /// </summary>
    public class AttrProductVariantViewModel
    {
        // SKU for the variant
        [Display(Name = "SKU")]
        [Required(ErrorMessage = "Variant SKU is required.")]
        [StringLength(150)]
        public string AttrSku { get; set; }

        // VAT percentage (0-100)
        [Display(Name = "VAT Percentage")]
        [Range(0, 100, ErrorMessage = "VAT must be between 0 and 100 percent.")]
        public decimal? AttrVatPercentage { get; set; }

        // Variant weight
        [Display(Name = "Weight")]
        [Range(0, double.MaxValue, ErrorMessage = "Weight must be zero or a positive number.")]
        public decimal? AttrWeight { get; set; }

        // Attribute description (e.g. "Size: M, Color: Red")
        [Display(Name = "Attribute")]
        [StringLength(500)]
        public string AttrAttribute { get; set; }

        // Additional optional display/price fields if needed
        [Display(Name = "Item Name")]
        [StringLength(250)]
        public string ItemName { get; set; }

        // Backwards-compatible property names used by views/controllers
        // Some views expect AttrItemName, AttrSellingPrice and AttrImageUrl. Provide them
        // mapped here as separate properties to avoid breaking existing code.
        [Display(Name = "Attribute Item Name")]
        [StringLength(250)]
        public string AttrItemName { get; set; }

        [Display(Name = "Item Cost")]
        [DataType(DataType.Currency)]
        [Range(0, double.MaxValue, ErrorMessage = "Cost must be zero or a positive value.")]
        public decimal? ItemCost { get; set; }

        [Display(Name = "Item Price")]
        [DataType(DataType.Currency)]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be zero or a positive value.")]
        public decimal? ItemPrice { get; set; }

        [Display(Name = "Selling Price")]
        [DataType(DataType.Currency)]
        [Range(0, double.MaxValue, ErrorMessage = "Selling price must be zero or a positive value.")]
        public decimal? AttrSellingPrice { get; set; }

        [Display(Name = "Image URL")]
        [DataType(DataType.ImageUrl)]
        [StringLength(1000)]
        public string AttrImageUrl { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }
    }





}
