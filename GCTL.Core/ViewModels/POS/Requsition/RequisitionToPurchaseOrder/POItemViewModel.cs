using System.ComponentModel.DataAnnotations;

namespace GCTL.Core.ViewModels.POS.Requsition.RequisitionToPurchaseOrder
{
    public class POItemViewModel
    {
        [Required]
        public int RequisitionItemId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0")]
        public decimal UnitPrice { get; set; }
    }
}