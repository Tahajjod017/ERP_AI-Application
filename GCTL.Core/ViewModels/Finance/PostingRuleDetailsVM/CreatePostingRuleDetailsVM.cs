using GCTL.Core.ViewModels.Finance.PostingRulesVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Finance.PostingRuleDetailsVM
{
    public class CreatePostingRuleDetailsVM
    {
        public int PostingRuleDetailID { get; set; }

        public int? PostingRuleID { get; set; }

        public int? SubDebitAccountID { get; set; }

        public int? SubCreditAccountID { get; set; }

        public int? TrxDebitAccountID { get; set; }

        public int? TrxCreditAccountID { get; set; }

        public bool? IsActive { get; set; }

        public CreatePostingRulesVM? PostingRuleVm { get; set; }
    }
}
