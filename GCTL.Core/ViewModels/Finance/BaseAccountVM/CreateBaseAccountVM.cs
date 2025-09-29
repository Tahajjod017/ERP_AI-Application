using GCTL.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Finance.BaseAccountVM
{
    public class CreateBaseAccountVM : BaseViewModel
    {
        public int BaseAccountID { get; set; }

        [Required(ErrorMessage = "{0} is required!"), Display(Name = "Base Account Code")]
        public string BaseAccountCode { get; set; }

        [Required(ErrorMessage = "{0} is required!"), Display(Name = "Base Account Name")]
        public string BaseAccountName { get; set; }

        public string? Description { get; set; }
    }
}
