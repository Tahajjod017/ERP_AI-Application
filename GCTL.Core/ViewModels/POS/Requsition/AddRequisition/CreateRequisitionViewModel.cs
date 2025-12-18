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
        [Display(Name = "Project Manager")]
        public int SupervisorId { get; set; }



        [Required]
        [Display(Name = "Project Name")]
        public int ProjectId { get; set; }

        public List<RequisitionProductViewModel> Products { get; set; } = new List<RequisitionProductViewModel>
        {
            new RequisitionProductViewModel() // Add one empty product by default
        };
    }
}
