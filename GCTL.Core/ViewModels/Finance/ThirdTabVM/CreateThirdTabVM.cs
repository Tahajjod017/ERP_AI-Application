using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Finance.ThirdTabVM
{
    public class CreateThirdTabVM : BaseViewModel
    {
        public int GroupID { get; set; }

        [Required(ErrorMessage = "{0} is required!"), Display(Name = "Class Name")]
        public int? ClassID { get; set; }

        [Required(ErrorMessage = "{0} is required!"), StringLength(2, MinimumLength = 2, ErrorMessage = "{0} must me at least {1} characters long!"), Display(Name = "Group Code")]
        public string GroupCode { get; set; }

        [Required(ErrorMessage = "{0} is required!"), Display(Name = "Group Name")]
        public string GroupName { get; set; }

        public string? Description { get; set; }
    }
}
