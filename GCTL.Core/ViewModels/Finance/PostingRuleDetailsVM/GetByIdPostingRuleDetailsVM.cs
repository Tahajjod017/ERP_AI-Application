using GCTL.Core.ViewModels.Finance.AddJournalVM;
using GCTL.Core.ViewModels.Finance.PostingRulesVM;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Finance.PostingRuleDetailsVM
{
    public class GetByIdPostingRuleDetailsVM
    {
        public int? PostingRuleDetailID { get; set; }

        public int? MainAccountID { get; set; }

        public int? SubAccID { get; set; }

        public int? TrxAccID { get; set; }

        public string? DebitCredit { get; set; }

        public GetByPostingRuleIdVM? PostingRuleVm { get; set; }
    }
}
