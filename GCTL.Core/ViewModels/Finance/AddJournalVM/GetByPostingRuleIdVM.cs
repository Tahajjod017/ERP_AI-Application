using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Finance.AddJournalVM
{
    public class GetByPostingRuleIdVM
    {
        public int? PostingRuleID { get; set; }
        public int? MainAccountID { get; set; }
        public int? SubAccountID { get; set; }
        public int? TrxAccountID { get; set; }
    }
}
