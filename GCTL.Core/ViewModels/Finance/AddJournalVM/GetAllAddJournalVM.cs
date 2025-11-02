using GCTL.Core.ViewModels.Finance.JournalDetailsVM;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Finance.AddJournalVM
{
    public class GetAllAddJournalVM : BaseViewModel
    {
        public int? JournalID { get; set; }

        public string? JournalType { get; set; }

        public string? JournalCode { get; set; }

        public string? PostingRule { get; set; }

        public string? JournalDate { get; set; }

        public string? Note { get; set; }

        public string? FileLink { get; set; }

        public IList<CreateJournalDetailsVM?> GetAllJournalDetailsVMs { get; set; } = new List<CreateJournalDetailsVM?>();
    }
}
