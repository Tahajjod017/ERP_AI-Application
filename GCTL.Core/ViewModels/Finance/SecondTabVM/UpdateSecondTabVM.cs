using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Finance.SecondTabVM
{
    public class UpdateSecondTabVM : BaseViewModel
    {
        public int ClassID { get; set; }

        [Required(ErrorMessage = "{0} is required!"), StringLength(1, ErrorMessage = "{0} must be at most {1} characters long!"), Display(Name = "Class Code")]
        [AllowedValues("0", "1", "2", "3", "4", "5", "6", "7", "8", "9", ErrorMessage = "{0} is valid only for 0-9!")]
        public string ClassCode { get; set; }

        [Required(ErrorMessage = "{0} is required!"), Display(Name = "Class Name")]
        public string ClassName { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "{0} is required!"), Display(Name = "Base Account")]
        public int BaseAccountID { get; set; }
    }
}
