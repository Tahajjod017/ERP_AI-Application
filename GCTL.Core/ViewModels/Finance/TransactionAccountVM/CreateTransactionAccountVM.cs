using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Finance.TransactionAccountVM
{
    public class CreateTransactionAccountVM : BaseViewModel
    {
        public int TrxAccID { get; set; }

        [Required(ErrorMessage = "Select a {0}!"), Display(Name = "Class Name")]
        public int? ClassID { get; set; }

        [Required(ErrorMessage = "{0} is required!"), Display(Name = "Main Account")]
        public int? MainAccountID { get; set; }

        [Required(ErrorMessage = "{0} is required!"), Display(Name = "Sub Account Name")]
        public int? SubAccountID { get; set; }

        [Required(ErrorMessage = "{0} is required!"), StringLength(12, ErrorMessage = "{0} must be at most {1} characters long."), Display(Name = "Transaction Account Code")]
        [RegularExpression(@"^.{12}$", ErrorMessage = "{0} must be exactly 12 characters long.")]
        public string TrxAccCode { get; set; }

        [Required(ErrorMessage = "{0} is required!"), StringLength(100, ErrorMessage = "{0} must be at most {1} characters long."), Display(Name = "Transaction Account Name")]
        public string TrxAccName { get; set; }

        [Display(Name = "Is Active?")]
        public bool IsActive { get; set; }

        [StringLength(200, ErrorMessage = "{0} must be at most {1} characters long."), Display(Name = "Description")]
        public string? Description { get; set; }
    }
}
