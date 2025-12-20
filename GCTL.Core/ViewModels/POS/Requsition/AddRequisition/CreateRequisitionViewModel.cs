using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.POS.Requsition.AddRequisition
{
    public class CreateRequisitionViewModel : BaseViewModel
    {
        [Required]
        [Display(Name = "Request By")]
        public int RequesterId { get; set; }



    

        [Required]
        [Display(Name = "Organization")]
        public int OrganizationId { get; set; }

        [Required]
        [Display(Name = "Branch")]
        public int OrganizationBranchId { get; set; }

        [Required]
        [Display(Name = "Priority")]
        public int Priority { get; set; }


        [StringLength(100)]
        [Display(Name = "Requisition Note")]
        public string? RequisitionNote { get; set; }

        public List<RequisitionProductViewModel> Products { get; set; } = new List<RequisitionProductViewModel>
        {
            new RequisitionProductViewModel() // Add one empty product by default
        };
    }
}
