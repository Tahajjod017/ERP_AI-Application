using GCTL.Core.ViewModels.Finance.PostingRuleDetailsVM;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Finance.PostingRulesVM
{
    public class GetByIdPostingRulesVM
    {
        public int? PostingRuleID { get; set; }

        public string? ScenarioCode { get; set; }

        public string? ScenarioName { get; set; }

        public bool? IsActive { get; set; }

        public IList<GetByIdPostingRulesDetailsVM>? PostingRuleDetailsVMs { get; set; } = new List<GetByIdPostingRulesDetailsVM>();
    }

    public class GetByIdPostingRulesDetailsVM
    {
        public int? PostingRuleDetailID { get; set; }

        public int? PostingRuleID { get; set; }

        public int? MainAccountID { get; set; }

        public int? SubAccID { get; set; }

        public int? TrxAccID { get; set; }

        public string? TrxType { get; set; }

        public GetByIdPostingRulesVM? GetByIdPostingRulesVMs { get; set; }
    }
}
