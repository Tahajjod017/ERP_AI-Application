using GCTL.Core.ViewModels.Finance.PostingRuleDetailsVM;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Finance.PostingRulesVM
{
    public class CreatePostingRulesVM
    {
        public int? PostingRuleID { get; set; }

        [Required(ErrorMessage = "{0} is Required!"), Display(Name = "Scenario Code")]
        public string ScenarioCode { get; set; }

        [Required(ErrorMessage = "{0} is Required!"), Display(Name = "Scenario Name")]
        public string ScenarioName { get; set; }

        public bool? IsActive { get; set; }

        public IList<CreatePostingRuleDetailsVM>? PostingRuleDetailsVMs { get; set; } = new List<CreatePostingRuleDetailsVM>();
    }
}
