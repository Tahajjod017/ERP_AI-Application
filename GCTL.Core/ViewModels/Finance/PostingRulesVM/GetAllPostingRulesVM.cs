using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Finance.PostingRulesVM
{
    public class GetAllPostingRulesVM
    {
        public int? PostingRuleID { get; set; }

        public string? ScenarioCode { get; set; }

        public string? ScenarioName { get; set; }
    }
}
