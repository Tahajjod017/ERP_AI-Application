using System.ComponentModel.DataAnnotations;

namespace GCTL.Core.ViewModels.POS.Requsition.AddRequisition
{
    public class RequisitionProductViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Product Type")]
        public int ProductTypeId { get; set; }

        [Required]
        [Display(Name = "Product Name")]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }
    }
}