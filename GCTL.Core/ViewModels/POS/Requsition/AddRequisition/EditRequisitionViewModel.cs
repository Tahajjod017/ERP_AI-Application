using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.POS.Requsition.AddRequisition
{
    public class EditRequisitionViewModel : BaseViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Project Name")]
        public int? RequisitionFor { get; set; }

        [Display(Name = "Project Supervisor")]
        public int? RequisitionBy { get; set; }

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        [Display(Name = "Sub Category")]
        public int? SubCategoryId { get; set; }

        [Display(Name = "Product Type")]
        public int? ProductTypeId { get; set; }

        [Required]
        [Display(Name = "Product Name")]
        public int? ProductId { get; set; }

        [Display(Name = "Brand")]
        public string? Brand { get; set; }

        [Display(Name = "Model")]
        public string? Model { get; set; }

        [Required]
        [Range(0.1, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        [Display(Name = "Product Quantity")]
        public decimal? Quantity { get; set; }

        [Display(Name = "Unit")]
        public string? UnitId { get; set; }

        public string? Unit { get; set; }

        public string? Status { get; set; }
    }
}
