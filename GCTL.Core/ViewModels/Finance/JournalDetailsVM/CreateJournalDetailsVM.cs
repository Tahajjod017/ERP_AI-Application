using GCTL.Core.ViewModels.Finance.AddJournalVM;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Finance.JournalDetailsVM
{
    public class CreateJournalDetailsVM : BaseViewModel
    {
        public int JournalDetailID { get; set; }

        public int? MainAccountID { get; set; }

        public int? SubAccountID { get; set; }

        public int? TransactionAccountID { get; set; }

        public int? JournalID { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "{0} is Required!"), Display(Name = "Transaction Type")]
        public string? TrxType { get; set; }

        [Required(ErrorMessage = "{0} is Required!"), Display(Name = "Amount")]
        public decimal? Amount { get; set; }

        public CreateAddJournalVM? CreateAddJournalVM { get; set; }
    }
}
