using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GCTL.Core.ViewModels.POS.Product.SingleProduct
{
    
    public class AttrProductAddViewModel : BaseViewModel
    {
        public bool AttrWarrantiesEnabled { get; set; } = false;
        public bool AttrManufacturerEnabled { get; set; } = false;
        public bool AttrExpiryEnabled { get; set; } = false;

        // ---------- Basic product info ----------
        [Display(Name = "Product Type")]
        [Required(ErrorMessage = "Product type is required.")]
        [StringLength(50)]
        public string AttrProductType { get; set; }

        [Display(Name = "Product Name")]
        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(200)]
        public string AttrProductName { get; set; }

        [Display(Name = "Base SKU")]
        [Required(ErrorMessage = "Base SKU is required.")]
        [StringLength(100)]
        public string AttrBaseSku { get; set; }

        // ---------- Categories / brand / model ----------
        [Display(Name = "Category")] public string AttrCategory { get; set; }
        [Display(Name = "Sub Category")] public string AttrSubCategory { get; set; }
        [Display(Name = "Brand")] public string AttrBrand { get; set; }
        [Display(Name = "Model No")] public string AttrModelNo { get; set; }
        [Display(Name = "Part No")] public string AttrPartNo { get; set; }

        // ---------- Barcode ----------
        [Display(Name = "Barcode Type")] public string AttrBarcodeType { get; set; }
        [Display(Name = "Barcode Value")] public string AttrBarcodeValue { get; set; }

        // ---------- Product / assets / quantity ----------
        [Display(Name = "Product")] public string AttrUnit { get; set; }
        [Display(Name = "Assets Type")] public string AttrAssetsType { get; set; }
        [Display(Name = "Quantity Alert")]
        [Range(0, int.MaxValue)]
        public int? AttrQuantityAlert { get; set; }

        // ---------- Flags ----------
        public bool AttrTrackBatch { get; set; } = false;
        public bool AttrIsActive { get; set; } = true;
        public bool AttrTraceManufacturingDate { get; set; } = false;
        public bool AttrTraceExpiryDate { get; set; } = false;
        public bool AttrSerialNumber { get; set; } = false;

        // ---------- Images (client-side) ----------
        // The actual files are sent as FormData; we keep the names here for binding if needed
      //  public List<string> AttrUploadedImageNames { get; set; } = new List<string>();

        // ---------- Description ----------
        [Display(Name = "Description")]
        [StringLength(2000)]
        public string? AttrDescription { get; set; }

        // ---------- Attribute Values (dynamic) ----------
        // Key = AttributeName (e.g. "Size"), Value = list of selected values
        // public Dictionary<string, List<string>> AttrSelectedValues { get; set; } = new();

        // ---------- Custom fields ----------
        //public bool AttrWarrantiesEnabled { get; set; } = false;
        //public bool AttrManufacturerEnabled { get; set; } = false;
        //public bool AttrExpiryEnabled { get; set; } = false;

        [Display(Name = "Warranty")] 
        public int? AttrWarranty { get; set; }

        [Display(Name = "Manufacturer Name")] 
        public string? AttrManufacturerName { get; set; }

        [Display(Name = "Manufactured Date")]
        [DataType(DataType.Date)]
        public DateOnly? AttrManufacturedDate { get; set; }

        [Display(Name = "Expiry Date")]
        [DataType(DataType.Date)]
        public DateOnly? AttrExpiryDate { get; set; }


        public List<string> AttrUploadedImageNames { get; set; } = new List<string>();
        //public Dictionary<string, List<string>> AttrSelectedValues { get; set; } = new();
        public Dictionary<string, List<AttributeValueDto>> AttrSelectedValues { get; set; }    = new();

        // Files will be sent separately
        public List<IFormFile>? AttrProductImages { get; set; }




       
    }

    public class AttributeValueDto
    {
        public int AtdId { get; set; }      // matches "atdId"
        public string Name { get; set; }    // matches "name"
    }

   
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
