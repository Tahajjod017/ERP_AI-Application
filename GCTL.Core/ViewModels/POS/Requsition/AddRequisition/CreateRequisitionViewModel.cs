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
        [Display(Name = "Request Location")]
        public int LocationId { get; set; }

        public List<RequisitionProductViewModel> Products { get; set; } = new List<RequisitionProductViewModel>
        {
            new RequisitionProductViewModel() // Add one empty product by default
        };
    }
}
