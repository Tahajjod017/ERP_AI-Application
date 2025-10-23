using GCTL.Core.ViewModels.Finance.PostingRulesVM;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Finance.PostingRuleDetailsVM
{
    public class CreatePostingRuleDetailsVM : BaseViewModel
    {
        public int PostingRuleDetailID { get; set; }

        [Required(ErrorMessage = "{0} is Required!"), Display(Name = "Main Account")]
        public int? MainAccountID { get; set; }

        [Required(ErrorMessage = "{0} is Required!"), Display(Name = "Sub Account")]
        public int? SubAccID { get; set; }

        public int? TrxAccID { get; set; }

        [Required(ErrorMessage = "{0} is Required!"), Display(Name = "Debit/Credit")]
        public string? DebitCredit { get; set; }

        public CreatePostingRulesVM? PostingRuleVm { get; set; }
    }
}
