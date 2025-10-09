using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Finance.AddMainAccountVM
{
    public class CreateAddMainAccountVM : BaseViewModel
    {
        public int MainAccountID { get; set; }

        [Required(ErrorMessage = "Select a {0}!"), Display(Name = "Class Name")]
        public int? ClassID { get; set; }

        [Required(ErrorMessage = "{0} is required!"), StringLength(4, MinimumLength = 4, ErrorMessage = "{0} must be at most {1} characters long."), Display(Name = "Main Account Code")]
        public string MainAccountCode { get; set; }

        [Required(ErrorMessage = "{0} is required!"), StringLength(100, ErrorMessage = "{0} must be at most {1} characters long."), Display(Name = "Main Account Name")]
        public string MainAccountName { get; set; }

        [StringLength(200, ErrorMessage = "{0} must be at most {1} characters long."), Display(Name = "Description")]
        public string? Description { get; set; }
    }
}
