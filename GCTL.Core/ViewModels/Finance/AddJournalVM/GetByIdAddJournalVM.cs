using GCTL.Core.ViewModels.Finance.JournalDetailsVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Finance.AddJournalVM
{
    public class GetByIdAddJournalVM : BaseViewModel
    {
        public int? JournalID { get; set; }

        public int? JournalTypeID { get; set; }

        public string? JournalCode { get; set; }

        public int? PostingRuleID { get; set; }

        public DateTime? JournalDate { get; set; }

        public string? Note { get; set; }

        public string? FileLink { get; set; }

        public IList<CreateJournalDetailsVM?> GetByIdJournalDetailsVMs { get; set; } = new List<CreateJournalDetailsVM?>();
    }
}
