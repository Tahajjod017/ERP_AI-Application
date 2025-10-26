using GCTL.Core.ViewModels.Finance.PostingRuleDetailsVM;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Finance.AddJournalVM
{
    public class GetByPostingRuleIdVM
    {
        public int? PostingRuleID { get; set; }
        
        public string? ScenarioCode { get; set; }

        public string? ScenarioName { get; set; }

        public IList<GetByIdPostingRuleDetailsVM> GetByIdPostingRuleDetailsVMs { get; set; } = new List<GetByIdPostingRuleDetailsVM>();
    }
}
