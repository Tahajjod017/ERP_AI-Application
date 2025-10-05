using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Finance.AddSubAccountVM
{
    public class CreateAddSubAccountVM : BaseViewModel
    {
        public int SubAccountID { get; set; }

        [Required(ErrorMessage = "Select a {0}!"), Display(Name = "Class Name")]
        public int? ClassID { get; set; }

        [Required(ErrorMessage = "Select a {0}!"), Display(Name = "Group Name")]
        public int? GroupID { get; set; }

        [Required(ErrorMessage = "{0} is required!"), Display(Name = "Main Account")]
        public int? MainAccountID { get; set; }

        [Required(ErrorMessage = "{0} is required!"), StringLength(8, ErrorMessage = "{0} must be at most {1} characters long."), Display(Name = "Sub Account Code")]
        [RegularExpression(@"^.{8}$", ErrorMessage = "{0} must be exactly 8 characters long.")]
        public string SubAccountCode { get; set; }

        [Required(ErrorMessage = "{0} is required!"), StringLength(100, ErrorMessage = "{0} must be at most {1} characters long."), Display(Name = "Sub Account Name")]
        public string SubAccountName { get; set; }

        [StringLength(200, ErrorMessage = "{0} must be at most {1} characters long."), Display(Name = "Description")]
        public string? Description { get; set; }
    }
}
